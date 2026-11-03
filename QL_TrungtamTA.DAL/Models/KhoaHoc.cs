using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_TrungtamTA.DAL.Models
{
    // Model dùng chung với LopHoc, DangKyHoc, ThongKe.
    // Không đổi các tên cột cũ: MaKH, TenKH, TrinhDo, ThoiLuong,
    // HocPhi, MoTa, TrangThai để tránh phá vỡ dữ liệu liên kết.
    public class KhoaHoc
    {
        [Key]
        public int MaKH { get; set; }

        [Required, StringLength(100)]
        public string TenKH { get; set; } = string.Empty; // Giao diện gọi là Chứng chỉ

        [StringLength(50)]
        public string? TrinhDo { get; set; } // Tương thích phần mềm cũ

        public int ThoiLuong { get; set; } // Tổng số GIỜ, trường cũ

        [Column(TypeName = "decimal(18,2)")]
        public decimal HocPhi { get; set; }

        [StringLength(500)]
        public string? MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        // Các trường mới: nullable để dữ liệu khóa học đã có không bị mất.
        public int? ThoiLuongThang { get; set; } // 3, 6, 9, 12
        public int? SoBuoi { get; set; }
        public int? SoGioMoiBuoi { get; set; } // Phục vụ tính tổng giờ cũ
        public int? SiSoToiDa { get; set; }

        [StringLength(60)]
        public string? LoaiKhoa { get; set; }

        [StringLength(50)]
        public string? TrinhDoDauVao { get; set; }

        [StringLength(50)]
        public string? TrinhDoDauRa { get; set; }
    }
}
