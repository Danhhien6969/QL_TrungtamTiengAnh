using System;
using System.Collections.Generic;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTA.BLL
{
    public class HocPhiBLL
    {
        private readonly HocPhiDAL dal = new();

        public List<DongCongNoHocPhi> LayCongNo(string tuKhoa = "") => dal.LayCongNo(tuKhoa);
        public List<LuaChonCongNo> LayDangKy() => dal.LayDangKy();
        public List<DongLichSuHocPhi> LayLichSu(int? maDK = null)
        {
            if (maDK.HasValue && maDK.Value <= 0)
                throw new ArgumentException("Mã đăng ký không hợp lệ.");
            return dal.LayLichSu(maDK);
        }

        public int GhiNhan(int maDK, decimal soTien, string hinhThuc, string? ghiChu)
        {
            if (maDK <= 0) throw new ArgumentException("Vui lòng chọn đăng ký học.");
            if (soTien <= 0m) throw new ArgumentException("Số tiền thanh toán phải lớn hơn 0.");
            if (soTien != decimal.Round(soTien, 2))
                throw new ArgumentException("Số tiền không được có quá 2 chữ số thập phân.");
            if (hinhThuc != "TienMat" && hinhThuc != "ChuyenKhoan" && hinhThuc != "The")
                throw new ArgumentException("Hình thức thanh toán không hợp lệ.");
            ghiChu = ghiChu?.Trim();
            if (ghiChu?.Length > 255) throw new ArgumentException("Ghi chú tối đa 255 ký tự.");
            return dal.GhiNhan(maDK, soTien, hinhThuc, ghiChu);
        }

        public decimal LayTongDemo(int maDK)
        {
            if (maDK <= 0) throw new ArgumentException("Mã đăng ký không hợp lệ.");
            return dal.LayTongDemo(maDK);
        }

        public List<DongLichSuDemo> LayLichSuDemo(int? maDK = null)
        {
            if (maDK.HasValue && maDK.Value <= 0)
                throw new ArgumentException("Mã đăng ký không hợp lệ.");
            return dal.LayLichSuDemo(maDK);
        }

        public void LuuThanhToanDemo(int maDK, decimal soTien,
            string maGiaoDich, string ghiChu)
        {
            if (maDK <= 0) throw new ArgumentException("Mã đăng ký không hợp lệ.");
            if (soTien <= 0 || soTien != decimal.Truncate(soTien))
                throw new ArgumentException("Số tiền DEMO phải là số nguyên VNĐ dương.");
            if (string.IsNullOrWhiteSpace(maGiaoDich) ||
                !maGiaoDich.StartsWith("DEMO-HP-", StringComparison.Ordinal) ||
                maGiaoDich.Length > 50)
                throw new ArgumentException("Mã giao dịch DEMO không hợp lệ.");
            if (ghiChu == null || ghiChu.Length > 255)
                throw new ArgumentException("Ghi chú tối đa 255 ký tự.");
            dal.LuuThanhToanDemo(maDK, soTien, maGiaoDich, ghiChu);
        }
    }
}
