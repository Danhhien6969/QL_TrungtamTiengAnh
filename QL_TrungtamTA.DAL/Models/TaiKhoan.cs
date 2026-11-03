using System;
using System.ComponentModel.DataAnnotations;

namespace QL_TrungtamTA.DAL.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTK { get; set; }
        [Required, StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;
        [Required, StringLength(255)]
        public string MatKhauHash { get; set; } = string.Empty;
        public int MaVT { get; set; }
        public bool TrangThai { get; set; } = true;
        public DateTime NgayTao { get; set; } = DateTime.UtcNow;
        public int? MaGV { get; set; } // Admin/NhanVien: null; GiaoVien: bắt buộc
        public VaiTro VaiTro { get; set; } = null!;
        public GiaoVien? GiaoVien { get; set; }
    }
}
