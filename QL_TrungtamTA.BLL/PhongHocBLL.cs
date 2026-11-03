using System;
using System.Collections.Generic;
using QL_TrungtamTA.DAL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.BLL
{
    public class PhongHocBLL
    {
        private readonly PhongHocDAL dal = new();
        public List<PhongHoc> LayDanhSach() => dal.LayDanhSach();
        public List<PhongHoc> TimKiem(string tuKhoa) => dal.TimKiem(tuKhoa);
        public PhongHoc? LayTheoMa(int maPhong) => dal.LayTheoMa(maPhong);
        public bool DaDuocSuDung(int maPhong) => maPhong > 0 ? dal.DaDuocSuDung(maPhong)
            : throw new ArgumentException("Mã phòng không hợp lệ.");
        public bool NgungSuDung(int maPhong) => maPhong > 0 ? dal.NgungSuDung(maPhong)
            : throw new ArgumentException("Vui lòng chọn phòng.");

        private static void KiemTra(PhongHoc phong)
        {
            ArgumentNullException.ThrowIfNull(phong);
            phong.TenPhong = (phong.TenPhong ?? "").Trim();
            if (phong.TenPhong.Length == 0)
                throw new ArgumentException("Tên phòng không được để trống.");
            if (phong.TenPhong.Length > 50)
                throw new ArgumentException("Tên phòng không vượt quá 50 ký tự.");
            if (phong.SucChua < 1 || phong.SucChua > 200)
                throw new ArgumentException("Sức chứa phải từ 1 đến 200 người.");
        }
        public int Them(PhongHoc phong)
        {
            KiemTra(phong);
            phong.MaPhong = 0;
            return dal.Them(phong);
        }
        public bool Sua(PhongHoc phong)
        {
            if (phong.MaPhong <= 0) throw new ArgumentException("Vui lòng chọn phòng cần sửa.");
            KiemTra(phong);
            return dal.Sua(phong);
        }
        public bool Xoa(int maPhong) => maPhong > 0 ? dal.Xoa(maPhong)
            : throw new ArgumentException("Vui lòng chọn phòng cần xóa.");
    }
}
