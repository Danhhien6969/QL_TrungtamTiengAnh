using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL
{
    // DTO chỉ đọc: không chuyển đổi kiểu cột trực tiếp trong CellFormatting.
    public sealed class DongLopHocTongHop
    {
        public int MaLop { get; set; }
        // STT chỉ dùng để hiển thị theo danh sách đang lọc, không lưu vào SQL.
        public int STT { get; set; }
        // Mã nhận diện lớp trên giao diện; khóa chính trong SQL vẫn là số nguyên MaLop.
        public string MaHienThi => $"LH{MaLop:D2}";
        public string TenLop { get; set; } = "";
        public int MaKH { get; set; }
        public int MaGV { get; set; }
        public string TenKhoaHoc { get; set; } = "";
        public string TenGiaoVien { get; set; } = "";
        public string TenPhong { get; set; } = "Chưa chọn";
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string BatDau => NgayBatDau.ToString("dd/MM/yyyy");
        public string KetThuc => NgayKetThuc.ToString("dd/MM/yyyy");
        public int SiSoToiDa { get; set; }
        public int SiSoHienTai { get; set; }
        public string SiSo => $"{SiSoHienTai}/{SiSoToiDa}";
        public string TrangThai { get; set; } = "";
        public string TrangThaiText => TrangThai switch
        {
            "SapMo" => "Sắp mở",
            "DangHoc" => "Đang học",
            "DaKetThuc" => "Đã kết thúc",
            "DaHuy" => "Đã hủy",
            _ => TrangThai
        };
        public decimal HocPhiApDung { get; set; }
        public int SiSoToiThieu { get; set; }
        public string GhiChu { get; set; } = "";
    }

    public sealed class HocVienTrongLop
    {
        public int MaHV { get; set; }
        public string MaHienThi => $"VT{MaHV:D2}";
        public string HoTen { get; set; } = "";
        public DateTime NgayDangKy { get; set; }
        public string NgayDangKyText => NgayDangKy.ToString("dd/MM/yyyy");
        public string TrangThai { get; set; } = "";
    }

    public class LopHocDAL
    {
        public List<LopHoc> LayDanhSach()
        {
            using var db = new TrungTamDbContext();
            return db.LopHocs.AsNoTracking()
                .Include(l => l.KhoaHoc).Include(l => l.GiaoVien)
                .OrderByDescending(l => l.MaLop).ToList();
        }

        public List<LopHoc> TimKiem(string tuKhoa)
        {
            var all = LayDanhSach();
            tuKhoa = (tuKhoa ?? "").Trim();
            return tuKhoa.Length == 0 ? all : all.Where(l =>
                l.TenLop.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                (l.KhoaHoc?.TenKH ?? "").Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                (l.GiaoVien?.HoTen ?? "").Contains(tuKhoa, StringComparison.OrdinalIgnoreCase) ||
                l.TrangThai.Contains(tuKhoa, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public LopHoc? LayTheoMa(int maLop)
        {
            using var db = new TrungTamDbContext();
            return db.LopHocs.AsNoTracking()
                .Include(l => l.KhoaHoc).Include(l => l.GiaoVien)
                .FirstOrDefault(l => l.MaLop == maLop);
        }

        public List<KhoaHoc> LayKhoaHocs()
        {
            using var db = new TrungTamDbContext();
            return db.KhoaHocs.AsNoTracking().OrderBy(k => k.TenKH).ToList();
        }

        public List<GiaoVien> LayGiaoViens()
        {
            using var db = new TrungTamDbContext();
            return db.GiaoViens.AsNoTracking().OrderBy(g => g.HoTen).ToList();
        }

        public List<PhongHoc> LayPhongHocs()
        {
            using var db = new TrungTamDbContext();
            // Chỉ phòng đang hoạt động được chọn làm phòng mặc định mới.
            return db.PhongHocs.AsNoTracking()
                .Where(p => p.TrangThai)
                .OrderBy(p => p.TenPhong).ToList();
        }

        // Lấy sĩ số từ bản ghi đăng ký, không lưu trùng một cột số lượng.
        public List<DongLopHocTongHop> LayBangTongHop()
        {
            using var db = new TrungTamDbContext();
            return db.LopHocs.AsNoTracking().OrderByDescending(l => l.MaLop)
                .Select(l => new DongLopHocTongHop
                {
                    MaLop = l.MaLop,
                    TenLop = l.TenLop,
                    MaKH = l.MaKH,
                    MaGV = l.MaGV,
                    TenKhoaHoc = l.KhoaHoc.TenKH,
                    TenGiaoVien = l.GiaoVien.HoTen,
                    TenPhong = db.PhongHocs.Where(p => p.MaPhong == l.MaPhongMacDinh)
                        .Select(p => p.TenPhong).FirstOrDefault() ?? "Chưa chọn",
                    NgayBatDau = l.NgayBatDau,
                    NgayKetThuc = l.NgayKetThuc,
                    SiSoToiDa = db.PhongHocs.Where(p => p.MaPhong == l.MaPhongMacDinh)
                        .Select(p => (int?)p.SucChua).FirstOrDefault() ?? l.SiSoToiDa,
                    SiSoHienTai = db.DangKyHocs.Count(d => d.MaLop == l.MaLop && d.TrangThai != "DaHuy"),
                    TrangThai = l.TrangThai,
                    HocPhiApDung = l.HocPhiRieng ?? l.KhoaHoc.HocPhi,
                    SiSoToiThieu = 1,
                    GhiChu = l.GhiChu ?? ""
                }).ToList();
        }

        public List<HocVienTrongLop> LayHocVienTrongLop(int maLop)
        {
            using var db = new TrungTamDbContext();
            return db.DangKyHocs.AsNoTracking()
                .Where(d => d.MaLop == maLop && d.TrangThai != "DaHuy")
                .OrderBy(d => d.HocVien.HoTen)
                .Select(d => new HocVienTrongLop
                {
                    MaHV = d.MaHV,
                    HoTen = d.HocVien.HoTen,
                    NgayDangKy = d.NgayDangKy,
                    TrangThai = d.TrangThai
                }).ToList();
        }

        public string GoiYTenLop(int maKH)
        {
            using var db = new TrungTamDbContext();
            var ten = db.KhoaHocs.AsNoTracking()
                .Where(k => k.MaKH == maKH).Select(k => k.TenKH).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ten)) return "";
            // Sinh từ hậu tố lớn nhất; tránh trùng tên nếu lớp trước đã bị xóa.
            var regex = new Regex("^" + Regex.Escape(ten) + @"-(\d+)$", RegexOptions.IgnoreCase);
            var tatCa = db.LopHocs.AsNoTracking().Select(l => l.TenLop).ToList();
            int lonNhat = tatCa.Select(s => regex.Match(s))
                .Where(m => m.Success).Select(m => int.TryParse(m.Groups[1].Value, out int n) ? n : 0)
                .DefaultIfEmpty(0).Max();
            return $"{ten}-{lonNhat + 1:D2}";
        }

        private static void KiemTraLienKet(TrungTamDbContext db, LopHoc lop, LopHoc? cu)
        {
            var khoa = db.KhoaHocs.AsNoTracking().FirstOrDefault(k => k.MaKH == lop.MaKH)
                ?? throw new InvalidOperationException("Khóa học không tồn tại.");
            if ((cu == null || cu.MaKH != lop.MaKH) && !khoa.TrangThai)
                throw new InvalidOperationException("Khóa học ngừng tuyển sinh, không thể mở lớp mới.");
            var gv = db.GiaoViens.AsNoTracking().FirstOrDefault(g => g.MaGV == lop.MaGV)
                ?? throw new InvalidOperationException("Giáo viên không tồn tại.");
            if ((cu == null || cu.MaGV != lop.MaGV) && !gv.TrangThai)
                throw new InvalidOperationException("Giáo viên đã ngừng làm việc.");

            if (!lop.MaPhongMacDinh.HasValue || lop.MaPhongMacDinh.Value <= 0)
                throw new InvalidOperationException("Vui lòng chọn phòng học cho lớp.");
            var phong = db.PhongHocs.AsNoTracking()
                .FirstOrDefault(p => p.MaPhong == lop.MaPhongMacDinh.Value)
                ?? throw new InvalidOperationException("Phòng học không tồn tại.");
            if (!phong.TrangThai)
                throw new InvalidOperationException("Phòng học đã ngừng sử dụng.");
            // Trường SiSoToiDa trong SQL giữ để tương thích các phân hệ cũ.
            lop.SiSoToiDa = phong.SucChua;
            lop.SiSoToiThieu = 1; // không còn chính sách sĩ số tối thiểu

        }

        private static void KiemTraTrungTen(TrungTamDbContext db, LopHoc lop)
        {
            string ten = lop.TenLop.Trim().ToUpper();
            if (db.LopHocs.Any(l => l.MaLop != lop.MaLop && l.TenLop.ToUpper() == ten))
                throw new InvalidOperationException("Tên lớp đã tồn tại.");
        }

        private static void KiemTraLichDaXep(TrungTamDbContext db, LopHoc lop, LopHoc cu)
        {
            // Khi đổi khoảng ngày, mọi lịch lặp đã tạo phải nằm trong khoảng mới.
            if (db.LichHocs.Any(l => l.MaLop == cu.MaLop &&
                (l.NgayBatDau < lop.NgayBatDau || l.NgayKetThuc > lop.NgayKetThuc)))
                throw new InvalidOperationException("Khoảng ngày mới không bao phủ lịch học đã xếp.");
            if (db.BuoiHocs.Any(b => b.MaLop == cu.MaLop &&
                (b.NgayHoc < lop.NgayBatDau || b.NgayHoc > lop.NgayKetThuc)))
                throw new InvalidOperationException("Khoảng ngày mới không bao phủ buổi học đã tạo.");

            // Đổi giáo viên có thể khiến lịch của hai lớp trùng ca.
            if (lop.MaGV != cu.MaGV)
            {
                var lichCuaLop = db.LichHocs.AsNoTracking().Where(x => x.MaLop == cu.MaLop).ToList();
                var lichGV = db.LichHocs.AsNoTracking().Where(x => x.MaLop != cu.MaLop && x.LopHoc.MaGV == lop.MaGV).ToList();
                foreach (var a in lichCuaLop)
                    if (lichGV.Any(b => a.Thu == b.Thu && a.NgayBatDau <= b.NgayKetThuc &&
                        a.NgayKetThuc >= b.NgayBatDau && a.GioBatDau < b.GioKetThuc && a.GioKetThuc > b.GioBatDau))
                        throw new InvalidOperationException("Đổi giáo viên gây trùng ca giảng dạy đã xếp.");
            }
            // Lịch thực tế có MaPhong riêng, đổi phòng mặc định KHÔNG tự sửa lịch.
            // Dù phòng mặc định đổi hay không, lịch riêng có thể dùng phòng khác.
            // Kiểm tra sức chứa từng phòng đã xếp với số học viên thực tế.
            int daDangKy = db.DangKyHocs.Count(d => d.MaLop == cu.MaLop && d.TrangThai != "DaHuy");
            var sucChuaPhongLich = db.LichHocs.Where(x => x.MaLop == cu.MaLop)
                .Select(x => (int?)x.PhongHoc.SucChua).ToList();
            if (sucChuaPhongLich.Any(sucChua => sucChua.HasValue && sucChua.Value < daDangKy))
                throw new InvalidOperationException("Phòng trong lịch học không đủ chỗ cho học viên đã đăng ký.");

        }

        private static bool CoNgayThu(int thu, DateTime bd, DateTime kt)
        {
            bd = bd.Date; kt = kt.Date;
            if (bd > kt) return false;
            int thuDau = bd.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)bd.DayOfWeek + 1;
            return bd.AddDays((thu - thuDau + 7) % 7) <= kt;
        }

        private static void KiemTraDoiPhong(TrungTamDbContext db, LopHoc lop, LopHoc cu)
        {
            if (lop.MaPhongMacDinh == cu.MaPhongMacDinh) return;
            var lich = db.LichHocs.Where(x => x.MaLop == cu.MaLop).ToList();
            if (lich.Count == 0) return;
            if (!lop.MaPhongMacDinh.HasValue)
                throw new InvalidOperationException("Lớp đã có lịch: phải chọn phòng mới để chuyển lịch.");
            if (db.BuoiHocs.Any(b => b.MaLop == cu.MaLop))
                throw new InvalidOperationException("Lớp đã có buổi học/điểm danh. Không chuyển toàn bộ lịch; hãy điều chỉnh phòng tại màn Lịch học để giữ lịch sử.");
            foreach (var a in lich)
            {
                var trung = db.LichHocs.AsNoTracking()
                    .Where(b => b.MaLop != cu.MaLop && b.MaPhong == lop.MaPhongMacDinh.Value &&
                        b.Thu == a.Thu && b.NgayBatDau <= a.NgayKetThuc && b.NgayKetThuc >= a.NgayBatDau &&
                        b.GioBatDau < a.GioKetThuc && b.GioKetThuc > a.GioBatDau)
                    .Select(b => new { b.NgayBatDau, b.NgayKetThuc }).ToList();
                foreach (var b in trung)
                {
                    var tu = a.NgayBatDau > b.NgayBatDau ? a.NgayBatDau : b.NgayBatDau;
                    var den = a.NgayKetThuc < b.NgayKetThuc ? a.NgayKetThuc : b.NgayKetThuc;
                    if (CoNgayThu(a.Thu, tu, den))
                        throw new InvalidOperationException("Phòng mới đã có lịch trùng ngày và giờ. Vui lòng chọn phòng khác.");
                }
            }
            // Lịch cũ chưa phát sinh buổi học, chuyển phòng đồng bộ trong cùng transaction.
            foreach (var a in lich) a.MaPhong = lop.MaPhongMacDinh.Value;
        }

        public int Them(LopHoc lop)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            KiemTraTrungTen(db, lop);
            KiemTraLienKet(db, lop, null);
            if (lop.TrangThai == "DangHoc")
                throw new InvalidOperationException("Lớp mới cần ở trạng thái Sắp mở trước khi đủ học viên khai giảng.");
            db.LopHocs.Add(lop);
            db.SaveChanges(); tx.Commit();
            return lop.MaLop;
        }

        public bool Sua(LopHoc lop)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            var cu = db.LopHocs.FirstOrDefault(l => l.MaLop == lop.MaLop);
            if (cu == null) return false;
            KiemTraTrungTen(db, lop);
            KiemTraLienKet(db, lop, cu);
            int daDangKy = db.DangKyHocs.Count(d => d.MaLop == lop.MaLop && d.TrangThai != "DaHuy");
            if (lop.SiSoToiDa < daDangKy)
                throw new InvalidOperationException($"Đã có {daDangKy} học viên, không thể hạ sĩ số xuống {lop.SiSoToiDa}.");
            if (cu.MaKH != lop.MaKH && db.DangKyHocs.Any(d => d.MaLop == lop.MaLop))
                throw new InvalidOperationException("Lớp đã có lịch sử đăng ký, không thể đổi khóa học.");
            KiemTraLichDaXep(db, lop, cu);
            KiemTraDoiPhong(db, lop, cu);

            cu.TenLop = lop.TenLop;
            cu.MaKH = lop.MaKH; cu.MaGV = lop.MaGV;
            cu.NgayBatDau = lop.NgayBatDau; cu.NgayKetThuc = lop.NgayKetThuc;
            cu.SiSoToiDa = lop.SiSoToiDa; cu.TrangThai = lop.TrangThai;
            cu.MaPhongMacDinh = lop.MaPhongMacDinh;
            cu.GhiChu = lop.GhiChu;
            cu.HocPhiRieng = lop.HocPhiRieng;
            cu.SiSoToiThieu = lop.SiSoToiThieu;
            db.SaveChanges(); tx.Commit();
            return true;
        }

        public bool Xoa(int maLop)
        {
            using var db = new TrungTamDbContext();
            var lop = db.LopHocs.FirstOrDefault(l => l.MaLop == maLop);
            if (lop == null) return false;
            if (db.DangKyHocs.Any(d => d.MaLop == maLop) ||
                db.LichHocs.Any(l => l.MaLop == maLop) ||
                db.BuoiHocs.Any(b => b.MaLop == maLop))
                throw new InvalidOperationException("Lớp đã có đăng ký/lịch/buổi học. Hãy Sửa trạng thái sang 'Đã hủy' thay vì xóa.");
            db.LopHocs.Remove(lop);
            db.SaveChanges(); return true;
        }

        // Mở lớp theo ngày khai giảng, không còn ngưỡng sĩ số tối thiểu.
        // Lớp Sắp mở hết hạn chưa khai giảng để nguyên cho nhân viên xử lý.
        public int CapNhatTrangThaiTheoNgay(DateTime homNay)
        {
            using var db = new TrungTamDbContext();
            var ds = db.LopHocs.Where(l =>
                (l.TrangThai == "SapMo" && l.NgayBatDau <= homNay && l.NgayKetThuc >= homNay) ||
                (l.TrangThai == "DangHoc" && l.NgayKetThuc < homNay)).ToList();
            int count = 0;
            foreach (var l in ds)
            {
                if (l.TrangThai == "DangHoc")
                {
                    l.TrangThai = "DaKetThuc";
                    count++;
                }
                else
                {
                    if (l.MaPhongMacDinh.HasValue && db.PhongHocs.Any(p =>
                        p.MaPhong == l.MaPhongMacDinh.Value && p.TrangThai))
                    {
                        l.TrangThai = "DangHoc";
                        count++;
                    }
                }
            }
            db.SaveChanges();
            return count;
        }
    }
}
