using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QL_TrungtamTA.DAL.Data;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL
{
    public sealed class DongCongNoHocPhi
    {
        public int MaDK { get; set; }
        public string TenHocVien { get; set; } = "";
        public string TenLop { get; set; } = "";
        public string TenKhoaHoc { get; set; } = "";
        public decimal HocPhiPhaiDong { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo => HocPhiPhaiDong - DaThanhToan;
        public string TrangThai { get; set; } = "";
    }
    public sealed class LuaChonCongNo
    {
        public int MaDK { get; set; }
        public string HienThi { get; set; } = "";
        public decimal HocPhiPhaiDong { get; set; }
        public decimal DaThanhToan { get; set; }
        public decimal ConNo => HocPhiPhaiDong - DaThanhToan;
        public string TrangThai { get; set; } = "";
    }
    public sealed class DongLichSuHocPhi
    {
        public int MaTT { get; set; }
        public int MaDK { get; set; }
        public string TenHocVien { get; set; } = "";
        public string TenLop { get; set; } = "";
        public decimal SoTien { get; set; }
        public DateTime NgayThanhToan { get; set; }
        public string HinhThucTT { get; set; } = "";
        public string TenHinhThuc => HinhThucTT switch
        {
            "TienMat" => "Tiền mặt",
            "ChuyenKhoan" => "Chuyển khoản",
            "The" => "Thẻ",
            _ => HinhThucTT
        };
        public string GhiChu { get; set; } = "";
    }

    public sealed class DongLichSuDemo
    {
        public int MaDemo { get; set; }
        public int MaDK { get; set; }
        public string MaGiaoDich { get; set; } = "";
        public decimal SoTien { get; set; }
        public DateTime NgayXacNhan { get; set; }
        public string TrangThai { get; set; } = "";
        public string GhiChu { get; set; } = "";
    }

    public class HocPhiDAL
    {
        public List<DongCongNoHocPhi> LayCongNo(string tuKhoa = "")
        {
            using var db = new TrungTamDbContext();
            var query = db.DangKyHocs.AsNoTracking()
                .Select(d => new DongCongNoHocPhi
                {
                    MaDK = d.MaDK,
                    TenHocVien = d.HocVien.HoTen,
                    TenLop = d.LopHoc.TenLop,
                    TenKhoaHoc = d.LopHoc.KhoaHoc.TenKH,
                    HocPhiPhaiDong = d.HocPhiPhaiDong,
                    DaThanhToan = d.ThanhToanHocPhis.Sum(t => (decimal?)t.SoTien) ?? 0m,
                    TrangThai = d.TrangThai
                });
            tuKhoa = (tuKhoa ?? "").Trim();
            if (tuKhoa.Length > 0)
                query = query.Where(d => d.TenHocVien.Contains(tuKhoa)
                    || d.TenLop.Contains(tuKhoa) || d.TenKhoaHoc.Contains(tuKhoa));
            return query.OrderByDescending(d => d.MaDK).ToList();
        }

        public List<LuaChonCongNo> LayDangKy() => LayCongNo()
            .Where(d => d.TrangThai != "DaHuy")
            .Select(d => new LuaChonCongNo
            {
                MaDK = d.MaDK,
                HienThi = $"ĐK {d.MaDK} - {d.TenHocVien} - {d.TenLop}",
                HocPhiPhaiDong = d.HocPhiPhaiDong,
                DaThanhToan = d.DaThanhToan,
                TrangThai = d.TrangThai
            }).ToList();

        public List<DongLichSuHocPhi> LayLichSu(int? maDK = null)
        {
            using var db = new TrungTamDbContext();
            var query = db.ThanhToanHocPhis.AsNoTracking();
            if (maDK.HasValue) query = query.Where(t => t.MaDK == maDK.Value);
            return query.Select(t => new DongLichSuHocPhi
            {
                MaTT = t.MaTT,
                MaDK = t.MaDK,
                TenHocVien = t.DangKyHoc.HocVien.HoTen,
                TenLop = t.DangKyHoc.LopHoc.TenLop,
                SoTien = t.SoTien,
                NgayThanhToan = t.NgayThanhToan,
                HinhThucTT = t.HinhThucTT,
                GhiChu = t.GhiChu ?? ""
            })
                .OrderByDescending(t => t.NgayThanhToan)
                .ThenByDescending(t => t.MaTT).ToList();
        }

        public int GhiNhan(int maDK, decimal soTien, string hinhThuc, string? ghiChu)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            var dangKy = db.DangKyHocs.SingleOrDefault(d => d.MaDK == maDK);
            if (dangKy == null) throw new InvalidOperationException("Không tìm thấy đăng ký học.");
            if (dangKy.TrangThai != "DangHoc" && dangKy.TrangThai != "HoanThanh")
                throw new InvalidOperationException("Đăng ký đã hủy hoặc không hợp lệ để thu học phí.");
            decimal daThu = db.ThanhToanHocPhis.Where(t => t.MaDK == maDK)
                .Sum(t => (decimal?)t.SoTien) ?? 0m;
            decimal conNo = dangKy.HocPhiPhaiDong - daThu;
            if (conNo <= 0) throw new InvalidOperationException("Đăng ký này đã thanh toán đủ học phí.");
            if (soTien > conNo) throw new InvalidOperationException(
                $"Số tiền thu vượt số còn nợ ({conNo:N0} VNĐ). Vui lòng tải lại dữ liệu.");
            var thanhToan = new ThanhToanHocPhi
            {
                MaDK = maDK,
                SoTien = soTien,
                NgayThanhToan = DateTime.Now,
                HinhThucTT = hinhThuc,
                GhiChu = ghiChu
            };
            db.ThanhToanHocPhis.Add(thanhToan);
            db.SaveChanges();
            tx.Commit();
            return thanhToan.MaTT;
        }

        // Đọc số đã thanh toán mô phỏng; KHÔNG cộng vào thu tiền thật.
        public decimal LayTongDemo(int maDK)
        {
            using var db = new TrungTamDbContext();
            return db.Database.SqlQuery<decimal>($"""
                SELECT CAST(COALESCE(SUM(SoTien), 0) AS decimal(18,2)) AS Value
                FROM dbo.ThanhToanHocPhiDemo WHERE MaDK = {maDK}
                """).Single();
        }

        public List<DongLichSuDemo> LayLichSuDemo(int? maDK = null)
        {
            using var db = new TrungTamDbContext();
            // Dùng SQL có tham số để giữ tương thích với bảng tạo bằng SSMS.
            var query = db.Database.SqlQuery<DongLichSuDemo>($"""
                SELECT MaDemo, MaDK, MaGiaoDich, SoTien, NgayXacNhan,
                       TrangThai, CAST(COALESCE(GhiChu, N'') AS nvarchar(255)) AS GhiChu
                FROM dbo.ThanhToanHocPhiDemo
                WHERE ({maDK} IS NULL OR MaDK = {maDK})
                """);
            return query.AsEnumerable().OrderByDescending(x => x.NgayXacNhan)
                .ThenByDescending(x => x.MaDemo).ToList();
        }

        public void LuuThanhToanDemo(int maDK, decimal soTien,
            string maGiaoDich, string ghiChu)
        {
            using var db = new TrungTamDbContext();
            using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
            var dangKy = db.DangKyHocs.SingleOrDefault(x => x.MaDK == maDK);
            if (dangKy == null || (dangKy.TrangThai != "DangHoc" &&
                dangKy.TrangThai != "HoanThanh"))
                throw new InvalidOperationException("Đăng ký học không hợp lệ.");

            // Chặn ghi lặp mã QR: DB cũng có UNIQUE(MaGiaoDich).
            bool daCo = db.Database.SqlQuery<int>($"""
                SELECT COUNT(*) AS Value FROM dbo.ThanhToanHocPhiDemo
                WHERE MaGiaoDich = {maGiaoDich}
                """).Single() > 0;
            if (daCo) throw new InvalidOperationException("Giao dịch DEMO đã lưu trước đó.");

            decimal daThuThuc = db.ThanhToanHocPhis.Where(x => x.MaDK == maDK)
                .Sum(x => (decimal?)x.SoTien) ?? 0m;
            decimal daThuDemo = LayTongDemoTrongGiaoDich(db, maDK);
            decimal conNoDemo = dangKy.HocPhiPhaiDong - daThuThuc - daThuDemo;
            if (soTien <= 0 || soTien > conNoDemo)
                throw new InvalidOperationException(
                    $"Tiền DEMO vượt mức còn nợ thử nghiệm ({conNoDemo:N0}đ).");

            // SQL tương ứng chính xác bảng đã tạo trong SSMS; có tham số, không ghép chuỗi.
            db.Database.ExecuteSqlInterpolated($"""
                INSERT INTO dbo.ThanhToanHocPhiDemo
                    (MaDK, MaGiaoDich, SoTien, NgayXacNhan, HinhThuc, TrangThai, GhiChu)
                VALUES ({maDK}, {maGiaoDich}, {soTien}, SYSDATETIME(),
                        {"ChuyenKhoanDemo"}, {"DaXacNhanDemo"}, {ghiChu})
                """);
            tx.Commit();
        }

        private static decimal LayTongDemoTrongGiaoDich(TrungTamDbContext db, int maDK)
        {
            // Câu truy vấn cùng context/transaction Serializable ở trên.
            return db.Database.SqlQuery<decimal>($"""
                SELECT CAST(COALESCE(SUM(SoTien), 0) AS decimal(18,2)) AS Value
                FROM dbo.ThanhToanHocPhiDemo WITH (UPDLOCK, HOLDLOCK)
                WHERE MaDK = {maDK}
                """).Single();
        }
    }
}
