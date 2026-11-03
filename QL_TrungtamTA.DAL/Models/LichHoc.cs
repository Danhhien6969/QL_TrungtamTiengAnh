
using System;
using System.ComponentModel.DataAnnotations;

namespace QL_TrungtamTA.DAL.Models
{
    public class LichHoc
    {
        [Key]
        public int MaLH { get; set; }

        // Khóa ngoại
        public int MaLop { get; set; }

        public int MaPhong { get; set; }

        // 2 = Thứ hai, ... 8 = Chủ nhật
        [Range(2, 8)]
        public int Thu { get; set; }

        public TimeSpan GioBatDau { get; set; }

        public TimeSpan GioKetThuc { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        // Thuộc tính điều hướng
        public LopHoc LopHoc { get; set; } = null!;

        public PhongHoc PhongHoc { get; set; } = null!;
    }
}
