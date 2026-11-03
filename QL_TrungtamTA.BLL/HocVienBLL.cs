using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QL_TrungtamTA.DAL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.BLL
{
    public class HocVienBLL
    {
        private readonly HocVienDAL hocVienDAL = new();

        public List<HocVien> LayDanhSach() => hocVienDAL.LayDanhSach();

        public List<HocVien> LayDanhSachDaXoa()
            => hocVienDAL.LayDanhSachDaXoa();

        public List<HocVien> TimKiemDaXoa(string tuKhoa)
            => hocVienDAL.TimKiemDaXoa(tuKhoa);

        public HocVien? LayTheoMa(int maHV)
        {
            if (maHV <= 0)
                throw new ArgumentException("Mã học viên không hợp lệ.");
            return hocVienDAL.LayTheoMa(maHV);
        }

        public List<HocVien> TimKiem(string tuKhoa)
            => hocVienDAL.TimKiem(tuKhoa);

        private static void KiemTraDuLieu(HocVien hocVien)
        {
            if (hocVien == null)
                throw new ArgumentNullException(nameof(hocVien));

            hocVien.HoTen = (hocVien.HoTen ?? string.Empty).Trim();
            if (hocVien.HoTen.Length < 2 || hocVien.HoTen.Length > 100)
                throw new ArgumentException("Họ tên học viên phải từ 2 đến 100 ký tự.");

            // Tuổi do người dùng tự nhập và có thể để trống.
            if (hocVien.Tuoi.HasValue &&
                (hocVien.Tuoi.Value < 1 || hocVien.Tuoi.Value > 120))
                throw new ArgumentException("Tuổi phải từ 1 đến 120.");

            hocVien.LopTruong = ChuanHoa(hocVien.LopTruong);
            if (hocVien.LopTruong?.Length > 50)
                throw new ArgumentException("Học sinh lớp không được quá 50 ký tự.");

            if (hocVien.NgaySinh.HasValue &&
                hocVien.NgaySinh.Value.Date > DateTime.Today)
                throw new ArgumentException("Ngày sinh không được trong tương lai.");

            hocVien.GioiTinh = ChuanHoa(hocVien.GioiTinh);
            if (hocVien.GioiTinh?.Length > 10)
                throw new ArgumentException("Giới tính không hợp lệ.");

            hocVien.SoDienThoai = ChuanHoa(hocVien.SoDienThoai);
            KiemTraDienThoai(hocVien.SoDienThoai, "Số điện thoại học viên");

            hocVien.DiaChi = ChuanHoa(hocVien.DiaChi);
            if (hocVien.DiaChi?.Length > 255)
                throw new ArgumentException("Địa chỉ không được quá 255 ký tự.");

            hocVien.HoTenPhuHuynh = ChuanHoa(hocVien.HoTenPhuHuynh);
            if (hocVien.HoTenPhuHuynh?.Length > 100)
                throw new ArgumentException("Họ tên phụ huynh không được quá 100 ký tự.");

            hocVien.SoDienThoaiPhuHuynh = ChuanHoa(hocVien.SoDienThoaiPhuHuynh);
            KiemTraDienThoai(hocVien.SoDienThoaiPhuHuynh,
                "Số điện thoại phụ huynh");
        }

        private static string? ChuanHoa(string? giaTri)
        {
            string? ketQua = giaTri?.Trim();
            return string.IsNullOrEmpty(ketQua) ? null : ketQua;
        }

        private static void KiemTraDienThoai(string? sdt, string tenTruong)
        {
            // Bỏ trống được; nếu nhập, phải đúng định dạng.
            if (sdt != null && !Regex.IsMatch(sdt, @"^\+?[0-9]{9,15}$"))
                throw new ArgumentException(
                    $"{tenTruong} phải có 9–15 chữ số (có thể bắt đầu bằng +).");
        }

        public int Them(HocVien hocVien)
        {
            KiemTraDuLieu(hocVien);
            hocVien.MaHV = 0;
            hocVien.NgayDangKy = DateTime.Today;
            hocVien.TrangThai = true; // Giữ tương thích đăng ký học.
            hocVien.Email = null;    // Không còn thu thập Email học viên.
            return hocVienDAL.Them(hocVien);
        }

        public bool Sua(HocVien hocVien)
        {
            if (hocVien.MaHV <= 0)
                throw new ArgumentException("Vui lòng chọn học viên cần sửa.");
            KiemTraDuLieu(hocVien);
            return hocVienDAL.Sua(hocVien);
        }

        // Chuyển vào thùng rác, không xóa bản ghi khỏi Database.
        public bool Xoa(int maHV)
        {
            if (maHV <= 0)
                throw new ArgumentException("Vui lòng chọn học viên cần lưu trữ.");
            return hocVienDAL.Xoa(maHV);
        }

        public bool KhoiPhuc(int maHV)
        {
            if (maHV <= 0)
                throw new ArgumentException("Vui lòng chọn học viên cần khôi phục.");
            return hocVienDAL.KhoiPhuc(maHV);
        }
    }
}
