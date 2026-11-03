using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL
{
    public sealed class TaiKhoanHienThi
    {
        public int MaTK { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public int MaVT { get; set; }
        public string TenVaiTro { get; set; } = string.Empty;
        public int? MaGV { get; set; }
        public string TenGiaoVien { get; set; } = string.Empty;
        public bool TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
    }
    public sealed class LuaChonVaiTro
    {
        public int MaVT { get; set; }
        public string TenVaiTro { get; set; } = string.Empty;
    }
    public sealed class LuaChonGiaoVienTK
    {
        public int MaGV { get; set; }
        public string HienThi { get; set; } = string.Empty;
    }

    public class QuanLyTaiKhoanDAL
    {
        private static void DamBaoAdmin(TrungTamDbContext db, int maTK)
        {
            if (!db.TaiKhoans.Any(t => t.MaTK == maTK && t.TrangThai && t.VaiTro.TenVaiTro == "Admin"))
                throw new UnauthorizedAccessException("Chỉ Admin đang hoạt động được quản lý tài khoản.");
        }
        private static TaiKhoan TimTaiKhoan(TrungTamDbContext db, int maTK) =>
            db.TaiKhoans.Include(t => t.VaiTro).SingleOrDefault(t => t.MaTK == maTK)
            ?? throw new InvalidOperationException("Không tìm thấy tài khoản.");
        private static void KiemTraAdminCuoi(TrungTamDbContext db, TaiKhoan tk)
        {
            if (tk.TrangThai && tk.VaiTro.TenVaiTro == "Admin" &&
                db.TaiKhoans.Count(t => t.TrangThai && t.VaiTro.TenVaiTro == "Admin") <= 1)
                throw new InvalidOperationException("Không thể khóa hoặc hạ quyền Admin cuối cùng.");
        }
        private static string TenVaiTro(TrungTamDbContext db, int maVT) =>
            db.VaiTros.Where(v => v.MaVT == maVT)
                .Select(v => v.TenVaiTro).SingleOrDefault()
            ?? throw new ArgumentException("Vai trò không tồn tại.");
        private static int? KiemTraGiaoVien(TrungTamDbContext db, string tenVT, int? maGV, int? maTK = null)
        {
            if (tenVT != "GiaoVien")
            {
                if (maGV.HasValue) throw new ArgumentException("Chỉ tài khoản Giáo viên mới được gắn hồ sơ giáo viên.");
                return null;
            }
            if (maGV is null or <= 0)
                throw new ArgumentException("Hãy chọn giáo viên liên kết với tài khoản.");
            if (!db.GiaoViens.Any(g => g.MaGV == maGV && g.TrangThai))
                throw new ArgumentException("Giáo viên không tồn tại hoặc đã ngừng làm việc.");
            if (db.TaiKhoans.Any(t => t.MaGV == maGV && (!maTK.HasValue || t.MaTK != maTK)))
                throw new InvalidOperationException("Giáo viên này đã liên kết với tài khoản khác.");
            return maGV;
        }
        public List<TaiKhoanHienThi> LayDanhSach(int maAdmin, string tuKhoa = "")
        {
            using var db = new TrungTamDbContext();
            DamBaoAdmin(db, maAdmin);
            var q = db.TaiKhoans.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string x = tuKhoa.Trim();
                q = q.Where(t => t.TenDangNhap.Contains(x) || t.VaiTro.TenVaiTro.Contains(x) ||
                    (t.GiaoVien != null && t.GiaoVien.HoTen.Contains(x)));
            }
            return q.OrderBy(t => t.MaTK).Select(t => new TaiKhoanHienThi
            {
                MaTK = t.MaTK,
                TenDangNhap = t.TenDangNhap,
                MaVT = t.MaVT,
                TenVaiTro = t.VaiTro.TenVaiTro,
                MaGV = t.MaGV,
                TenGiaoVien = t.GiaoVien == null ? "" : t.GiaoVien.HoTen,
                TrangThai = t.TrangThai,
                NgayTao = t.NgayTao
            }).ToList();
        }
        public List<LuaChonVaiTro> LayVaiTro(int maAdmin)
        {
            using var db = new TrungTamDbContext();
            DamBaoAdmin(db, maAdmin);
            // Hỗ trợ hệ thống cũ: chưa có vai trò GiaoVien.
            if (!db.VaiTros.Any(v => v.TenVaiTro == "GiaoVien"))
            {
                db.VaiTros.Add(new VaiTro { TenVaiTro = "GiaoVien", MoTa = "Giáo viên giảng dạy" });
                db.SaveChanges();
            }
            return db.VaiTros.AsNoTracking()
                .Where(v => v.TenVaiTro == "Admin" || v.TenVaiTro == "NhanVien" || v.TenVaiTro == "GiaoVien")
                .OrderBy(v => v.MaVT).Select(v => new LuaChonVaiTro
                { MaVT = v.MaVT, TenVaiTro = v.TenVaiTro }).ToList();
        }
        public List<LuaChonGiaoVienTK> LayGiaoVienCoTheGan(int maAdmin, int? maGVDangGan = null)
        {
            using var db = new TrungTamDbContext();
            DamBaoAdmin(db, maAdmin);
            return db.GiaoViens.AsNoTracking()
                .Where(g => g.TrangThai && (g.MaGV == maGVDangGan || !db.TaiKhoans.Any(t => t.MaGV == g.MaGV)))
                .OrderBy(g => g.HoTen)
                .Select(g => new { g.MaGV, g.HoTen }).ToList()
                .Select(g => new LuaChonGiaoVienTK
                { MaGV = g.MaGV, HienThi = $"TC{g.MaGV:00} - {g.HoTen}" }).ToList();
        }
        public int TaoTaiKhoan(int maAdmin, string tenDangNhap, string hash, int maVT, int? maGV = null)
        {
            using var db = new TrungTamDbContext();
            using var tran = db.Database.BeginTransaction(IsolationLevel.Serializable);
            DamBaoAdmin(db, maAdmin);
            string tenVT = TenVaiTro(db, maVT);
            if (tenVT != "Admin" && tenVT != "NhanVien" && tenVT != "GiaoVien")
                throw new ArgumentException("Vai trò không hợp lệ.");
            if (db.TaiKhoans.Any(t => t.TenDangNhap == tenDangNhap))
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại.");
            maGV = KiemTraGiaoVien(db, tenVT, maGV);
            var tk = new TaiKhoan
            {
                TenDangNhap = tenDangNhap,
                MatKhauHash = hash,
                MaVT = maVT,
                MaGV = maGV,
                TrangThai = true,
                NgayTao = DateTime.UtcNow
            };
            db.TaiKhoans.Add(tk);
            db.SaveChanges();
            tran.Commit();
            return tk.MaTK;
        }
        public void DoiVaiTro(int maAdmin, int maTK, int maVT, int? maGV = null)
        {
            using var db = new TrungTamDbContext();
            using var tran = db.Database.BeginTransaction(IsolationLevel.Serializable);
            DamBaoAdmin(db, maAdmin);
            if (maAdmin == maTK) throw new InvalidOperationException("Không thể tự đổi vai trò của mình.");
            var tk = TimTaiKhoan(db, maTK);
            string tenVT = TenVaiTro(db, maVT);
            if (tenVT != "Admin" && tenVT != "NhanVien" && tenVT != "GiaoVien")
                throw new ArgumentException("Vai trò không hợp lệ.");
            if (tk.MaVT != maVT) KiemTraAdminCuoi(db, tk);
            tk.MaGV = KiemTraGiaoVien(db, tenVT, maGV, maTK);
            tk.MaVT = maVT;
            db.SaveChanges();
            tran.Commit();
        }
        public void DatTrangThai(int maAdmin, int maTK, bool hoatDong)
        {
            using var db = new TrungTamDbContext();
            using var tran = db.Database.BeginTransaction(IsolationLevel.Serializable);
            DamBaoAdmin(db, maAdmin);
            if (maAdmin == maTK) throw new InvalidOperationException("Không thể tự khóa tài khoản của mình.");
            var tk = TimTaiKhoan(db, maTK);
            if (tk.TrangThai && !hoatDong) KiemTraAdminCuoi(db, tk);
            tk.TrangThai = hoatDong;
            db.SaveChanges(); tran.Commit();
        }
        public void DatLaiMatKhau(int maAdmin, int maTK, string hash)
        {
            using var db = new TrungTamDbContext();
            using var tran = db.Database.BeginTransaction(IsolationLevel.Serializable);
            DamBaoAdmin(db, maAdmin);
            if (maAdmin == maTK) throw new InvalidOperationException("Hãy dùng mục Đổi mật khẩu để đổi mật khẩu của mình.");
            TimTaiKhoan(db, maTK).MatKhauHash = hash;
            db.SaveChanges(); tran.Commit();
        }
        public string LayMatKhauHashCaNhan(int maTK)
        {
            using var db = new TrungTamDbContext();
            return db.TaiKhoans.AsNoTracking()
                .Where(t => t.MaTK == maTK && t.TrangThai).Select(t => t.MatKhauHash)
                .SingleOrDefault() ?? throw new UnauthorizedAccessException("Tài khoản không còn hoạt động.");
        }
        public void DoiMatKhauCaNhan(int maTK, string hashCu, string hashMoi)
        {
            using var db = new TrungTamDbContext();
            using var tran = db.Database.BeginTransaction(IsolationLevel.Serializable);
            var tk = db.TaiKhoans.SingleOrDefault(t => t.MaTK == maTK && t.TrangThai)
                ?? throw new UnauthorizedAccessException("Tài khoản không còn hoạt động.");
            if (tk.MatKhauHash != hashCu) throw new InvalidOperationException("Mật khẩu đã được thay đổi. Vui lòng thử lại.");
            tk.MatKhauHash = hashMoi;
            db.SaveChanges(); tran.Commit();
        }
    }
}
