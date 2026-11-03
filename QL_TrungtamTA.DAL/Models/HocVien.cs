using System;
using System.ComponentModel.DataAnnotations;

namespace QL_TrungtamTA.DAL.Models
{
    public class HocVien
    {
        [Key]
        public int MaHV { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        // Tuổi do nhân viên nhập trực tiếp, không tự tính từ ngày sinh.
        [Range(1, 120)]
        public int? Tuoi { get; set; }

        // Lớp ở trường phổ thông, ví dụ: 9A1, 12A3.
        // Khác với bảng LopHoc của trung tâm tiếng Anh.
        [StringLength(50)]
        public string? LopTruong { get; set; }

        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        public string? GioiTinh { get; set; }

        // Số điện thoại của học viên là KHÔNG BẮT BUỘC.
        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        // Thông tin phụ huynh.
        [StringLength(100)]
        public string? HoTenPhuHuynh { get; set; }

        [StringLength(15)]
        public string? SoDienThoaiPhuHuynh { get; set; }

        public DateTime NgayDangKy { get; set; } = DateTime.Today;

        // Giữ lại hai thuộc tính cũ để KHÔNG gây lỗi các chức năng đang chạy.
        // Email, Trạng thái không còn xuất hiện trên giao diện Quản lý học viên.
        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        public bool TrangThai { get; set; } = true;
    }
}
