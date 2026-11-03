
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QL_TrungtamTA.DAL.Models
{
    public class BuoiHoc
    {
        [Key]
        public int MaBuoi { get; set; }

        // Khóa ngoại đến lớp học
        public int MaLop { get; set; }

        public DateTime NgayHoc { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        [StringLength(500)]
        public string? NoiDung { get; set; }

        [Required]
        [StringLength(30)]
        public string TrangThai { get; set; } = "DuKien";

        // Thuộc tính điều hướng
        public LopHoc LopHoc { get; set; } = null!;

        public ICollection<DiemDanh> DiemDanhs
        { get; set; } = new List<DiemDanh>();
    }
}
