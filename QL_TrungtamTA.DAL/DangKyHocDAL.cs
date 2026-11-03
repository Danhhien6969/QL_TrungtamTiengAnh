using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Data;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL
{
    // DTO chỉ đọc: dữ liệu hiển thị trong DataGridView.
    public sealed class DongDangKyHoc
    {
        public int MaDK { get; set; }
        public int MaHV { get; set; }
        public int MaLop { get; set; }
        public string TenHocVien { get; set; } = "";
        public string TenLop { get; set; } = "";
        public string TenKhoaHoc { get; set; } = "";
        public DateTime NgayDangKy { get; set; }
        public decimal HocPhiPhaiDong { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo => HocPhiPhaiDong - DaThanhToan;
        public string TrangThai { get; set; } = "";
    }

    public sealed class LuaChonHocVien
    {
        public int MaHV { get; set; }
        public string HienThi { get; set; } = "";
    }

    public sealed class LuaChonLopHoc
    {
        public int MaLop { get; set; }
        public string HienThi { get; set; } = "";
        public decimal HocPhi { get; set; }
    }

    public class DangKyHocDAL
    {
        private static IQueryable<DongDangKyHoc> TaoTruyVan(TrungTamDbContext db)
        {
            return db.DangKyHocs.AsNoTracking()
                .Select(d => new DongDangKyHoc
                {
                    MaDK = d.MaDK,
                    MaHV = d.MaHV,
                    MaLop = d.MaLop,
                    TenHocVien = d.HocVien.HoTen,
                    TenLop = d.LopHoc.TenLop,
                    TenKhoaHoc = d.LopHoc.KhoaHoc.TenKH,
                    NgayDangKy = d.NgayDangKy,
                    HocPhiPhaiDong = d.HocPhiPhaiDong,
                    DaThanhToan = d.ThanhToanHocPhis
                        .Sum(t => (decimal?)t.SoTien) ?? 0m,
                    TrangThai = d.TrangThai
                });
        }

        public List<DongDangKyHoc> LayDanhSach(string tuKhoa = "")
        {
            using var db = new TrungTamDbContext();
            var query = TaoTruyVan(db);
            tuKhoa = (tuKhoa ?? "").Trim();

            if (tuKhoa.Length > 0)
            {
                query = query.Where(d =>
                    d.TenHocVien.Contains(tuKhoa) ||
                    d.TenLop.Contains(tuKhoa) ||
                    d.TenKhoaHoc.Contains(tuKhoa));
            }

            return query.OrderByDescending(d => d.MaDK).ToList();
        }

        // Lấy dữ liệu cho ComboBox; có cả bản ghi ngừng hoạt động
        // để những đăng ký cũ vẫn hiển thị được khi người dùng chọn dòng.
        public List<LuaChonHocVien> LayHocViens()
        {
            using var db = new TrungTamDbContext();
            return db.HocViens.AsNoTracking()
                .OrderBy(h => h.HoTen).ThenBy(h => h.MaHV)
                .Select(h => new { h.MaHV, h.HoTen, h.TrangThai })
                .ToList()
                .Select(h => new LuaChonHocVien
                {
                    MaHV = h.MaHV,
                    HienThi = h.HoTen + " (HV " + h.MaHV + ")" +
                        (h.TrangThai ? "" : " - ngừng hoạt động")
                }).ToList();
        }

        public List<LuaChonLopHoc> LayLopHocs()
        {
            using var db = new TrungTamDbContext();
            return db.LopHocs.AsNoTracking()
                .OrderBy(l => l.TenLop)
                .Select(l => new {
                    l.MaLop,
                    l.TenLop,
                    TenKH = l.KhoaHoc.TenKH,
                    HocPhi = l.HocPhiRieng ?? l.KhoaHoc.HocPhi
                })
                .ToList()
                .Select(l => new LuaChonLopHoc
                {
                    MaLop = l.MaLop,
                    HienThi = l.TenLop + " - " + l.TenKH +
                        " (lớp " + l.MaLop + ")",
                    HocPhi = l.HocPhi
                }).ToList();
        }

        // Thực hiện kiểm tra và lưu trong một transaction để tránh
        // vượt sĩ số khi hai nhân viên cùng thêm học viên.
        public int Them(int maHV, int maLop)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);

            var hv = db.HocViens.AsNoTracking()
                .FirstOrDefault(h => h.MaHV == maHV);
            if (hv == null || !hv.TrangThai)
                throw new InvalidOperationException(
                    "Học viên không tồn tại hoặc đã ngừng hoạt động.");

            var lop = db.LopHocs.AsNoTracking()
                .Include(l => l.KhoaHoc)
                .FirstOrDefault(l => l.MaLop == maLop);
            if (lop == null)
                throw new InvalidOperationException("Lớp học không tồn tại.");
            if (lop.TrangThai != "SapMo" && lop.TrangThai != "DangHoc")
                throw new InvalidOperationException(
                    "Chỉ đăng ký cho lớp sắp mở hoặc đang học.");
            if (!lop.KhoaHoc.TrangThai)
                throw new InvalidOperationException("Khóa học đã ngừng hoạt động.");

            // Chỉ được có một bản ghi đăng ký cho mỗi (học viên, lớp),
            // kể cả khi đăng ký trước đã bị hủy (ràng buộc UNIQUE của DB).
            if (db.DangKyHocs.Any(d => d.MaHV == maHV && d.MaLop == maLop))
                throw new InvalidOperationException(
                    "Học viên đã có đăng ký lớp này. " +
                    "Nếu đã hủy, hãy dùng chức năng Khôi phục.");

            int soChoDaDung = db.DangKyHocs.Count(d =>
                d.MaLop == maLop && d.TrangThai != "DaHuy");
            if (soChoDaDung >= lop.SiSoToiDa)
                throw new InvalidOperationException("Lớp đã đủ sĩ số.");

            var dangKy = new DangKyHoc
            {
                MaHV = maHV,
                MaLop = maLop,
                NgayDangKy = DateTime.Now,
                HocPhiPhaiDong = lop.HocPhiRieng ?? lop.KhoaHoc.HocPhi,
                TrangThai = "DangHoc"
            };

            db.DangKyHocs.Add(dangKy);
            db.SaveChanges();
            tx.Commit();
            return dangKy.MaDK;
        }

        // Không cho sửa học viên/lớp của đăng ký đã có: làm vậy sẽ
        // ảnh hưởng dữ liệu thanh toán và lịch sử ghi nhận.
        public bool DoiTrangThai(int maDK, string trangThaiMoi)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);

            var dk = db.DangKyHocs.FirstOrDefault(d => d.MaDK == maDK);
            if (dk == null) return false;
            if (dk.TrangThai == trangThaiMoi)
                throw new InvalidOperationException("Đăng ký đã ở trạng thái này.");

            if (trangThaiMoi == "DaHuy" && dk.TrangThai == "DangHoc")
            {
                if (db.ThanhToanHocPhis.Any(t => t.MaDK == maDK))
                    throw new InvalidOperationException(
                        "Đăng ký đã có thanh toán. Cần xử lý hoàn tiền " +
                        "trước khi hủy để không mất đối soát học phí.");
            }
            else if (trangThaiMoi == "HoanThanh" && dk.TrangThai == "DangHoc")
            {
                // BLL/UI sau này có thể bổ sung điều kiện kết quả học tập.
            }
            else if (trangThaiMoi == "DangHoc" && dk.TrangThai == "DaHuy")
            {
                var hv = db.HocViens.AsNoTracking()
                    .FirstOrDefault(h => h.MaHV == dk.MaHV);
                var lop = db.LopHocs.AsNoTracking()
                    .Include(l => l.KhoaHoc)
                    .FirstOrDefault(l => l.MaLop == dk.MaLop);
                if (hv == null || !hv.TrangThai || lop == null ||
                    !lop.KhoaHoc.TrangThai ||
                    (lop.TrangThai != "SapMo" && lop.TrangThai != "DangHoc"))
                    throw new InvalidOperationException(
                        "Không thể khôi phục: học viên/khóa học không hoạt động " +
                        "hoặc lớp không còn nhận đăng ký.");

                int soChoDaDung = db.DangKyHocs.Count(d =>
                    d.MaLop == dk.MaLop && d.TrangThai != "DaHuy" && d.MaDK != maDK);
                if (soChoDaDung >= lop.SiSoToiDa)
                    throw new InvalidOperationException("Lớp đã đủ sĩ số.");
            }
            else
            {
                throw new InvalidOperationException(
                    "Không thể chuyển giữa hai trạng thái này.");
            }

            dk.TrangThai = trangThaiMoi;
            db.SaveChanges();
            tx.Commit();
            return true;
        }
    }
}
