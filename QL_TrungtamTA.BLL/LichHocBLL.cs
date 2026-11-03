using System;
using System.Collections.Generic;
using System.Linq;
using QL_TrungtamTA.DAL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.BLL
{
    public class LichHocBLL
    {
        private readonly LichHocDAL dal = new();
        public List<LichHoc> LayDanhSach() => dal.LayDanhSach();
        public List<LichHoc> TimKiem(string tuKhoa) => dal.TimKiem(tuKhoa);
        public LichHoc? LayTheoMa(int maLH) => dal.LayTheoMa(maLH);
        public List<LopHoc> LayLopHocs() => dal.LayLopHocs();
        public List<PhongHoc> LayPhongHocs() => dal.LayPhongHocs();

        private static int QuyDoiThu(DateTime ngay) =>
            ngay.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)ngay.DayOfWeek + 1;

        public static int DemSoBuoi(int thu, DateTime tuNgay, DateTime denNgay)
        {
            if (thu < 2 || thu > 8 || denNgay.Date < tuNgay.Date) return 0;
            var ngay = tuNgay.Date;
            int doLech = (thu - QuyDoiThu(ngay) + 7) % 7;
            ngay = ngay.AddDays(doLech);
            if (ngay > denNgay.Date) return 0;
            return 1 + (int)((denNgay.Date - ngay).TotalDays / 7);
        }

        // Không tự trừ ngày nghỉ lễ, nghỉ học, học bù.
        public static DateTime TinhNgayKetThuc(
            DateTime ngayKhaiGiang, IEnumerable<int> cacThu, int soBuoi)
        {
            if (soBuoi <= 0 || soBuoi > 2000)
                throw new ArgumentException("Số buổi phải từ 1 đến 2000.");
            var tapThu = cacThu.Distinct().ToHashSet();
            if (tapThu.Count == 0 || tapThu.Any(t => t < 2 || t > 8))
                throw new ArgumentException("Chọn ít nhất một thứ trong tuần hợp lệ.");
            DateTime ngay = ngayKhaiGiang.Date;
            int dem = 0;
            for (int i = 0; i < 14000; i++, ngay = ngay.AddDays(1))
                if (tapThu.Contains(QuyDoiThu(ngay)) && ++dem == soBuoi)
                    return ngay;
            throw new InvalidOperationException("Không tính được ngày kết thúc.");
        }

        // Phòng học chỉ nhận từ phòng chính của lớp, không nhận nhập tay từ lịch.
        private void KiemTraPhongChinh(LichHoc lich)
        {
            var lop = dal.LayLopHocs().FirstOrDefault(l => l.MaLop == lich.MaLop)
                ?? throw new InvalidOperationException("Lớp học không tồn tại.");
            if (!lop.MaPhongMacDinh.HasValue || lop.MaPhongMacDinh.Value <= 0)
                throw new InvalidOperationException(
                    $"Lớp {lop.TenLop} chưa có phòng chính. Hãy gán phòng tại Quản lý lớp học trước.");
            var phong = dal.LayPhongHocs().FirstOrDefault(p => p.MaPhong == lop.MaPhongMacDinh.Value);
            if (phong == null || !phong.TrangThai)
                throw new InvalidOperationException(
                    $"Phòng chính của lớp {lop.TenLop} đã ngừng hoạt động hoặc không tồn tại.");
            if (lich.MaPhong != lop.MaPhongMacDinh.Value)
                throw new InvalidOperationException(
                    $"Phòng lịch học không khớp phòng chính của lớp {lop.TenLop}. " +
                    "Hãy tải lại danh sách hoặc đổi phòng ở Quản lý lớp học.");
        }

        private void KiemTra(LichHoc lich)
        {
            ArgumentNullException.ThrowIfNull(lich);
            if (lich.MaLop <= 0)
                throw new ArgumentException("Vui lòng chọn lớp học.");
            if (lich.MaPhong <= 0)
                throw new ArgumentException("Vui lòng chọn phòng học.");
            KiemTraPhongChinh(lich);
            if (lich.Thu < 2 || lich.Thu > 8)
                throw new ArgumentException("Vui lòng chọn thứ trong tuần.");
            lich.NgayBatDau = lich.NgayBatDau.Date;
            lich.NgayKetThuc = lich.NgayKetThuc.Date;
            if (lich.NgayKetThuc < lich.NgayBatDau)
                throw new ArgumentException("Ngày kết thúc phải bằng hoặc sau ngày bắt đầu.");
            if (lich.GioBatDau < TimeSpan.Zero ||
                lich.GioKetThuc > TimeSpan.FromDays(1) ||
                lich.GioBatDau >= lich.GioKetThuc)
                throw new ArgumentException("Giờ bắt đầu phải nhỏ hơn giờ kết thúc.");
            if (DemSoBuoi(lich.Thu, lich.NgayBatDau, lich.NgayKetThuc) == 0)
                throw new ArgumentException("Khoảng ngày không có buổi nào đúng thứ đã chọn.");
        }

        public int Them(LichHoc lich)
        {
            KiemTra(lich);
            lich.MaLH = 0;
            return dal.Them(lich);
        }

        public int ThemNhieu(List<LichHoc> cacLich)
        {
            if (cacLich == null || cacLich.Count == 0)
                throw new ArgumentException("Vui lòng chọn ít nhất một thứ học.");
            if (cacLich.Select(l => l.Thu).Distinct().Count() != cacLich.Count)
                throw new ArgumentException("Không được chọn trùng thứ trong một lần xếp lịch.");
            if (cacLich.Select(l => l.MaLop).Distinct().Count() != 1)
                throw new ArgumentException("Các lịch trong một lần tạo phải thuộc cùng lớp.");
            foreach (var lich in cacLich)
            {
                KiemTra(lich);
                lich.MaLH = 0;
            }
            return dal.ThemNhieu(cacLich);
        }

        public int TaoLichTuTinhNgayKetThuc(List<LichHoc> cacLich, int soBuoi)
        {
            if (cacLich == null || cacLich.Count == 0)
                throw new ArgumentException("Vui lòng chọn ít nhất một thứ học.");
            if (cacLich.Select(l => l.Thu).Distinct().Count() != cacLich.Count)
                throw new ArgumentException("Không được chọn thứ trùng nhau.");
            if (cacLich.Select(l => l.MaLop).Distinct().Count() != 1)
                throw new ArgumentException("Các lịch phải cùng một lớp.");
            DateTime ngayDau = cacLich[0].NgayBatDau.Date;
            if (cacLich.Any(l => l.NgayBatDau.Date != ngayDau))
                throw new ArgumentException("Các lịch phải có cùng ngày bắt đầu.");
            DateTime ngayCuoi = TinhNgayKetThuc(
                ngayDau, cacLich.Select(l => l.Thu), soBuoi);
            foreach (var lich in cacLich)
            {
                lich.NgayKetThuc = ngayCuoi;
                lich.MaLH = 0;
                KiemTra(lich);
            }
            int dem = cacLich.Sum(l => DemSoBuoi(l.Thu, ngayDau, ngayCuoi));
            if (dem != soBuoi)
                throw new InvalidOperationException("Số buổi tính toán không khớp yêu cầu.");
            return dal.ThemNhieuVaCapNhatNgayLop(cacLich, ngayCuoi, soBuoi);
        }

        public bool Sua(LichHoc lich)
        {
            if (lich.MaLH <= 0)
                throw new ArgumentException("Vui lòng chọn lịch cần sửa.");
            KiemTra(lich);
            return dal.Sua(lich);
        }

        public bool Xoa(int maLH)
        {
            if (maLH <= 0)
                throw new ArgumentException("Vui lòng chọn lịch cần xóa.");
            return dal.Xoa(maLH);
        }
    }
}
