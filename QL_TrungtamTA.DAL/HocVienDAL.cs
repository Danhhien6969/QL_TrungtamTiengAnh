using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL
{
    public class HocVienDAL
    {
        public List<HocVien> LayDanhSach()
        {
            using var db = new TrungTamDbContext();
            return db.HocViens.AsNoTracking()
                .Where(h => h.TrangThai)
                .OrderByDescending(h => h.MaHV)
                .ToList();
        }

        public HocVien? LayTheoMa(int maHV)
        {
            using var db = new TrungTamDbContext();
            return db.HocViens.AsNoTracking()
                .FirstOrDefault(h => h.MaHV == maHV && h.TrangThai);
        }

        public List<HocVien> TimKiem(string tuKhoa)
        {
            tuKhoa = (tuKhoa ?? string.Empty).Trim();
            if (tuKhoa.Length == 0)
                return LayDanhSach();

            using var db = new TrungTamDbContext();
            return db.HocViens.AsNoTracking()
                .Where(h => h.TrangThai &&
                    (h.HoTen.Contains(tuKhoa)
                    || (h.SoDienThoai != null && h.SoDienThoai.Contains(tuKhoa))
                    || (h.HoTenPhuHuynh != null && h.HoTenPhuHuynh.Contains(tuKhoa))
                    || (h.SoDienThoaiPhuHuynh != null && h.SoDienThoaiPhuHuynh.Contains(tuKhoa))
                    || (h.LopTruong != null && h.LopTruong.Contains(tuKhoa))))
                .OrderByDescending(h => h.MaHV)
                .ToList();
        }


        // 3a. Xem danh sách học viên đã chuyển vào thùng rác.
        public List<HocVien> LayDanhSachDaXoa()
        {
            using var db = new TrungTamDbContext();
            return db.HocViens.AsNoTracking()
                .Where(h => !h.TrangThai)
                .OrderByDescending(h => h.MaHV)
                .ToList();
        }

        // 3b. Tìm kiếm trong thùng rác; không tìm lẫn học viên đang học.
        public List<HocVien> TimKiemDaXoa(string tuKhoa)
        {
            tuKhoa = (tuKhoa ?? string.Empty).Trim();
            if (tuKhoa.Length == 0)
                return LayDanhSachDaXoa();

            using var db = new TrungTamDbContext();
            return db.HocViens.AsNoTracking()
                .Where(h => !h.TrangThai &&
                    (h.HoTen.Contains(tuKhoa)
                    || (h.SoDienThoai != null && h.SoDienThoai.Contains(tuKhoa))
                    || (h.HoTenPhuHuynh != null && h.HoTenPhuHuynh.Contains(tuKhoa))
                    || (h.SoDienThoaiPhuHuynh != null && h.SoDienThoaiPhuHuynh.Contains(tuKhoa))
                    || (h.LopTruong != null && h.LopTruong.Contains(tuKhoa))))
                .OrderByDescending(h => h.MaHV)
                .ToList();
        }

        public int Them(HocVien hocVien)
        {
            using var db = new TrungTamDbContext();
            db.HocViens.Add(hocVien);
            db.SaveChanges();
            return hocVien.MaHV;
        }

        public bool Sua(HocVien hocVien)
        {
            using var db = new TrungTamDbContext();
            var cu = db.HocViens.FirstOrDefault(h => h.MaHV == hocVien.MaHV && h.TrangThai);
            if (cu == null)
                return false;

            cu.HoTen = hocVien.HoTen;
            cu.Tuoi = hocVien.Tuoi;
            cu.LopTruong = hocVien.LopTruong;
            cu.NgaySinh = hocVien.NgaySinh;
            cu.GioiTinh = hocVien.GioiTinh;
            cu.SoDienThoai = hocVien.SoDienThoai;
            cu.DiaChi = hocVien.DiaChi;
            cu.HoTenPhuHuynh = hocVien.HoTenPhuHuynh;
            cu.SoDienThoaiPhuHuynh = hocVien.SoDienThoaiPhuHuynh;

            // KHÔNG thay Email, TrangThai, NgayDangKy, MaHV cũ.
            // Các phân hệ khác có thể vẫn sử dụng các trường này.
            db.SaveChanges();
            return true;
        }

        // 6. XÓA MỀM: Không xóa khỏi SQL, không làm mất các liên kết khác.
        public bool Xoa(int maHV)
        {
            using var db = new TrungTamDbContext();
            var hv = db.HocViens.FirstOrDefault(h => h.MaHV == maHV && h.TrangThai);
            if (hv == null)
                return false;

            hv.TrangThai = false;
            db.SaveChanges();
            return true;
        }

        // 7. KHÔI PHỤC: Giữ nguyên mã MaHV và mọi thông tin học viên.
        public bool KhoiPhuc(int maHV)
        {
            using var db = new TrungTamDbContext();
            var hv = db.HocViens.FirstOrDefault(h => h.MaHV == maHV && !h.TrangThai);
            if (hv == null)
                return false;

            hv.TrangThai = true;
            db.SaveChanges();
            return true;
        }
    }
}
