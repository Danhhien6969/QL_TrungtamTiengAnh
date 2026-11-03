using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;

namespace QL_TrungtamTA.DAL
{
    public sealed class DongLichDayCuaToi
    {
        public string TenLop { get; set; } = "";
        public string Phong { get; set; } = "";
        public string Thu { get; set; } = "";
        public string GioHoc { get; set; } = "";
        public string TuNgay { get; set; } = "";
        public string DenNgay { get; set; } = "";
    }

    public sealed class LichDayCuaToiDAL
    {
        public List<DongLichDayCuaToi> LayDanhSach(int maTK)
        {
            using var db = new TrungTamDbContext();
            var tk = db.TaiKhoans.AsNoTracking()
                .Where(x => x.MaTK == maTK && x.TrangThai &&
                            x.VaiTro.TenVaiTro == "GiaoVien" && x.MaGV.HasValue)
                .Select(x => new { x.MaGV }).SingleOrDefault();
            if (tk == null || tk.MaGV == null)
                throw new UnauthorizedAccessException("Tài khoản giáo viên chưa được liên kết hoặc đã khóa.");

            var lich = db.LichHocs.AsNoTracking()
                .Where(x => x.LopHoc.MaGV == tk.MaGV.Value)
                .OrderBy(x => x.Thu).ThenBy(x => x.GioBatDau)
                .Select(x => new { x.LopHoc.TenLop, x.PhongHoc.TenPhong,
                    x.Thu, x.GioBatDau, x.GioKetThuc, x.NgayBatDau, x.NgayKetThuc })
                .ToList();
            return lich.Select(x => new DongLichDayCuaToi
            {
                TenLop = x.TenLop,
                Phong = x.TenPhong,
                Thu = x.Thu == 8 ? "Chủ nhật" : $"Thứ {x.Thu}",
                GioHoc = $"{x.GioBatDau:hh\\:mm}–{x.GioKetThuc:hh\\:mm}",
                TuNgay = x.NgayBatDau.ToString("dd/MM/yyyy"),
                DenNgay = x.NgayKetThuc.ToString("dd/MM/yyyy")
            }).ToList();
        }
    }
}
