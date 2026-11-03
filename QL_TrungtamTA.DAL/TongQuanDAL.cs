using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;

namespace QL_TrungtamTA.DAL
{
    public sealed class DongBuoiHomNay
    {
        public string Lop { get; set; } = "";
        public string GiaoVien { get; set; } = "";
        public string Phong { get; set; } = "";
        public string Gio { get; set; } = "";
        public string SiSo { get; set; } = "";
        public string DiemDanh { get; set; } = "";
    }

    public sealed class DongCanChuY
    {
        public string NoiDung { get; set; } = "";
        public string ChiTiet { get; set; } = "";
    }

    public sealed class DongLichSapToi
    {
        public string Ngay { get; set; } = "";
        public string Lop { get; set; } = "";
        public string GiaoVien { get; set; } = "";
        public string Phong { get; set; } = "";
        public string Gio { get; set; } = "";
        public string DiemDanh { get; set; } = "";
    }

    public sealed class DuLieuTongQuan
    {
        public int SoBuoiHomNay { get; set; }
        public int SoBuoiDaDiemDanh { get; set; }
        public int SoBuoiChuaDiemDanh { get; set; }
        public List<DongBuoiHomNay> BuoiHomNay { get; set; } = new();
        public List<DongCanChuY> CanChuY { get; set; } = new();
        public List<DongLichSapToi> LichSapToi { get; set; } = new();
    }

