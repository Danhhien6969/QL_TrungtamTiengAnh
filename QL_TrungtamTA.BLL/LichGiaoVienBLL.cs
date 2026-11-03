
using System;
using System.Collections.Generic;
using System.Linq;
using QL_TrungtamTA.DAL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.BLL
{
    public sealed class DongLichDayGiaoVien
    {
        public int MaGV { get; set; }
        public int MaLop { get; set; }
        public int MaLH { get; set; }

        public DateTime NgayHoc { get; set; }

        public string Ngay =>
            NgayHoc.ToString("dd/MM/yyyy");

        public string Thu => NgayHoc.DayOfWeek switch
        {
            DayOfWeek.Monday => "Thứ Hai",
            DayOfWeek.Tuesday => "Thứ Ba",
            DayOfWeek.Wednesday => "Thứ Tư",
            DayOfWeek.Thursday => "Thứ Năm",
            DayOfWeek.Friday => "Thứ Sáu",
            DayOfWeek.Saturday => "Thứ Bảy",
            _ => "Chủ Nhật"
        };

        public string GiaoVien { get; set; } = "";
        public string LopHoc { get; set; } = "";
        public string ChungChi { get; set; } = "";
        public string PhongHoc { get; set; } = "";

        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }

        public string GioHoc =>
            $"{GioBatDau:hh\\:mm} - {GioKetThuc:hh\\:mm}";

        public string TrangThai { get; set; } =
            "Theo lịch kế hoạch";
    }

    public class LichGiaoVienBLL
    {
        private readonly LichHocDAL lichDal = new();
        private readonly LopHocDAL lopDal = new();

        public List<GiaoVien> LayGiaoViens()
        {
            return lopDal.LayGiaoViens()
                .OrderBy(x => x.HoTen)
                .ToList();
        }

        // Quy đổi ngày của C# sang thứ
        // được lưu trong bảng LichHoc:
        // Thứ Hai = 2 ... Chủ Nhật = 8.
        private static int LayThu(DateTime ngay)
        {
            return ngay.DayOfWeek == DayOfWeek.Sunday
                ? 8
                : (int)ngay.DayOfWeek + 1;
        }

        public List<DongLichDayGiaoVien> LayLichTheoTuan(
            DateTime ngayBatDauTuan,
            int? maGV = null)
        {
            if (maGV.HasValue && maGV.Value <= 0)
                throw new ArgumentException(
                    "Mã giáo viên không hợp lệ.");

            DateTime tuNgay = ngayBatDauTuan.Date;
            DateTime denNgay = tuNgay.AddDays(6);

            // Lấy dữ liệu từ các bảng hiện có.
            var cacLop = lopDal.LayDanhSach()
                .ToDictionary(x => x.MaLop);

            var cacLich = lichDal.LayDanhSach();

            var cacPhong = lichDal.LayPhongHocs()
                .ToDictionary(x => x.MaPhong);

            var ketQua = new List<DongLichDayGiaoVien>();

            foreach (var lich in cacLich)
            {
                if (!cacLop.TryGetValue(
                    lich.MaLop, out var lop))
                    continue;

                // Không hiển thị lớp đã hủy.
                if (lop.TrangThai == "DaHuy")
                    continue;

                if (maGV.HasValue && lop.MaGV != maGV.Value)
                    continue;

                DateTime dau = lich.NgayBatDau.Date > tuNgay
                    ? lich.NgayBatDau.Date
                    : tuNgay;

                DateTime cuoi = lich.NgayKetThuc.Date < denNgay
                    ? lich.NgayKetThuc.Date
                    : denNgay;

                if (dau > cuoi)
                    continue;

                cacPhong.TryGetValue(
                    lich.MaPhong, out var phong);

                for (DateTime ngay = dau;
                     ngay <= cuoi;
                     ngay = ngay.AddDays(1))
                {
                    if (LayThu(ngay) != lich.Thu)
                        continue;

                    ketQua.Add(new DongLichDayGiaoVien
                    {
                        MaGV = lop.MaGV,
                        MaLop = lop.MaLop,
                        MaLH = lich.MaLH,
                        NgayHoc = ngay,
                        GiaoVien =
                            lop.GiaoVien?.HoTen ?? "",
                        LopHoc = lop.TenLop,
                        ChungChi =
                            lop.KhoaHoc?.TenKH ?? "",
                        PhongHoc =
                            phong?.TenPhong ?? "Chưa xác định",
                        GioBatDau = lich.GioBatDau,
                        GioKetThuc = lich.GioKetThuc
                    });
                }
            }

            return ketQua
                .OrderBy(x => x.NgayHoc)
                .ThenBy(x => x.GioBatDau)
                .ThenBy(x => x.GiaoVien)
                .ToList();
        }

        // Phát hiện giáo viên bị xếp trùng giờ
        // trong danh sách lịch của một tuần.
        public List<string> KiemTraTrungLich(
            List<DongLichDayGiaoVien> danhSach)
        {
            var canhBao = new HashSet<string>();

            foreach (var nhom in danhSach
                .GroupBy(x => new
                {
                    x.MaGV,
                    x.NgayHoc
                }))
            {
                var ds = nhom
                    .OrderBy(x => x.GioBatDau)
                    .ToList();

                for (int i = 0; i < ds.Count; i++)
                {
                    for (int j = i + 1; j < ds.Count; j++)
                    {
                        // Chỉ tính trùng khi hai khoảng
                        // thời gian giao nhau.
                        if (ds[i].GioBatDau < ds[j].GioKetThuc
                            && ds[j].GioBatDau <
                               ds[i].GioKetThuc)
                        {
                            canhBao.Add(
                                $"{ds[i].GiaoVien} ngày " +
                                $"{ds[i].Ngay}: " +
                                $"{ds[i].LopHoc} trùng " +
                                $"{ds[j].LopHoc}.");
                        }
                    }
                }
            }

            return canhBao.ToList();
        }

        public static DateTime LayDauTuan(DateTime ngay)
        {
            int doLech =
                ((int)ngay.DayOfWeek + 6) % 7;

            return ngay.Date.AddDays(-doLech);
        }
    }
}
