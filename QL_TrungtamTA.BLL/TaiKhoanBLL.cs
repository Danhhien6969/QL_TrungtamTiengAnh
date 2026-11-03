
using System;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTA.BLL
{
    // Lưu thông tin người dùng sau khi đăng nhập
    public sealed record PhienDangNhap(
        int MaTK,
        string TenDangNhap,
        string TenVaiTro);

    public class TaiKhoanBLL
    {
        private readonly TaiKhoanDAL taiKhoanDAL
            = new TaiKhoanDAL();

        // Kiểm tra hệ thống đã có Admin chưa
        public bool CanKhoiTaoAdmin()
        {
            return !taiKhoanDAL.DaCoTaiKhoan();
        }

        // Tạo tài khoản Admin đầu tiên
        public void TaoAdminDauTien(
            string tenDangNhap,
            string matKhau)
        {
            tenDangNhap = tenDangNhap.Trim();

            // Kiểm tra tên đăng nhập
            if (tenDangNhap.Length < 4 ||
                tenDangNhap.Length > 50 ||
                Array.Exists(
                    tenDangNhap.ToCharArray(),
                    char.IsWhiteSpace))
            {
                throw new ArgumentException(
                    "Tên đăng nhập phải từ 4 đến 50 ký tự " +
                    "và không chứa khoảng trắng.");
            }

            // Kiểm tra độ dài mật khẩu
            // Đã giảm từ 12 xuống 6 ký tự
            if (string.IsNullOrEmpty(matKhau) ||
                matKhau.Length < 6 ||
                matKhau.Length > 128)
            {
                throw new ArgumentException(
                    "Mật khẩu phải từ 6 đến 128 ký tự.");
            }

            // Mã hóa mật khẩu trước khi lưu SQL
            string hash =
                BaoMatMatKhau.BamMatKhau(matKhau);

            // Gọi DAL lưu tài khoản Admin
            taiKhoanDAL.TaoAdminDauTien(
                tenDangNhap, hash);
        }

        // Xử lý đăng nhập
        public PhienDangNhap? DangNhap(
            string tenDangNhap,
            string matKhau)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) ||
                string.IsNullOrEmpty(matKhau))
            {
                return null;
            }

            var taiKhoan =
                taiKhoanDAL.TimTheoTenDangNhap(
                    tenDangNhap.Trim());

            // Kiểm tra tài khoản tồn tại và đang hoạt động
            if (taiKhoan == null ||
                !taiKhoan.TrangThai)
            {
                return null;
            }

            // Kiểm tra mật khẩu
            bool hopLe =
                BaoMatMatKhau.KiemTraMatKhau(
                    matKhau,
                    taiKhoan.MatKhauHash);

            if (!hopLe)
            {
                return null;
            }

            // Trả về phiên đăng nhập
            return new PhienDangNhap(
                taiKhoan.MaTK,
                taiKhoan.TenDangNhap,
                taiKhoan.VaiTro.TenVaiTro);
        }
    }
}
