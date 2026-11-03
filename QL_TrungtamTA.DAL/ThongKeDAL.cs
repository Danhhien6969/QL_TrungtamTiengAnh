using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;

namespace QL_TrungtamTA.DAL
{
    // DTO chỉ dùng cho báo cáo, không tạo thêm bảng trong Database.
    public sealed class DoanhThuTheoThang
    {
        public int Thang { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoGiaoDich { get; set; }
    }

    public sealed class KhoaHocNhieuDangKy
    {
        public string TenKhoaHoc { get; set; } = string.Empty;
        public int SoDangKy { get; set; }
    }

    public sealed class DuLieuThongKe
    {
        public int Nam { get; set; }
        public int TongHocVien { get; set; }
        public int TongGiaoVien { get; set; }
        public int TongKhoaHoc { get; set; }
        public int TongLopHoc { get; set; }
        public int TongDangKyHopLe { get; set; }
        public decimal DoanhThuTrongNam { get; set; }
        public decimal CongNoHienTai { get; set; }
        public int SoGiaoDichTrongNam { get; set; }
        public List<DoanhThuTheoThang> DoanhThu12Thang { get; set; } = new();
        public List<KhoaHocNhieuDangKy> TopKhoaHoc { get; set; } = new();
    }

    public class ThongKeDAL
    {
        public DuLieuThongKe LayThongKe(int nam)
        {
            DateTime tuNgay = new DateTime(nam, 1, 1);
            DateTime denNgay = tuNgay.AddYears(1); // Mốc kết thúc không bao gồm.

            using var db = new TrungTamDbContext();

            var baoCao = new DuLieuThongKe
            {
                Nam = nam,
                TongHocVien = db.HocViens.Count(),
                TongGiaoVien = db.GiaoViens.Count(),
                TongKhoaHoc = db.KhoaHocs.Count(),
                TongLopHoc = db.LopHocs.Count(),
                TongDangKyHopLe = db.DangKyHocs.Count(d =>
                    d.TrangThai == "DangHoc" || d.TrangThai == "HoanThanh")
            };

            // Doanh thu là tổng tiền ĐÃ THU trong năm, không phải tổng học phí ghi danh.
            var thanhToanNam = db.ThanhToanHocPhis.AsNoTracking()
                .Where(t => t.NgayThanhToan >= tuNgay && t.NgayThanhToan < denNgay);

            baoCao.DoanhThuTrongNam = thanhToanNam
                .Sum(t => (decimal?)t.SoTien) ?? 0m;
            baoCao.SoGiaoDichTrongNam = thanhToanNam.Count();

            var tongHopThang = thanhToanNam
                .GroupBy(t => t.NgayThanhToan.Month)
                .Select(g => new
                {
                    Thang = g.Key,
                    DoanhThu = g.Sum(t => t.SoTien),
                    SoGiaoDich = g.Count()
                }).ToList();

            for (int thang = 1; thang <= 12; thang++)
            {
                var muc = tongHopThang.FirstOrDefault(x => x.Thang == thang);
                baoCao.DoanhThu12Thang.Add(new DoanhThuTheoThang
                {
                    Thang = thang,
                    DoanhThu = muc?.DoanhThu ?? 0m,
                    SoGiaoDich = muc?.SoGiaoDich ?? 0
                });
            }

            // Công nợ tại thời điểm truy vấn: chỉ các đăng ký đang học/hoàn thành.
            // Khoản thanh toán của đăng ký đã hủy vẫn nằm trong doanh thu ghi nhận,
            // nhưng đăng ký đã hủy không tính vào học phí còn phải thu.
            var dangKyConHieuLuc = db.DangKyHocs.AsNoTracking()
                .Where(d => d.TrangThai == "DangHoc" || d.TrangThai == "HoanThanh")
                .Select(d => new
                {
                    d.HocPhiPhaiDong,
                    DaThanhToan = d.ThanhToanHocPhis.Sum(t => (decimal?)t.SoTien) ?? 0m
                })
                .ToList();

            baoCao.CongNoHienTai = dangKyConHieuLuc.Sum(d =>
                Math.Max(0m, d.HocPhiPhaiDong - d.DaThanhToan));

            // Top 5 khóa học có nhiều đăng ký đang học hoặc đã hoàn thành.
            // Bảng này là số liệu tích lũy, không lọc theo năm báo cáo.
            baoCao.TopKhoaHoc = db.DangKyHocs.AsNoTracking()
                .Where(d => d.TrangThai == "DangHoc" || d.TrangThai == "HoanThanh")
                .GroupBy(d => new { d.LopHoc.MaKH, d.LopHoc.KhoaHoc.TenKH })
                .Select(g => new KhoaHocNhieuDangKy
                {
                    TenKhoaHoc = g.Key.TenKH,
                    SoDangKy = g.Count()
                })
                .OrderByDescending(x => x.SoDangKy)
                .ThenBy(x => x.TenKhoaHoc)
                .Take(5)
                .ToList();

            return baoCao;
        }
    }
}
