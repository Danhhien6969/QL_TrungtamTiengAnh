using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL
{
    // Đối tượng chỉ dùng để hiển thị trên ComboBox và DataGridView.
    public sealed class LuaChonLopDiemDanh
    {
        public int MaLop { get; set; }
        public string HienThi { get; set; } = "";
    }

    public sealed class DongBuoiHoc
    {
        public int MaBuoi { get; set; }
        public int MaLop { get; set; }
        public string TenLop { get; set; } = "";
        public DateTime NgayHoc { get; set; }
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public string NoiDung { get; set; } = "";
        public string TrangThai { get; set; } = "";
        public int DaGhiNhan { get; set; }
        public string KhungGio => GioBatDau.ToString(@"hh\:mm") + " - " + GioKetThuc.ToString(@"hh\:mm");
        public string HienThiTrangThai => TrangThai switch
        {
            "DuKien" => "Dự kiến",
            "DaDienRa" => "Đã điểm danh",
            "DaHuy" => "Đã hủy",
            _ => TrangThai
        };
    }

    // Trạng thái ChuaDiemDanh chỉ dùng trên giao diện, không ghi xuống SQL.
    public sealed class DongDiemDanh
    {
        public int MaHV { get; set; }
        public string HoTen { get; set; } = "";
        public string SoDienThoai { get; set; } = "";
        public string TrangThai { get; set; } = "ChuaDiemDanh";
        public string GhiChu { get; set; } = "";
    }

    public sealed class CapNhatDiemDanh
    {
        public int MaHV { get; set; }
        public string TrangThai { get; set; } = "";
        public string? GhiChu { get; set; }
    }

    public class DiemDanhDAL
    {
        // Sức chứa được quản lý tập trung ở Phòng học, không lấy từ Khóa học.
        // Trả về null nếu lớp chưa có phòng mặc định hoặc phòng không tồn tại.
        public int? LaySucChuaPhongCuaLop(int maLop)
        {
            if (maLop <= 0)
                throw new ArgumentException("Mã lớp không hợp lệ.");

            using var db = new TrungTamDbContext();
            return (from lop in db.LopHocs.AsNoTracking()
                    join phong in db.PhongHocs.AsNoTracking()
                        on lop.MaPhongMacDinh equals phong.MaPhong
                    where lop.MaLop == maLop
                    select (int?)phong.SucChua)
                .FirstOrDefault();
        }

        public List<LuaChonLopDiemDanh> LayLopHocs()
        {
            using var db = new TrungTamDbContext();
            return db.LopHocs.AsNoTracking()
                .OrderBy(l => l.TenLop)
                .Select(l => new LuaChonLopDiemDanh
                {
                    MaLop = l.MaLop,
                    HienThi = l.TenLop + " - " + l.KhoaHoc.TenKH
                })
                .ToList();
        }

        public List<DongBuoiHoc> LayBuoiHocs(int? maLop = null)
        {
            using var db = new TrungTamDbContext();
            var query = db.BuoiHocs.AsNoTracking().AsQueryable();
            if (maLop.HasValue)
                query = query.Where(b => b.MaLop == maLop.Value);

            return query
                .OrderByDescending(b => b.NgayHoc)
                .ThenBy(b => b.GioBatDau)
                .Select(b => new DongBuoiHoc
                {
                    MaBuoi = b.MaBuoi,
                    MaLop = b.MaLop,
                    TenLop = b.LopHoc.TenLop,
                    NgayHoc = b.NgayHoc,
                    GioBatDau = b.GioBatDau,
                    GioKetThuc = b.GioKetThuc,
                    NoiDung = b.NoiDung ?? "",
                    TrangThai = b.TrangThai,
                    DaGhiNhan = b.DiemDanhs.Count()
                })
                .ToList();
        }

        public int TaoBuoiHoc(int maLop, DateTime ngayHoc,
            TimeSpan gioBatDau, TimeSpan gioKetThuc, string? noiDung)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);

            var lop = db.LopHocs.AsNoTracking()
                .FirstOrDefault(l => l.MaLop == maLop);
            if (lop == null)
                throw new InvalidOperationException("Không tìm thấy lớp học.");
            if (lop.TrangThai == "DaHuy" || lop.TrangThai == "DaKetThuc")
                throw new InvalidOperationException("Không thể tạo buổi mới cho lớp đã hủy hoặc kết thúc.");
            if (ngayHoc.Date < lop.NgayBatDau.Date || ngayHoc.Date > lop.NgayKetThuc.Date)
                throw new InvalidOperationException("Ngày học phải thuộc khoảng thời gian của lớp.");

            // Index UNIQUE (MaLop, NgayHoc, GioBatDau) áp dụng cả buổi đã hủy.
            if (db.BuoiHocs.Any(b => b.MaLop == maLop &&
                b.NgayHoc == ngayHoc.Date && b.GioBatDau == gioBatDau))
                throw new InvalidOperationException(
                    "Đã tồn tại buổi học cùng ngày và giờ bắt đầu, kể cả buổi đã hủy.");

            // Ngăn hai buổi chưa hủy của cùng lớp bị chồng khoảng thời gian.
            bool biTrung = db.BuoiHocs.Any(b =>
                b.MaLop == maLop && b.NgayHoc == ngayHoc.Date &&
                b.TrangThai != "DaHuy" &&
                b.GioBatDau < gioKetThuc && b.GioKetThuc > gioBatDau);
            if (biTrung)
                throw new InvalidOperationException("Lớp đã có buổi học bị trùng thời gian.");

            var buoi = new BuoiHoc
            {
                MaLop = maLop,
                NgayHoc = ngayHoc.Date,
                GioBatDau = gioBatDau,
                GioKetThuc = gioKetThuc,
                NoiDung = noiDung,
                TrangThai = "DuKien"
            };
            db.BuoiHocs.Add(buoi);
            db.SaveChanges();
            tx.Commit();
            return buoi.MaBuoi;
        }

        // Lấy học viên đã đăng ký lớp từ ngày của buổi học trở về trước.
        // Nếu đăng ký đã hủy nhưng từng được điểm danh, vẫn giữ dòng lịch sử.
        private static IQueryable<DangKyHoc> TaoTruyVanHocVien(
            TrungTamDbContext db, BuoiHoc buoi)
        {
            return db.DangKyHocs.AsNoTracking().Where(d =>
                d.MaLop == buoi.MaLop && d.NgayDangKy.Date <= buoi.NgayHoc.Date &&
                (d.TrangThai != "DaHuy" ||
                 db.DiemDanhs.Any(dd => dd.MaBuoi == buoi.MaBuoi && dd.MaHV == d.MaHV)));
        }

        public List<DongDiemDanh> LayDiemDanh(int maBuoi)
        {
            using var db = new TrungTamDbContext();
            var buoi = db.BuoiHocs.AsNoTracking()
                .FirstOrDefault(b => b.MaBuoi == maBuoi);
            if (buoi == null)
                throw new InvalidOperationException("Buổi học không tồn tại.");

            // Một học viên có tối đa một đăng ký/lớp (index UNIQUE trong DB).
            return TaoTruyVanHocVien(db, buoi)
                .OrderBy(d => d.HocVien.HoTen)
                .Select(d => new DongDiemDanh
                {
                    MaHV = d.MaHV,
                    HoTen = d.HocVien.HoTen,
                    SoDienThoai = d.HocVien.SoDienThoai ?? "",
                    TrangThai = db.DiemDanhs
                        .Where(dd => dd.MaBuoi == maBuoi && dd.MaHV == d.MaHV)
                        .Select(dd => dd.TrangThai)
                        .FirstOrDefault() ?? "ChuaDiemDanh",
                    GhiChu = db.DiemDanhs
                        .Where(dd => dd.MaBuoi == maBuoi && dd.MaHV == d.MaHV)
                        .Select(dd => dd.GhiChu)
                        .FirstOrDefault() ?? ""
                })
                .ToList();
        }

        public int LuuDiemDanh(int maBuoi, List<CapNhatDiemDanh> danhSach)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            var buoi = db.BuoiHocs.FirstOrDefault(b => b.MaBuoi == maBuoi);
            if (buoi == null)
                throw new InvalidOperationException("Buổi học không tồn tại.");
            if (buoi.TrangThai == "DaHuy")
                throw new InvalidOperationException("Buổi học đã hủy, không thể điểm danh.");

            var hopLe = TaoTruyVanHocVien(db, buoi)
                .Select(d => d.MaHV).ToHashSet();
            var ids = danhSach.Select(d => d.MaHV).ToList();
            if (ids.Count != ids.Distinct().Count() ||
                ids.Count != hopLe.Count || ids.Any(id => !hopLe.Contains(id)))
                throw new InvalidOperationException(
                    "Danh sách học viên đã thay đổi. Hãy tải lại trước khi lưu.");

            var daCo = db.DiemDanhs.Where(d => d.MaBuoi == maBuoi).ToList();
            foreach (var item in danhSach)
            {
                var diemDanh = daCo.FirstOrDefault(d => d.MaHV == item.MaHV);
                if (diemDanh == null)
                {
                    db.DiemDanhs.Add(new DiemDanh
                    {
                        MaBuoi = maBuoi,
                        MaHV = item.MaHV,
                        TrangThai = item.TrangThai,
                        GhiChu = item.GhiChu
                    });
                }
                else
                {
                    diemDanh.TrangThai = item.TrangThai;
                    diemDanh.GhiChu = item.GhiChu;
                }
            }

            buoi.TrangThai = "DaDienRa";
            db.SaveChanges();
            tx.Commit();
            return danhSach.Count;
        }

        // Chỉ cho hủy buổi chưa điểm danh để không làm mất ý nghĩa lịch sử.
        public bool HuyBuoiHoc(int maBuoi)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            var buoi = db.BuoiHocs.FirstOrDefault(b => b.MaBuoi == maBuoi);
            if (buoi == null) return false;
            if (buoi.TrangThai == "DaHuy")
                throw new InvalidOperationException("Buổi học đã được hủy.");
            if (db.DiemDanhs.Any(d => d.MaBuoi == maBuoi))
                throw new InvalidOperationException(
                    "Buổi học đã có điểm danh; không thể hủy. Cần xử lý dữ liệu trước.");
            buoi.TrangThai = "DaHuy";
            db.SaveChanges();
            tx.Commit();
            return true;
        }
    }
}