    public sealed class TongQuanDAL
    {
        public DuLieuTongQuan LayDuLieu(int maTK, DateTime ngay, bool kemLichSapToi = true)
        {
            using var db = new TrungTamDbContext();
            var taiKhoan = db.TaiKhoans.AsNoTracking()
                .Where(t => t.MaTK == maTK && t.TrangThai)
                .Select(t => new { VaiTro = t.VaiTro.TenVaiTro, t.MaGV })
                .SingleOrDefault()
                ?? throw new UnauthorizedAccessException("Tài khoản không còn hoạt động.");

            bool admin = string.Equals(taiKhoan.VaiTro, "Admin", StringComparison.OrdinalIgnoreCase);
            bool giaoVien = string.Equals(taiKhoan.VaiTro, "GiaoVien", StringComparison.OrdinalIgnoreCase);
            if (!admin && (!giaoVien || !taiKhoan.MaGV.HasValue))
                throw new UnauthorizedAccessException("Tài khoản không có quyền xem Tổng quan.");

            int? maGV = admin ? null : taiKhoan.MaGV;
            DateTime homNay = ngay.Date;
            DateTime ngayMai = homNay.AddDays(1);
            // LichHoc.Thu: Thứ Hai=2 ... Thứ Bảy=7, Chủ Nhật=8.
            int thuHomNay = homNay.DayOfWeek == DayOfWeek.Sunday
                ? 8 : (int)homNay.DayOfWeek + 1;

            // Lịch cố định hằng tuần: không phụ thuộc giáo viên đã tạo BuoiHoc hay chưa.
            var lichQuery = db.LichHocs.AsNoTracking().Where(l =>
                l.Thu == thuHomNay &&
                l.NgayBatDau < ngayMai && l.NgayKetThuc >= homNay &&
                l.LopHoc.TrangThai != "DaHuy");
            if (maGV.HasValue)
                lichQuery = lichQuery.Where(l => l.LopHoc.MaGV == maGV.Value);

            var lichHomNay = lichQuery.Select(l => new
            {
                l.MaLop,
                l.MaPhong,
                l.GioBatDau,
                l.GioKetThuc,
                Lop = l.LopHoc.TenLop,
                GiaoVien = l.LopHoc.GiaoVien.HoTen
            }).ToList();

            // Buổi phát sinh/thực tế được phép xuất hiện ngay cả khi không có lịch cố định.
            var buoiQuery = db.BuoiHocs.AsNoTracking()
                .Where(b => b.NgayHoc >= homNay && b.NgayHoc < ngayMai);
            if (maGV.HasValue)
                buoiQuery = buoiQuery.Where(b => b.LopHoc.MaGV == maGV.Value);
            var buoiHomNay = buoiQuery.Select(b => new
            {
                b.MaBuoi,
                b.MaLop,
                b.GioBatDau,
                b.GioKetThuc,
                b.TrangThai,
                Lop = b.LopHoc.TenLop,
                GiaoVien = b.LopHoc.GiaoVien.HoTen,
                b.LopHoc.MaPhongMacDinh
            }).ToList();

            var idsLop = lichHomNay.Select(l => l.MaLop)
                .Concat(buoiHomNay.Select(b => b.MaLop)).Distinct().ToList();
            var idsPhong = lichHomNay.Select(l => l.MaPhong)
                .Concat(buoiHomNay.Where(b => b.MaPhongMacDinh.HasValue)
                    .Select(b => b.MaPhongMacDinh!.Value)).Distinct().ToList();
            var phong = db.PhongHocs.AsNoTracking()
                .Where(p => idsPhong.Contains(p.MaPhong))
                .Select(p => new { p.MaPhong, p.TenPhong, p.SucChua })
                .ToList().ToDictionary(p => p.MaPhong);
            var siSo = db.DangKyHocs.AsNoTracking()
                .Where(d => idsLop.Contains(d.MaLop) && d.TrangThai != "DaHuy" && d.NgayDangKy < ngayMai)
                .GroupBy(d => d.MaLop)
                .Select(g => new { MaLop = g.Key, So = g.Count() })
                .ToList().ToDictionary(x => x.MaLop, x => x.So);
            var idsBuoi = buoiHomNay.Select(b => b.MaBuoi).ToList();
            var daGhi = db.DiemDanhs.AsNoTracking()
                .Where(d => idsBuoi.Contains(d.MaBuoi))
                .GroupBy(d => d.MaBuoi)
                .Select(g => new { MaBuoi = g.Key, So = g.Count() })
                .ToList().ToDictionary(x => x.MaBuoi, x => x.So);

            var kq = new DuLieuTongQuan();
            var daGhép = new HashSet<int>();
            void ThemDong(string lop, string gv, int maLop, int? maPhong,
                TimeSpan batDau, TimeSpan ketThuc, int? maBuoi, string? trangThai)
            {
                if (trangThai == "DaHuy") return;
                int tong = siSo.GetValueOrDefault(maLop);
                string tenPhong = "Chưa phân phòng";
                string sucChua = "—";
                if (maPhong.HasValue && phong.TryGetValue(maPhong.Value, out var p))
                {
                    tenPhong = p.TenPhong;
                    sucChua = p.SucChua.ToString();
                }
                int ghi = maBuoi.HasValue ? daGhi.GetValueOrDefault(maBuoi.Value) : 0;
                bool hoanTat = maBuoi.HasValue && trangThai == "DaDienRa" && tong > 0 && ghi >= tong;
                kq.SoBuoiHomNay++;
                if (hoanTat) kq.SoBuoiDaDiemDanh++;
                else kq.SoBuoiChuaDiemDanh++;
                kq.BuoiHomNay.Add(new DongBuoiHomNay
                {
                    Lop = lop,
                    GiaoVien = gv,
                    Phong = tenPhong,
                    Gio = $"{batDau:hh\\:mm}–{ketThuc:hh\\:mm}",
                    SiSo = $"{tong}/{sucChua}",
                    DiemDanh = !maBuoi.HasValue ? "Chưa tạo buổi" :
                        hoanTat ? "Đã điểm danh" : ghi > 0 ? $"Chưa đủ ({ghi}/{tong})" : "Chưa điểm danh"
                });
            }

            foreach (var lich in lichHomNay)
            {
                // Ghép theo lớp + giờ bắt đầu; tránh đếm trùng lịch cố định và buổi thực tế.
                var b = buoiHomNay.FirstOrDefault(b =>
                    b.MaLop == lich.MaLop && b.GioBatDau == lich.GioBatDau);
                if (b != null)
                {
                    daGhép.Add(b.MaBuoi);
                    ThemDong(b.Lop, b.GiaoVien, b.MaLop, lich.MaPhong,
                        b.GioBatDau, b.GioKetThuc, b.MaBuoi, b.TrangThai);
                }
                else
                    ThemDong(lich.Lop, lich.GiaoVien, lich.MaLop, lich.MaPhong,
                        lich.GioBatDau, lich.GioKetThuc, null, null);
            }
            foreach (var b in buoiHomNay.Where(b => !daGhép.Contains(b.MaBuoi)))
                ThemDong(b.Lop, b.GiaoVien, b.MaLop, b.MaPhongMacDinh,
                    b.GioBatDau, b.GioKetThuc, b.MaBuoi, b.TrangThai);

            kq.BuoiHomNay = kq.BuoiHomNay.OrderBy(b => b.Gio).ThenBy(b => b.Lop).ToList();
            if (kq.SoBuoiChuaDiemDanh > 0)
                kq.CanChuY.Add(new DongCanChuY
                {
                    NoiDung = "Buổi chưa hoàn tất điểm danh",
                    ChiTiet = $"{kq.SoBuoiChuaDiemDanh} buổi hôm nay chưa hoàn tất (bao gồm buổi chưa tạo)."
                });

            var lopQuery = db.LopHocs.AsNoTracking().Where(l =>
                l.TrangThai != "DaHuy" && l.TrangThai != "DaKetThuc");
            if (maGV.HasValue)
                lopQuery = lopQuery.Where(l => l.MaGV == maGV.Value);
            var lop = lopQuery.Select(l => new
            {
                l.MaLop,
                l.TenLop,
                l.NgayKetThuc,
                l.MaPhongMacDinh
            }).ToList();
            var ids = lop.Select(l => l.MaLop).ToList();
            var phongLopIds = lop.Where(l => l.MaPhongMacDinh.HasValue)
                .Select(l => l.MaPhongMacDinh!.Value).Distinct().ToList();
            var phongLop = db.PhongHocs.AsNoTracking()
                .Where(p => phongLopIds.Contains(p.MaPhong))
                .Select(p => new { p.MaPhong, p.SucChua }).ToList()
                .ToDictionary(p => p.MaPhong, p => p.SucChua);
            var soDangKy = db.DangKyHocs.AsNoTracking()
                .Where(d => ids.Contains(d.MaLop) && d.TrangThai != "DaHuy")
                .GroupBy(d => d.MaLop)
                .Select(g => new { MaLop = g.Key, So = g.Count() })
                .ToList().ToDictionary(x => x.MaLop, x => x.So);
            foreach (var l in lop)
            {
                int dangKy = soDangKy.GetValueOrDefault(l.MaLop);
                if (l.MaPhongMacDinh.HasValue &&
                    phongLop.TryGetValue(l.MaPhongMacDinh.Value, out int sucChua) &&
                    sucChua > 0 && sucChua - dangKy <= 2)
                    kq.CanChuY.Add(new DongCanChuY
                    {
                        NoiDung = $"Lớp {l.TenLop} gần đầy",
                        ChiTiet = $"{dangKy}/{sucChua} học viên; còn {Math.Max(0, sucChua - dangKy)} chỗ."
                    });
                int ngayCon = (l.NgayKetThuc.Date - homNay).Days;
                if (ngayCon >= 0 && ngayCon <= 7)
                    kq.CanChuY.Add(new DongCanChuY
                    {
                        NoiDung = $"Lớp {l.TenLop} sắp kết thúc",
                        ChiTiet = $"Ngày kết thúc: {l.NgayKetThuc:dd/MM/yyyy}."
                    });
            }
            // Bảy ngày tiếp theo kể từ ngày đang xem. Dùng cùng bộ lọc quyền
            // và quy tắc ghép lịch, nhưng không lặp đệ quy danh sách sắp tới.
            if (kemLichSapToi)
            {
                for (int i = 1; i <= 7; i++)
                {
                    DateTime ngayKe = homNay.AddDays(i);
                    var duLieuNgay = LayDuLieu(maTK, ngayKe, false);
                    foreach (var dong in duLieuNgay.BuoiHomNay)
                        kq.LichSapToi.Add(new DongLichSapToi
                        {
                            Ngay = ngayKe.ToString("dd/MM/yyyy"),
                            Lop = dong.Lop,
                            GiaoVien = dong.GiaoVien,
                            Phong = dong.Phong,
                            Gio = dong.Gio,
                            DiemDanh = dong.DiemDanh
                        });
                }
            }
            return kq;
        }
    }
}
