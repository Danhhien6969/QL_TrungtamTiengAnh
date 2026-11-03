using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_TrungtamTA.DAL.Models
{
    public class GiaoVien
    {
        [Key]
        public int MaGV { get; set; }

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        public string? GioiTinh { get; set; }

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? ChuyenMon { get; set; }

        public DateTime NgayVaoLam { get; set; } = DateTime.Today;

        public bool TrangThai { get; set; } = true;

        // Các thông tin mới - để nullable, tránh ảnh hưởng giáo viên cũ.
        [StringLength(255)]
        public string? DiaChi { get; set; }

        [StringLength(1000)]
        public string? GhiChu { get; set; }

        [StringLength(100)]
        public string? TrinhDoHocVan { get; set; }

        [StringLength(30)]
        public string? XepLoaiTotNghiep { get; set; }

        // Một ảnh chứng chỉ/bằng cấp, lưu ngay trong SQL Server.
        // Không phụ thuộc đường dẫn ảnh trên máy người sử dụng.
        [Column(TypeName = "varbinary(max)")]
        public byte[]? AnhChungChi { get; set; }
    }
}
