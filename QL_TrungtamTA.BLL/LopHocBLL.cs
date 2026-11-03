using System;
using System.Collections.Generic;
using System.Linq;
using QL_TrungtamTA.DAL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.BLL
{
    public class LopHocBLL
    {
        private readonly LopHocDAL dal = new();

        public List<LopHoc> LayDanhSach() => dal.LayDanhSach();
        public List<LopHoc> TimKiem(string tuKhoa) => dal.TimKiem(tuKhoa);
        public List<KhoaHoc> LayKhoaHocs() => dal.LayKhoaHocs();
        public List<GiaoVien> LayGiaoViens() => dal.LayGiaoViens();
        public List<PhongHoc> LayPhongHocs() => dal.LayPhongHocs();
        public List<DongLopHocTongHop> LayBangTongHop() => dal.LayBangTongHop();
        public List<HocVienTrongLop> LayHocVienTrongLop(int maLop) => maLop > 0
            ? dal.LayHocVienTrongLop(maLop)
            : throw new ArgumentException("Chọn lớp trước khi xem học viên.");
        public string GoiYTenLop(int maKH) => maKH > 0 ? dal.GoiYTenLop(maKH) : "";
        public LopHoc? LayTheoMa(int maLop) => maLop > 0 ? dal.LayTheoMa(maLop)
            : throw new ArgumentException("Mã lớp không hợp lệ.");

        private void Validate(LopHoc l)
        {
            ArgumentNullException.ThrowIfNull(l);
            l.TenLop = (l.TenLop ?? "").Trim();
            if (l.TenLop.Length < 2 || l.TenLop.Length > 100)
                throw new ArgumentException("Tên lớp cần từ 2–100 ký tự.");
            if (l.MaKH <= 0)
                throw new ArgumentException("Bạn chưa chọn chứng chỉ/khóa học.");
            if (l.MaGV <= 0)
                throw new ArgumentException("Bạn chưa chọn giáo viên.");
            if (!l.MaPhongMacDinh.HasValue || l.MaPhongMacDinh.Value <= 0)
                throw new ArgumentException("Bạn chưa chọn phòng chính cho lớp. Hãy chọn phòng trước khi lưu.");

            // Phòng chính phải tồn tại và đang sử dụng.
            var phong = dal.LayPhongHocs()
                .FirstOrDefault(p => p.MaPhong == l.MaPhongMacDinh.Value);
            if (phong == null || !phong.TrangThai)
                throw new ArgumentException("Phòng chính không tồn tại hoặc đã ngừng sử dụng.");

            l.NgayBatDau = l.NgayBatDau.Date;
            l.NgayKetThuc = l.NgayKetThuc.Date;
            if (l.NgayKetThuc <= l.NgayBatDau)
                throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");
            if (l.HocPhiRieng is < 0m or > 1_000_000_000m)
                throw new ArgumentException("Học phí riêng không hợp lệ (0–1 tỷ đồng).");
            l.GhiChu = string.IsNullOrWhiteSpace(l.GhiChu) ? null : l.GhiChu.Trim();
            if (l.GhiChu?.Length > 500)
                throw new ArgumentException("Ghi chú không vượt 500 ký tự.");
            if (Array.IndexOf(new[] { "SapMo", "DangHoc", "DaKetThuc", "DaHuy" }, l.TrangThai) < 0)
                throw new ArgumentException("Trạng thái lớp không hợp lệ.");
            // Sức chứa từ PhongHoc.SucChua, kiểm tra ở DAL khi lưu.
        }

        public int Them(LopHoc l)
        {
            Validate(l);
            l.MaLop = 0;
            return dal.Them(l);
        }

        public bool Sua(LopHoc l)
        {
            if (l.MaLop <= 0)
                throw new ArgumentException("Vui lòng chọn lớp để sửa.");
            Validate(l);
            // LopHocDAL.Sua chịu trách nhiệm kiểm tra lịch đang tồn tại,
            // xung đột phòng, buổi học và cập nhật đồng bộ trong transaction.
            return dal.Sua(l);
        }

        public bool Xoa(int maLop) => maLop > 0 ? dal.Xoa(maLop)
            : throw new ArgumentException("Vui lòng chọn lớp để xóa.");

        public int CapNhatTrangThaiTheoNgay() => dal.CapNhatTrangThaiTheoNgay(DateTime.Today);
    }
}
