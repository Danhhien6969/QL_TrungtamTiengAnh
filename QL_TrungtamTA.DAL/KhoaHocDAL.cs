using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL
{
    public class KhoaHocDAL
    {
        public List<KhoaHoc> LayDanhSach(string tuKhoa = "", bool? trangThai = null,
            string? dauVao = null, string? dauRa = null)
        {
            using var db = new TrungTamDbContext();
            IQueryable<KhoaHoc> query = db.KhoaHocs.AsNoTracking();
            tuKhoa = (tuKhoa ?? "").Trim();
            if (tuKhoa.Length > 0)
                query = query.Where(k => k.TenKH.Contains(tuKhoa) ||
                    (k.LoaiKhoa != null && k.LoaiKhoa.Contains(tuKhoa)) ||
                    (k.TrinhDo != null && k.TrinhDo.Contains(tuKhoa)) ||
                    (k.TrinhDoDauVao != null && k.TrinhDoDauVao.Contains(tuKhoa)) ||
                    (k.TrinhDoDauRa != null && k.TrinhDoDauRa.Contains(tuKhoa)) ||
                    (k.MoTa != null && k.MoTa.Contains(tuKhoa)));
            if (trangThai.HasValue) query = query.Where(k => k.TrangThai == trangThai.Value);
            if (!string.IsNullOrWhiteSpace(dauVao))
            {
                string loc = dauVao.Trim();
                query = query.Where(k => (k.TrinhDoDauVao ?? k.TrinhDo) == loc);
            }
            if (!string.IsNullOrWhiteSpace(dauRa))
            {
                string loc = dauRa.Trim();
                query = query.Where(k => k.TrinhDoDauRa == loc);
            }
            return query.OrderByDescending(k => k.MaKH).ToList();
        }
        public KhoaHoc? LayTheoMa(int maKH)
        {
            using var db = new TrungTamDbContext();
            return db.KhoaHocs.AsNoTracking().FirstOrDefault(k => k.MaKH == maKH);
        }
        public List<KhoaHoc> TimKiem(string tuKhoa) => LayDanhSach(tuKhoa);
        private static void KiemTraTrungChungChi(TrungTamDbContext db, KhoaHoc khoa)
        {
            string ten = khoa.TenKH.Trim().ToLower();
            if (db.KhoaHocs.Any(k => k.MaKH != khoa.MaKH && k.TenKH.ToLower() == ten))
                throw new InvalidOperationException("Tên chứng chỉ đã tồn tại. Vui lòng chọn tên khác.");
        }
        public int Them(KhoaHoc khoa)
        {
            using var db = new TrungTamDbContext();
            KiemTraTrungChungChi(db, khoa);
            // Cột cũ vẫn giữ trong SQL/Model để tương thích, nhưng không còn là giới hạn.
            khoa.SiSoToiDa = null;
            db.KhoaHocs.Add(khoa);
            db.SaveChanges();
            return khoa.MaKH;
        }
        public bool Sua(KhoaHoc khoa)
        {
            using var db = new TrungTamDbContext();
            KhoaHoc? cu = db.KhoaHocs.FirstOrDefault(k => k.MaKH == khoa.MaKH);
            if (cu == null) return false;
            KiemTraTrungChungChi(db, khoa);
            cu.TenKH = khoa.TenKH;
            cu.TrinhDo = khoa.TrinhDo;
            cu.ThoiLuong = khoa.ThoiLuong;
            cu.HocPhi = khoa.HocPhi;
            cu.MoTa = khoa.MoTa;
            cu.ThoiLuongThang = khoa.ThoiLuongThang;
            cu.SoBuoi = khoa.SoBuoi;
            cu.SoGioMoiBuoi = khoa.SoGioMoiBuoi;
            cu.SiSoToiDa = null;
            cu.LoaiKhoa = khoa.LoaiKhoa;
            cu.TrinhDoDauVao = khoa.TrinhDoDauVao;
            cu.TrinhDoDauRa = khoa.TrinhDoDauRa;
            db.SaveChanges();
            return true;
        }
        public bool DoiTrangThai(int maKH, bool trangThaiMoi)
        {
            using var db = new TrungTamDbContext();
            KhoaHoc? khoa = db.KhoaHocs.FirstOrDefault(k => k.MaKH == maKH);
            if (khoa == null) return false;
            khoa.TrangThai = trangThaiMoi;
            db.SaveChanges();
            return true;
        }
        public bool Xoa(int maKH)
        {
            using var db = new TrungTamDbContext();
            KhoaHoc? khoa = db.KhoaHocs.FirstOrDefault(k => k.MaKH == maKH);
            if (khoa == null) return false;
            if (db.LopHocs.Any(l => l.MaKH == maKH))
                throw new InvalidOperationException("Chứng chỉ đã có lớp liên kết, hãy đóng tuyển sinh thay vì xóa.");
            db.KhoaHocs.Remove(khoa);
            db.SaveChanges();
            return true;
        }
    }
}
