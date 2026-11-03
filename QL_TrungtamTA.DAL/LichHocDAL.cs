using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL
{
    public class LichHocDAL
    {
        public List<LichHoc> LayDanhSach()
        {
            using var db = new TrungTamDbContext();
            return db.LichHocs.AsNoTracking()
                .Include(l => l.LopHoc).ThenInclude(l => l.GiaoVien)
                .Include(l => l.PhongHoc)
                .OrderBy(l => l.LopHoc.TenLop)
                .ThenBy(l => l.Thu)
                .ThenBy(l => l.GioBatDau)
                .ToList();
        }

        public LichHoc? LayTheoMa(int maLH)
        {
            using var db = new TrungTamDbContext();
            return db.LichHocs.AsNoTracking()
                .FirstOrDefault(l => l.MaLH == maLH);
        }

        public List<LichHoc> TimKiem(string tuKhoa)
        {
            tuKhoa = (tuKhoa ?? "").Trim();
            if (tuKhoa.Length == 0) return LayDanhSach();
            using var db = new TrungTamDbContext();
            return db.LichHocs.AsNoTracking()
                .Include(l => l.LopHoc).ThenInclude(l => l.GiaoVien)
                .Include(l => l.PhongHoc)
                .Where(l => l.LopHoc.TenLop.Contains(tuKhoa) ||
                            l.PhongHoc.TenPhong.Contains(tuKhoa) ||
                            l.LopHoc.GiaoVien.HoTen.Contains(tuKhoa))
                .OrderBy(l => l.LopHoc.TenLop)
                .ThenBy(l => l.Thu)
                .ThenBy(l => l.GioBatDau)
                .ToList();
        }

        public List<LopHoc> LayLopHocs()
        {
            using var db = new TrungTamDbContext();
            return db.LopHocs.AsNoTracking()
                .Include(l => l.GiaoVien)
                .OrderBy(l => l.TenLop).ToList();
        }

        public List<PhongHoc> LayPhongHocs()
        {
            using var db = new TrungTamDbContext();
            return db.PhongHocs.AsNoTracking()
                .Where(p => p.TrangThai)
                .OrderBy(p => p.TenPhong).ToList();
        }

        // Một thứ chỉ thật sự trùng khi có ít nhất một ngày thuộc thứ ấy
        // nằm trong phần giao nhau của 2 khoảng ngày áp dụng.
        private static bool CoBuoiThucTe(int thu, DateTime tuNgay, DateTime denNgay)
        {
            tuNgay = tuNgay.Date;
            denNgay = denNgay.Date;
            if (tuNgay > denNgay) return false;
            int thuDau = tuNgay.DayOfWeek == DayOfWeek.Sunday
                ? 8 : (int)tuNgay.DayOfWeek + 1;
            int soNgayCho = (thu - thuDau + 7) % 7;
            return tuNgay.AddDays(soNgayCho) <= denNgay;
        }

        private static void KiemTraNghiepVu(TrungTamDbContext db,
            LichHoc lich, LichHoc? lichCu)
        {
            var lop = db.LopHocs.AsNoTracking()
                .FirstOrDefault(l => l.MaLop == lich.MaLop);
            if (lop == null)
                throw new InvalidOperationException("Lớp học không tồn tại.");

            var phong = db.PhongHocs.AsNoTracking()
                .FirstOrDefault(p => p.MaPhong == lich.MaPhong);
            if (phong == null)
                throw new InvalidOperationException("Phòng học không tồn tại.");

            if ((lichCu == null || lichCu.MaLop != lich.MaLop) &&
                (lop.TrangThai == "DaHuy" || lop.TrangThai == "DaKetThuc"))
                throw new InvalidOperationException("Không tạo lịch mới cho lớp đã hủy/kết thúc.");

            if (!phong.TrangThai)
                throw new InvalidOperationException("Phòng đã ngừng sử dụng. Hãy chọn phòng đang hoạt động.");

            // Ngày kết thúc lớp được tính từ lịch khi TẠO mới.
            // Với lịch cũ/điều chỉnh, không cho thay đổi giai đoạn ngoài lớp.
            if (lich.NgayBatDau.Date < lop.NgayBatDau.Date ||
                (lichCu != null && lich.NgayKetThuc.Date > lop.NgayKetThuc.Date))
                throw new InvalidOperationException("Ngày áp dụng không hợp lệ so với lớp học.");

            if (phong.SucChua < lop.SiSoToiDa)
                throw new InvalidOperationException(
                    $"Phòng {phong.TenPhong} chứa {phong.SucChua} người; " +
                    $"lớp yêu cầu {lop.SiSoToiDa} chỗ.");

            // Lọc ứng viên trong SQL, rồi so khớp ngày học thật trên .NET.
            // Giờ biểu diễn dạng [bắt đầu, kết thúc), hai ca giáp giờ không trùng.
            var cacLichCoTheTrung = db.LichHocs.AsNoTracking()
                .Where(l => l.MaLH != lich.MaLH &&
                            l.Thu == lich.Thu &&
                            l.NgayBatDau <= lich.NgayKetThuc &&
                            l.NgayKetThuc >= lich.NgayBatDau &&
                            l.GioBatDau < lich.GioKetThuc &&
                            l.GioKetThuc > lich.GioBatDau &&
                            (l.MaPhong == lich.MaPhong ||
                             l.MaLop == lich.MaLop ||
                             l.LopHoc.MaGV == lop.MaGV))
                .Select(l => new
                {
                    l.MaLop,
                    l.MaPhong,
                    MaGV = l.LopHoc.MaGV,
                    l.NgayBatDau,
                    l.NgayKetThuc,
                    l.GioBatDau,
                    l.GioKetThuc
                }).ToList();

            foreach (var cu in cacLichCoTheTrung)
            {
                var tu = cu.NgayBatDau > lich.NgayBatDau
                    ? cu.NgayBatDau : lich.NgayBatDau;
                var den = cu.NgayKetThuc < lich.NgayKetThuc
                    ? cu.NgayKetThuc : lich.NgayKetThuc;
                if (!CoBuoiThucTe(lich.Thu, tu, den)) continue;

                string lyDo = cu.MaLop == lich.MaLop ? "cùng lớp học" :
                    cu.MaPhong == lich.MaPhong ? "trùng phòng học" : "trùng giáo viên";
                throw new InvalidOperationException(
                    $"Không thể xếp lịch: {lyDo}, trong khung " +
                    $"{cu.GioBatDau.ToString(@"hh\:mm")}-{cu.GioKetThuc.ToString(@"hh\:mm")} " +
                    $"(có ngày thực tế trùng từ {tu:dd/MM/yyyy} đến {den:dd/MM/yyyy}).");
            }
        }

        public int Them(LichHoc lich)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            KiemTraNghiepVu(db, lich, null);
            db.LichHocs.Add(lich);
            db.SaveChanges();
            tx.Commit();
            return lich.MaLH;
        }

        // Tạo nhiều quy tắc lịch trong CÙNG một giao dịch.
        // Một thứ bị trùng => không lưu bất kỳ thứ nào trong đợt tạo này.
        public int ThemNhieu(List<LichHoc> cacLich)
        {
            if (cacLich == null || cacLich.Count == 0)
                throw new ArgumentException("Vui lòng chọn ít nhất một thứ học.");
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            foreach (var lich in cacLich)
                KiemTraNghiepVu(db, lich, null);
            db.LichHocs.AddRange(cacLich);
            db.SaveChanges();
            tx.Commit();
            return cacLich.Count;
        }

        // Tạo bộ lịch đầu tiên và đồng bộ ngày kết thúc lớp trong một transaction.
        // Không cho tự động ghi đè lịch lớp đã tồn tại để tránh làm sai dữ liệu cũ.
        public int ThemNhieuVaCapNhatNgayLop(List<LichHoc> cacLich, DateTime ngayKetThuc, int soBuoiYeuCau)
        {
            if (cacLich == null || cacLich.Count == 0)
                throw new ArgumentException("Chưa chọn ngày học.");
            int maLop = cacLich[0].MaLop;
            if (cacLich.Any(l => l.MaLop != maLop || l.NgayKetThuc.Date != ngayKetThuc.Date))
                throw new ArgumentException("Các lịch phải cùng lớp và cùng ngày kết thúc.");
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            var lop = db.LopHocs.Include(l => l.KhoaHoc)
                .FirstOrDefault(l => l.MaLop == maLop)
                ?? throw new InvalidOperationException("Lớp không còn tồn tại.");
            if (db.LichHocs.Any(l => l.MaLop == maLop))
                throw new InvalidOperationException(
                    "Lớp đã có lịch. Không tự động thay ngày kết thúc để tránh thay đổi lịch cũ. " +
                    "Hãy xử lý các lịch hiện có trước khi tạo lại lịch toàn khóa.");
            if (lop.KhoaHoc.SoBuoi != soBuoiYeuCau || soBuoiYeuCau <= 0)
                throw new InvalidOperationException("Số buổi của chứng chỉ đã thay đổi; vui lòng tải lại.");
            foreach (var lich in cacLich)
                KiemTraNghiepVu(db, lich, null);
            lop.NgayKetThuc = ngayKetThuc.Date;
            db.LichHocs.AddRange(cacLich);
            db.SaveChanges();
            tx.Commit();
            return cacLich.Count;
        }

        public bool Sua(LichHoc lich)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            var cu = db.LichHocs.FirstOrDefault(l => l.MaLH == lich.MaLH);
            if (cu == null) return false;
            KiemTraNghiepVu(db, lich, cu);

            cu.MaLop = lich.MaLop;
            cu.MaPhong = lich.MaPhong;
            cu.Thu = lich.Thu;
            cu.GioBatDau = lich.GioBatDau;
            cu.GioKetThuc = lich.GioKetThuc;
            cu.NgayBatDau = lich.NgayBatDau;
            cu.NgayKetThuc = lich.NgayKetThuc;
            db.SaveChanges();
            tx.Commit();
            return true;
        }

        public bool Xoa(int maLH)
        {
            using var db = new TrungTamDbContext();
            var lich = db.LichHocs.FirstOrDefault(l => l.MaLH == maLH);
            if (lich == null) return false;
            // Chỉ xóa quy tắc xếp lịch; BuoiHoc/DiemDanh đã tạo ở bảng khác giữ nguyên.
            db.LichHocs.Remove(lich);
            db.SaveChanges();
            return true;
        }
    }
}
