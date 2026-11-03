
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_TrungtamTA.DAL.Models
{
    public class ThanhToanHocPhi
    {
        [Key]
        public int MaTT { get; set; }

        // Khóa ngoại đến đăng ký học
        public int MaDK { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTien { get; set; }

        public DateTime NgayThanhToan { get; set; }
            = DateTime.Now;

        [Required]
        [StringLength(30)]
        public string HinhThucTT { get; set; } = "TienMat";

        [StringLength(255)]
        public string? GhiChu { get; set; }

        // Thuộc tính điều hướng
        public DangKyHoc DangKyHoc { get; set; } = null!;
    }
}
