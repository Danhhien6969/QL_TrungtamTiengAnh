using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL
{
    public class GiaoVienDAL
    {
        // Bảng danh sách KHÔNG tải ảnh nhị phân để giữ giao diện nhanh.
        private static IQueryable<GiaoVien> KhongLayAnh(IQueryable<GiaoVien> query)
        {
            return query.Select(g => new GiaoVien
            {
                MaGV = g.MaGV,
                HoTen = g.HoTen,
                NgaySinh = g.NgaySinh,
                GioiTinh = g.GioiTinh,
                SoDienThoai = g.SoDienThoai,
                Email = g.Email,
                ChuyenMon = g.ChuyenMon,
                NgayVaoLam = g.NgayVaoLam,
                TrangThai = g.TrangThai,
                DiaChi = g.DiaChi,
                GhiChu = g.GhiChu,
                TrinhDoHocVan = g.TrinhDoHocVan,
                XepLoaiTotNghiep = g.XepLoaiTotNghiep
            });
        }

        public List<GiaoVien> LayDanhSach()
        {
            using var db = new TrungTamDbContext();
            return KhongLayAnh(db.GiaoViens.AsNoTracking())
                .OrderByDescending(g => g.MaGV)
                .ToList();
        }

        // Khi chọn một giáo viên mới lấy ảnh chứng chỉ của người đó.
        public GiaoVien? LayTheoMa(int maGV)
        {
            using var db = new TrungTamDbContext();
            return db.GiaoViens.AsNoTracking()
                .FirstOrDefault(g => g.MaGV == maGV);
        }

        public List<GiaoVien> TimKiem(string tuKhoa)
        {
            tuKhoa = (tuKhoa ?? string.Empty).Trim();
            if (tuKhoa.Length == 0)
                return LayDanhSach();

            using var db = new TrungTamDbContext();
            var query = db.GiaoViens.AsNoTracking().Where(g =>
                g.HoTen.Contains(tuKhoa) ||
                (g.SoDienThoai != null && g.SoDienThoai.Contains(tuKhoa)) ||
                (g.Email != null && g.Email.Contains(tuKhoa)) ||
                (g.ChuyenMon != null && g.ChuyenMon.Contains(tuKhoa)) ||
                (g.DiaChi != null && g.DiaChi.Contains(tuKhoa)) ||
                (g.TrinhDoHocVan != null && g.TrinhDoHocVan.Contains(tuKhoa)) ||
                (g.XepLoaiTotNghiep != null && g.XepLoaiTotNghiep.Contains(tuKhoa)));

            return KhongLayAnh(query)
                .OrderByDescending(g => g.MaGV)
                .ToList();
        }

        public int Them(GiaoVien giaoVien)
        {
            using var db = new TrungTamDbContext();
            db.GiaoViens.Add(giaoVien);
            db.SaveChanges();
            return giaoVien.MaGV;
        }

        public bool Sua(GiaoVien giaoVien)
        {
            using var db = new TrungTamDbContext();
            var hienTai = db.GiaoViens.FirstOrDefault(g => g.MaGV == giaoVien.MaGV);
            if (hienTai == null)
                return false;

            hienTai.HoTen = giaoVien.HoTen;
            hienTai.NgaySinh = giaoVien.NgaySinh;
            hienTai.GioiTinh = giaoVien.GioiTinh;
            hienTai.SoDienThoai = giaoVien.SoDienThoai;
            hienTai.Email = giaoVien.Email;
            hienTai.ChuyenMon = giaoVien.ChuyenMon;
            hienTai.NgayVaoLam = giaoVien.NgayVaoLam;
            hienTai.TrangThai = giaoVien.TrangThai;
            hienTai.DiaChi = giaoVien.DiaChi;
            hienTai.GhiChu = giaoVien.GhiChu;
            hienTai.TrinhDoHocVan = giaoVien.TrinhDoHocVan;
            hienTai.XepLoaiTotNghiep = giaoVien.XepLoaiTotNghiep;
            hienTai.AnhChungChi = giaoVien.AnhChungChi;

            db.SaveChanges();
            return true;
        }

        public bool Xoa(int maGV)
        {
            using var db = new TrungTamDbContext();
            var giaoVien = db.GiaoViens.FirstOrDefault(g => g.MaGV == maGV);
            if (giaoVien == null)
                return false;

            if (db.LopHocs.Any(l => l.MaGV == maGV))
                throw new InvalidOperationException(
                    "Giáo viên đã được phân công lớp học. " +
                    "Hãy chuyển trạng thái sang Ngừng hoạt động thay vì xóa.");

            db.GiaoViens.Remove(giaoVien);
            db.SaveChanges();
            return true;
        }
    }
}
