using System;
using System.Collections.Generic;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTA.BLL
{
    public class DangKyHocBLL
    {
        private readonly DangKyHocDAL dal = new();

        public List<DongDangKyHoc> LayDanhSach(string tuKhoa = "")
            => dal.LayDanhSach(tuKhoa);

        public List<LuaChonHocVien> LayHocViens()
            => dal.LayHocViens();

        public List<LuaChonLopHoc> LayLopHocs()
            => dal.LayLopHocs();

        public int Them(int maHV, int maLop)
        {
            if (maHV <= 0)
                throw new ArgumentException("Vui lòng chọn học viên.");
            if (maLop <= 0)
                throw new ArgumentException("Vui lòng chọn lớp học.");
            return dal.Them(maHV, maLop);
        }

        public bool Huy(int maDK)
        {
            if (maDK <= 0)
                throw new ArgumentException("Vui lòng chọn đăng ký cần hủy.");
            return dal.DoiTrangThai(maDK, "DaHuy");
        }

        public bool KhoiPhuc(int maDK)
        {
            if (maDK <= 0)
                throw new ArgumentException("Vui lòng chọn đăng ký cần khôi phục.");
            return dal.DoiTrangThai(maDK, "DangHoc");
        }

        public bool HoanThanh(int maDK)
        {
            if (maDK <= 0)
                throw new ArgumentException("Vui lòng chọn đăng ký cần hoàn thành.");
            return dal.DoiTrangThai(maDK, "HoanThanh");
        }
    }
}
