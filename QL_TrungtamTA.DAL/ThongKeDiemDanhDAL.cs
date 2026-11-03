using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;

namespace QL_TrungtamTA.DAL
{
    public sealed class DongTheoDoiDiemDanh
    {
        public int MaLop { get; set; }
        public int? MaBuoi { get; set; }
        public string TenLop { get; set; } = "";
        public string GiaoVien { get; set; } = "";
        public string SiSo { get; set; } = "";
        public string Phong { get; set; } = "";
        public string NgayHoc { get; set; } = "";
        public string GioHoc { get; set; } = "";
        public string CoMat { get; set; } = "—";
        public string Vang { get; set; } = "—";
        public string CoPhep { get; set; } = "—";
        public string DiTre { get; set; } = "—";
        public string ChuaDiemDanh { get; set; } = "—";
        public string TrangThai { get; set; } = "Chưa tạo buổi";
    }

    public sealed class ChiTietVang
    {
        public string HocVien { get; set; } = "";
        public string TrangThai { get; set; } = "";
        public string GhiChu { get; set; } = "";
    }

    public sealed class ThongKeDiemDanhDAL
    {
        public List<DongTheoDoiDiemDanh> LayTheoNgay(DateTime ngay)
        {
            using var db = new TrungTamDbContext();
            var lop = db.LopHocs.AsNoTracking().Select(l => new
            {
                l.MaLop,
                l.TenLop,
                l.MaPhongMacDinh,
                GiaoVien = l.GiaoVien.HoTen
            }).ToList();
            var phong = db.PhongHocs.AsNoTracking()
                .Select(p => new { p.MaPhong, p.TenPhong, p.SucChua }).ToList()
                .ToDictionary(p => p.MaPhong);
            var dangKy = db.DangKyHocs.AsNoTracking()
                .Where(d => d.TrangThai != "DaHuy")
                .GroupBy(d => d.MaLop)
                .Select(g => new { MaLop = g.Key, SoLuong = g.Count() })
                .ToList().ToDictionary(x => x.MaLop, x => x.SoLuong);
            // Mỗi dòng ứng với một buổi cụ thể, tránh cộng nhiều buổi thành sai sĩ số.
            var buoi = db.BuoiHocs.AsNoTracking()
                .Where(b => b.NgayHoc == ngay.Date)
                .Select(b => new
                {
                    b.MaBuoi,
                    b.MaLop,
                    b.GioBatDau,
                    b.GioKetThuc,
                    b.TrangThai
                }).ToList();
            var ids = buoi.Select(b => b.MaBuoi).ToList();
            var diemDanh = db.DiemDanhs.AsNoTracking()
                .Where(d => ids.Contains(d.MaBuoi))
                .Select(d => new { d.MaBuoi, d.TrangThai })
                .ToList().GroupBy(d => d.MaBuoi)
                .ToDictionary(g => g.Key, g => g.ToList());

            var kq = new List<DongTheoDoiDiemDanh>();
            foreach (var l in lop.OrderBy(l => l.TenLop))
            {
                var coPhong = l.MaPhongMacDinh.HasValue &&
                              phong.TryGetValue(l.MaPhongMacDinh.Value, out _);
                var p = coPhong ? phong[l.MaPhongMacDinh!.Value] : null;
                int daDangKy = dangKy.TryGetValue(l.MaLop, out int sl) ? sl : 0;
                var danhSachBuoi = buoi.Where(b => b.MaLop == l.MaLop)
                    .OrderBy(b => b.GioBatDau).ToList();
                if (danhSachBuoi.Count == 0)
                {
                    kq.Add(new DongTheoDoiDiemDanh
                    {
                        MaLop = l.MaLop,
                        TenLop = l.TenLop,
                        GiaoVien = l.GiaoVien,
                        Phong = p?.TenPhong ?? "Chưa chọn",
                        SiSo = $"{daDangKy}/{p?.SucChua.ToString() ?? "—"}"
                    });
                    continue;
                }
                foreach (var b in danhSachBuoi)
                {
                    var row = new DongTheoDoiDiemDanh
                    {
                        MaLop = l.MaLop,
                        MaBuoi = b.MaBuoi,
                        TenLop = l.TenLop,
                        GiaoVien = l.GiaoVien,
                        Phong = p?.TenPhong ?? "Chưa chọn",
                        SiSo = $"{daDangKy}/{p?.SucChua.ToString() ?? "—"}",
                        NgayHoc = ngay.ToString("dd/MM/yyyy"),
                        GioHoc = b.GioBatDau.ToString(@"hh\:mm") + "–" + b.GioKetThuc.ToString(@"hh\:mm"),
                        TrangThai = b.TrangThai == "DaHuy" ? "Đã hủy" : "Chưa ghi nhận"
                    };
                    if (b.TrangThai != "DaHuy" &&
                        diemDanh.TryGetValue(b.MaBuoi, out var ds) && ds.Count > 0)
                    {
                        int coMat = ds.Count(d => d.TrangThai == "CoMat");
                        int vang = ds.Count(d => d.TrangThai == "Vang");
                        int phep = ds.Count(d => d.TrangThai == "CoPhep");
                        int tre = ds.Count(d => d.TrangThai == "DiTre");
                        row.CoMat = coMat.ToString();
                        row.Vang = vang.ToString();
                        row.CoPhep = phep.ToString();
                        row.DiTre = tre.ToString();
                        row.ChuaDiemDanh = Math.Max(0, daDangKy - ds.Count).ToString();
                        row.TrangThai = row.ChuaDiemDanh == "0" ? "Đã ghi nhận" : "Chưa đầy đủ";
                    }
                    kq.Add(row);
                }
            }
            return kq;
        }

        public List<ChiTietVang> LayHocVienVang(int maBuoi)
        {
            using var db = new TrungTamDbContext();
            return db.DiemDanhs.AsNoTracking()
                .Where(d => d.MaBuoi == maBuoi &&
                    (d.TrangThai == "Vang" || d.TrangThai == "CoPhep"))
                .OrderBy(d => d.HocVien.HoTen)
                .Select(d => new ChiTietVang
                {
                    HocVien = d.HocVien.HoTen,
                    TrangThai = d.TrangThai == "CoPhep" ? "Có phép" : "Vắng",
                    GhiChu = d.GhiChu ?? ""
                }).ToList();
        }
    }
}
