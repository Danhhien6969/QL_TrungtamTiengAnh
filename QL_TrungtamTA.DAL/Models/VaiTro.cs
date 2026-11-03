
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QL_TrungtamTA.DAL.Models
{
    public class VaiTro
    {
        [Key]
        public int MaVT { get; set; }

        [Required]
        [StringLength(50)]
        public string TenVaiTro { get; set; } = string.Empty;

        [StringLength(255)]
        public string? MoTa { get; set; }

        // Một vai trò có nhiều tài khoản
        public ICollection<TaiKhoan> TaiKhoans
        { get; set; } = new List<TaiKhoan>();
    }
}
