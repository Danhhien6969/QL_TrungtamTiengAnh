using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using QL_TrungtamTA.DAL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.BLL
{
    public class GiaoVienBLL
    {
        private readonly GiaoVienDAL giaoVienDAL = new();
        private const int GioiHanAnh = 5 * 1024 * 1024; // 5 MB

        public List<GiaoVien> LayDanhSach() => giaoVienDAL.LayDanhSach();

        public GiaoVien? LayTheoMa(int maGV)
        {
            if (maGV <= 0)
                throw new ArgumentException("Mã giáo viên không hợp lệ.");
            return giaoVienDAL.LayTheoMa(maGV);
        }

        public List<GiaoVien> TimKiem(string tuKhoa)
            => giaoVienDAL.TimKiem(tuKhoa);

        private static string? ChuanHoa(string? chuoi) =>
            string.IsNullOrWhiteSpace(chuoi) ? null : chuoi.Trim();

        private static void KiemTraDuLieu(GiaoVien gv)
        {
            ArgumentNullException.ThrowIfNull(gv);

            gv.HoTen = (gv.HoTen ?? string.Empty).Trim();
            if (gv.HoTen.Length < 2 || gv.HoTen.Length > 100)
                throw new ArgumentException("Họ tên phải có từ 2 đến 100 ký tự.");

            if (gv.NgaySinh.HasValue && gv.NgaySinh.Value.Date > DateTime.Today)
                throw new ArgumentException("Ngày sinh không được lớn hơn ngày hiện tại.");

            if (gv.NgayVaoLam.Date > DateTime.Today)
                throw new ArgumentException("Ngày vào làm không được lớn hơn ngày hiện tại.");

            if (gv.NgaySinh.HasValue && gv.NgayVaoLam.Date < gv.NgaySinh.Value.Date)
                throw new ArgumentException("Ngày vào làm không được trước ngày sinh.");

            gv.GioiTinh = ChuanHoa(gv.GioiTinh);
            if (gv.GioiTinh?.Length > 10)
                throw new ArgumentException("Giới tính không được vượt quá 10 ký tự.");

            gv.SoDienThoai = ChuanHoa(gv.SoDienThoai);
            if (gv.SoDienThoai != null &&
                !Regex.IsMatch(gv.SoDienThoai, @"^\+?[0-9]{9,15}$"))
                throw new ArgumentException("Số điện thoại cần 9–15 chữ số, có thể bắt đầu bằng dấu +.");

            gv.Email = ChuanHoa(gv.Email);
            if (gv.Email != null &&
                (gv.Email.Length > 100 || !new EmailAddressAttribute().IsValid(gv.Email)))
                throw new ArgumentException("Email không hợp lệ hoặc quá 100 ký tự.");

            gv.ChuyenMon = ChuanHoa(gv.ChuyenMon);
            if (gv.ChuyenMon?.Length > 100)
                throw new ArgumentException("Chuyên môn không được vượt quá 100 ký tự.");

            gv.DiaChi = ChuanHoa(gv.DiaChi);
            if (gv.DiaChi?.Length > 255)
                throw new ArgumentException("Địa chỉ không được vượt quá 255 ký tự.");

            gv.GhiChu = ChuanHoa(gv.GhiChu);
            if (gv.GhiChu?.Length > 1000)
                throw new ArgumentException("Ghi chú không được vượt quá 1000 ký tự.");

            gv.TrinhDoHocVan = ChuanHoa(gv.TrinhDoHocVan);
            if (gv.TrinhDoHocVan?.Length > 100)
                throw new ArgumentException("Trình độ học vấn không được vượt quá 100 ký tự.");

            gv.XepLoaiTotNghiep = ChuanHoa(gv.XepLoaiTotNghiep);
            if (gv.XepLoaiTotNghiep != null &&
                gv.XepLoaiTotNghiep != "Yếu" &&
                gv.XepLoaiTotNghiep != "Trung bình" &&
                gv.XepLoaiTotNghiep != "Khá" &&
                gv.XepLoaiTotNghiep != "Tốt" &&
                gv.XepLoaiTotNghiep != "Xuất sắc")
                throw new ArgumentException("Xếp loại tốt nghiệp không hợp lệ.");

            if (gv.AnhChungChi != null)
            {
                if (gv.AnhChungChi.Length == 0 || gv.AnhChungChi.Length > GioiHanAnh)
                    throw new ArgumentException("Ảnh chứng chỉ phải có dung lượng tối đa 5 MB.");

                byte[] a = gv.AnhChungChi;
                bool png = a.Length >= 8 && a[0] == 0x89 && a[1] == 0x50 &&
                    a[2] == 0x4E && a[3] == 0x47 && a[4] == 0x0D && a[5] == 0x0A &&
                    a[6] == 0x1A && a[7] == 0x0A;
                bool jpg = a.Length >= 3 && a[0] == 0xFF && a[1] == 0xD8 && a[2] == 0xFF;
                if (!png && !jpg)
                    throw new ArgumentException("Chỉ hỗ trợ ảnh chứng chỉ PNG hoặc JPG.");
            }
        }

        public int Them(GiaoVien giaoVien)
        {
            KiemTraDuLieu(giaoVien);
            giaoVien.MaGV = 0;
            return giaoVienDAL.Them(giaoVien);
        }

        public bool Sua(GiaoVien giaoVien)
        {
            if (giaoVien.MaGV <= 0)
                throw new ArgumentException("Vui lòng chọn giáo viên cần sửa.");
            KiemTraDuLieu(giaoVien);
            return giaoVienDAL.Sua(giaoVien);
        }

        public bool Xoa(int maGV)
        {
            if (maGV <= 0)
                throw new ArgumentException("Vui lòng chọn giáo viên cần xóa.");
            return giaoVienDAL.Xoa(maGV);
        }
    }
}
