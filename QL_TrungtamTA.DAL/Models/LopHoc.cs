using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QL_TrungtamTA.DAL.Models
{
    // Giữ tên/kiểu của tất cả các cột cũ để các phân hệ đang chạy không bị đổi.
    public class LopHoc
    {
        [Key]
        public int MaLop { get; set; }
        [Required, StringLength(100)]
        public string TenLop { get; set; } = string.Empty;
        public int MaKH { get; set; }
        public int MaGV { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int SiSoToiDa { get; set; }
        [Required, StringLength(20)]
        public string TrangThai { get; set; } = "SapMo";

        public KhoaHoc KhoaHoc { get; set; } = null!;
        public GiaoVien GiaoVien { get; set; } = null!;

        // Navigation property này đang được dùng bởi TrungTamDbContext
        // khi cấu hình quan hệ LopHoc -> LichHoc (WithMany).
        public ICollection<LichHoc> LichHocs { get; set; } = new List<LichHoc>();

        // 4 cột mới nullable để migration không phá dữ liệu đã có.
        public int? MaPhongMacDinh { get; set; }
        [ForeignKey(nameof(MaPhongMacDinh))]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public PhongHoc? PhongHocMacDinh { get; set; }
        [StringLength(500)]
        public string? GhiChu { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal? HocPhiRieng { get; set; }
        public int? SiSoToiThieu { get; set; }
    }
}
