using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL
{
    public class PhongHocDAL
    {
        public List<PhongHoc> LayDanhSach()
        {
            using var db = new TrungTamDbContext();
            return db.PhongHocs.AsNoTracking().OrderBy(p => p.TenPhong).ToList();
        }
        public List<PhongHoc> TimKiem(string tuKhoa)
        {
            tuKhoa = (tuKhoa ?? "").Trim();
            if (tuKhoa.Length == 0) return LayDanhSach();
            using var db = new TrungTamDbContext();
            return db.PhongHocs.AsNoTracking().Where(p => p.TenPhong.Contains(tuKhoa))
                .OrderBy(p => p.TenPhong).ToList();
        }
        public PhongHoc? LayTheoMa(int maPhong)
        {
            using var db = new TrungTamDbContext();
            return db.PhongHocs.AsNoTracking().FirstOrDefault(p => p.MaPhong == maPhong);
        }
        private static void KiemTraTrungTen(TrungTamDbContext db, PhongHoc phong)
        {
            string ten = (phong.TenPhong ?? "").Trim().ToUpper();
            if (db.PhongHocs.Any(p => p.MaPhong != phong.MaPhong && p.TenPhong.ToUpper() == ten))
                throw new InvalidOperationException("Tên phòng đã tồn tại (không phân biệt hoa/thường).");
        }
        public bool DaDuocSuDung(int maPhong)
        {
            using var db = new TrungTamDbContext();
            return db.LopHocs.Any(l => l.MaPhongMacDinh == maPhong)
                || db.LichHocs.Any(l => l.MaPhong == maPhong);
        }
        public bool NgungSuDung(int maPhong)
        {
            using var db = new TrungTamDbContext();
            var phong = db.PhongHocs.FirstOrDefault(p => p.MaPhong == maPhong);
            if (phong == null) return false;
            phong.TrangThai = false;
            db.SaveChanges();
            return true;
        }
        public int Them(PhongHoc phong)
        {
            using var db = new TrungTamDbContext();
            KiemTraTrungTen(db, phong);
            db.PhongHocs.Add(phong);
            db.SaveChanges();
            return phong.MaPhong;
        }
        public bool Sua(PhongHoc phong)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            var cu = db.PhongHocs.FirstOrDefault(p => p.MaPhong == phong.MaPhong);
            if (cu == null) return false;
            KiemTraTrungTen(db, phong);

            // Không kiểm tra giới hạn cấu hình cũ (SiSoToiDa) vì giới hạn này
            // được tính lại từ sức chứa mới của phòng học.
            var lopMacDinh = db.LopHocs.Where(l => l.MaPhongMacDinh == phong.MaPhong).ToList();
            var maLopDungLich = db.LichHocs.Where(l => l.MaPhong == phong.MaPhong)
                .Select(l => l.MaLop).Distinct().ToList();
            var maLopCanKiemTra = lopMacDinh.Select(l => l.MaLop)
                .Concat(maLopDungLich).Distinct().ToList();
            foreach (int maLop in maLopCanKiemTra)
            {
                int dangHoc = db.DangKyHocs.Count(d => d.MaLop == maLop && d.TrangThai != "DaHuy");
                if (dangHoc > phong.SucChua)
                    throw new InvalidOperationException(
                        $"Lớp LH{maLop:D2} hiện có {dangHoc} học viên, " +
                        $"không thể giảm sức chứa phòng xuống {phong.SucChua}.");
            }

            // Cập nhật giá trị cache cho các lớp mặc định dùng phòng này;
            // các phân hệ Đăng ký học cũ vẫn đọc LopHoc.SiSoToiDa chính xác.
            foreach (var lop in lopMacDinh)
            {
                lop.SiSoToiDa = phong.SucChua;
                lop.SiSoToiThieu = 1;
            }
            cu.TenPhong = phong.TenPhong;
            cu.SucChua = phong.SucChua;
            cu.TrangThai = phong.TrangThai;
            db.SaveChanges();
            tx.Commit();
            return true;
        }
        public bool Xoa(int maPhong)
        {
            using var db = new TrungTamDbContext();
            var phong = db.PhongHocs.FirstOrDefault(p => p.MaPhong == maPhong);
            if (phong == null) return false;
            if (db.LichHocs.Any(l => l.MaPhong == maPhong) ||
                db.LopHocs.Any(l => l.MaPhongMacDinh == maPhong))
                throw new InvalidOperationException(
                    "Phòng đã được gán cho lớp hoặc lịch học. Hãy ngừng sử dụng thay vì xóa.");
            db.PhongHocs.Remove(phong);
            db.SaveChanges();
            return true;
        }
    }
}
