using System;
using System.Collections.Generic;
using QL_TrungtamTA.DAL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.BLL
{
    public class KhoaHocBLL
    {
        private readonly KhoaHocDAL dal = new();
        public List<KhoaHoc> LayDanhSach() => dal.LayDanhSach();
        public List<KhoaHoc> TimKiem(string tuKhoa) => dal.TimKiem(tuKhoa);
        public List<KhoaHoc> LocDanhSach(string tuKhoa, bool? trangThai, string? dauVao, string? dauRa)
            => dal.LayDanhSach(tuKhoa, trangThai, dauVao, dauRa);
        public KhoaHoc? LayTheoMa(int maKH) => maKH > 0
            ? dal.LayTheoMa(maKH) : throw new ArgumentException("Mã khóa học không hợp lệ.");

        private static void KiemTraDuLieu(KhoaHoc kh)
        {
            ArgumentNullException.ThrowIfNull(kh);
            kh.TenKH = (kh.TenKH ?? "").Trim();
            if (kh.TenKH.Length < 2 || kh.TenKH.Length > 100)
                throw new ArgumentException("Chứng chỉ phải có tên từ 2 đến 100 ký tự.");
            kh.LoaiKhoa = (kh.LoaiKhoa ?? "").Trim();
            if (kh.LoaiKhoa.Length < 2 || kh.LoaiKhoa.Length > 60)
                throw new ArgumentException("Loại khóa phải từ 2 đến 60 ký tự.");
            kh.TrinhDoDauVao = (kh.TrinhDoDauVao ?? "").Trim().ToUpperInvariant();
            kh.TrinhDoDauRa = (kh.TrinhDoDauRa ?? "").Trim().ToUpperInvariant();
            if (kh.TrinhDoDauVao.Length == 0 || kh.TrinhDoDauVao.Length > 50 ||
                kh.TrinhDoDauRa.Length == 0 || kh.TrinhDoDauRa.Length > 50)
                throw new ArgumentException("Vui lòng nhập trình độ đầu vào và đầu ra (tối đa 50 ký tự).");
            string[] bac = { "A1", "A2", "B1", "B2", "C1", "C2" };
            int iVao = Array.IndexOf(bac, kh.TrinhDoDauVao);
            int iRa = Array.IndexOf(bac, kh.TrinhDoDauRa);
            if (iVao >= 0 && iRa >= 0 && iRa < iVao)
                throw new ArgumentException("Trình độ đầu ra không được thấp hơn đầu vào.");
            if (kh.ThoiLuongThang is not (3 or 6 or 9 or 12))
                throw new ArgumentException("Thời lượng phải chọn 3, 6, 9 hoặc 12 tháng.");
            if (kh.SoBuoi is null or < 1 or > 1000)
                throw new ArgumentException("Số buổi phải từ 1 đến 1000.");
            if (kh.SoGioMoiBuoi is null or < 1 or > 8)
                throw new ArgumentException("Số giờ/buổi phải từ 1 đến 8 giờ.");
            if (kh.HocPhi < 0m || kh.HocPhi > 1_000_000_000m)
                throw new ArgumentException("Học phí phải từ 0 đến 1 tỷ đồng.");
            kh.MoTa = string.IsNullOrWhiteSpace(kh.MoTa) ? null : kh.MoTa.Trim();
            if (kh.MoTa?.Length > 500)
                throw new ArgumentException("Mô tả không được vượt quá 500 ký tự.");
            kh.TrinhDo = kh.TrinhDoDauVao;
            checked { kh.ThoiLuong = kh.SoBuoi.Value * kh.SoGioMoiBuoi.Value; }
            if (kh.ThoiLuong > 5000)
                throw new ArgumentException("Tổng thời lượng vượt quá 5000 giờ.");
            // Không kiểm tra sĩ số khóa học: sức chứa nằm ở Phòng học.
        }

        public int Them(KhoaHoc kh)
        {
            KiemTraDuLieu(kh);
            kh.MaKH = 0;
            kh.TrangThai = true;
            return dal.Them(kh);
        }
        public bool Sua(KhoaHoc kh)
        {
            if (kh.MaKH <= 0) throw new ArgumentException("Vui lòng chọn chứng chỉ cần sửa.");
            KiemTraDuLieu(kh);
            return dal.Sua(kh);
        }
        public bool Xoa(int maKH) => maKH > 0 ? dal.Xoa(maKH)
            : throw new ArgumentException("Vui lòng chọn chứng chỉ cần xóa.");
        public bool DoiTrangThai(int maKH, bool trangThai) => maKH > 0
            ? dal.DoiTrangThai(maKH, trangThai)
            : throw new ArgumentException("Vui lòng chọn chứng chỉ.");
    }
}
