
using System.ComponentModel.DataAnnotations;

namespace QL_TrungtamTA.DAL.Models
{
    public class DiemDanh
    {
        [Key]
        public int MaDD { get; set; }

        // Khóa ngoại đến buổi học
        public int MaBuoi { get; set; }

        // Khóa ngoại đến học viên
        public int MaHV { get; set; }

        [Required]
        [StringLength(20)]
        public string TrangThai { get; set; } = "CoMat";

        [StringLength(255)]
        public string? GhiChu { get; set; }

        // Thuộc tính điều hướng
        public BuoiHoc BuoiHoc { get; set; } = null!;

        public HocVien HocVien { get; set; } = null!;
    }
}
