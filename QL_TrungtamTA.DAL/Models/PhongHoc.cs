
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QL_TrungtamTA.DAL.Models
{
    public class PhongHoc
    {
        [Key]
        public int MaPhong { get; set; }

        [Required]
        [StringLength(50)]
        public string TenPhong { get; set; } = string.Empty;

        [Range(1, 200)]
        public int SucChua { get; set; } = 30;

        public bool TrangThai { get; set; } = true;

        public ICollection<LichHoc> LichHocs { get; set; }
            = new List<LichHoc>();
    }
}
