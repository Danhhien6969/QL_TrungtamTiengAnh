
using System;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL
{
    public class TaiKhoanDAL
    {
        // Tìm tài khoản theo tên đăng nhập
        public TaiKhoan? TimTheoTenDangNhap(
            string tenDangNhap)
        {
            using var db = new TrungTamDbContext();

            return db.TaiKhoans
                .AsNoTracking()
                .Include(t => t.VaiTro)
                .SingleOrDefault(t =>
                    t.TenDangNhap == tenDangNhap);
        }

        // Kiểm tra hệ thống đã có tài khoản chưa
        public bool DaCoTaiKhoan()
        {
            using var db = new TrungTamDbContext();

            return db.TaiKhoans.Any();
        }

        // Tạo tài khoản Admin lần đầu
        public void TaoAdminDauTien(
            string tenDangNhap,
            string matKhauHash)
        {
            using var db = new TrungTamDbContext();

            using var transaction =
                db.Database.BeginTransaction(
                    IsolationLevel.Serializable);

            if (db.TaiKhoans.Any())
            {
                throw new InvalidOperationException(
                    "Hệ thống đã có tài khoản.");
            }

            var admin = db.VaiTros
                .SingleOrDefault(v =>
                    v.TenVaiTro == "Admin");

            if (admin == null)
            {
                admin = new VaiTro
                {
                    TenVaiTro = "Admin",
                    MoTa = "Quản trị viên hệ thống"
                };

                db.VaiTros.Add(admin);
            }

            if (!db.VaiTros.Any(v =>
                v.TenVaiTro == "NhanVien"))
            {
                db.VaiTros.Add(new VaiTro
                {
                    TenVaiTro = "NhanVien",
                    MoTa = "Nhân viên trung tâm"
                });
            }

            db.TaiKhoans.Add(new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhauHash = matKhauHash,
                VaiTro = admin,
                TrangThai = true,
                NgayTao = DateTime.UtcNow
            });

            db.SaveChanges();
            transaction.Commit();
        }
    }
}
