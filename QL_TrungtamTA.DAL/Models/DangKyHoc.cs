
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_TrungtamTA.DAL.Models
{
    public class DangKyHoc
    {
        [Key]
        public int MaDK { get; set; }

        // Khóa ngoại đến học viên
        public int MaHV { get; set; }

        // Khóa ngoại đến lớp học
        public int MaLop { get; set; }

        public DateTime NgayDangKy { get; set; }
            = DateTime.Now;

        // Học phí tại thời điểm đăng ký
        [Column(TypeName = "decimal(18,2)")]
        public decimal HocPhiPhaiDong { get; set; }

        [Required]
        [StringLength(30)]
        public string TrangThai { get; set; } = "DangHoc";

        // Thuộc tính điều hướng
        public HocVien HocVien { get; set; } = null!;

        public LopHoc LopHoc { get; set; } = null!;

        // Một đăng ký có thể thanh toán nhiều lần
        public ICollection<ThanhToanHocPhi> ThanhToanHocPhis
        { get; set; } = new List<ThanhToanHocPhi>();
    }
}
