using System;
using System.Collections.Generic;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTA.BLL
{
    public class QuanLyTaiKhoanBLL
    {
        private readonly QuanLyTaiKhoanDAL dal = new();
        private static int LayMaAdmin(PhienDangNhap phien)
        {
            ArgumentNullException.ThrowIfNull(phien);
            return phien.MaTK;
        }
        // Tên đăng nhập linh hoạt, không để trống, tối đa 50 ký tự.
        private static void KiemTraTenDangNhap(string tenDangNhap)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap))
                throw new ArgumentException("Vui lòng nhập tên đăng nhập.");
            if (tenDangNhap.Length > 50)
                throw new ArgumentException("Tên đăng nhập không được vượt quá 50 ký tự.");
        }

        // Không bắt buộc độ dài tối thiểu; vẫn giới hạn theo dữ liệu đang hỗ trợ.
        private static void KiemTraMatKhauMoi(string matKhau)
        {
            if (string.IsNullOrEmpty(matKhau))
                throw new ArgumentException("Vui lòng nhập mật khẩu.");
            if (matKhau.Length > 128)
                throw new ArgumentException("Mật khẩu không được vượt quá 128 ký tự.");
        }

        public List<TaiKhoanHienThi> LayDanhSach(PhienDangNhap phien, string tuKhoa = "") =>
            dal.LayDanhSach(LayMaAdmin(phien), tuKhoa);
        public List<LuaChonVaiTro> LayVaiTro(PhienDangNhap phien) => dal.LayVaiTro(LayMaAdmin(phien));
        public List<LuaChonGiaoVienTK> LayGiaoVienCoTheGan(PhienDangNhap phien, int? maGV = null) =>
            dal.LayGiaoVienCoTheGan(LayMaAdmin(phien), maGV);
        public int TaoTaiKhoan(PhienDangNhap phien, string tenDangNhap,
            string matKhauMoi, int maVT, int? maGV = null)
        {
            tenDangNhap = (tenDangNhap ?? "").Trim();
            KiemTraTenDangNhap(tenDangNhap);
            KiemTraMatKhauMoi(matKhauMoi);
            return dal.TaoTaiKhoan(LayMaAdmin(phien), tenDangNhap,
                BaoMatMatKhau.BamMatKhau(matKhauMoi), maVT, maGV);
        }
        public void DoiVaiTro(PhienDangNhap phien, int maTK, int maVT, int? maGV = null)
        {
            if (maTK <= 0) throw new ArgumentException("Vui lòng chọn tài khoản.");
            dal.DoiVaiTro(LayMaAdmin(phien), maTK, maVT, maGV);
        }
        public void DatTrangThai(PhienDangNhap phien, int maTK, bool hoatDong)
        {
            if (maTK <= 0) throw new ArgumentException("Vui lòng chọn tài khoản.");
            dal.DatTrangThai(LayMaAdmin(phien), maTK, hoatDong);
        }
        public void DatLaiMatKhau(PhienDangNhap phien, int maTK, string matKhauMoi)
        {
            if (maTK <= 0) throw new ArgumentException("Vui lòng chọn tài khoản.");
            KiemTraMatKhauMoi(matKhauMoi);
            dal.DatLaiMatKhau(LayMaAdmin(phien), maTK, BaoMatMatKhau.BamMatKhau(matKhauMoi));
        }
        public void DoiMatKhauCaNhan(PhienDangNhap phien, string matKhauCu, string matKhauMoi)
        {
            ArgumentNullException.ThrowIfNull(phien);
            if (string.IsNullOrEmpty(matKhauCu))
                throw new ArgumentException("Vui lòng nhập mật khẩu hiện tại.");
            KiemTraMatKhauMoi(matKhauMoi);
            string hashCu = dal.LayMatKhauHashCaNhan(phien.MaTK);
            if (!BaoMatMatKhau.KiemTraMatKhau(matKhauCu, hashCu))
                throw new ArgumentException("Mật khẩu hiện tại không đúng.");
            if (BaoMatMatKhau.KiemTraMatKhau(matKhauMoi, hashCu))
                throw new ArgumentException("Mật khẩu mới phải khác mật khẩu cũ.");
            dal.DoiMatKhauCaNhan(phien.MaTK, hashCu, BaoMatMatKhau.BamMatKhau(matKhauMoi));
        }
    }
}
