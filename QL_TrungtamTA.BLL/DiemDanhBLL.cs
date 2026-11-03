using System;
using System.Collections.Generic;
using System.Linq;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTA.BLL
{
    public class DiemDanhBLL
    {
        private readonly DiemDanhDAL dal = new();

        public List<LuaChonLopDiemDanh> LayLopHocs() => dal.LayLopHocs();
        public List<DongBuoiHoc> LayBuoiHocs(int? maLop = null)
            => dal.LayBuoiHocs(maLop);
        public List<DongDiemDanh> LayDiemDanh(int maBuoi)
            => dal.LayDiemDanh(maBuoi);

        public int TaoBuoiHoc(int maLop, DateTime ngayHoc,
            TimeSpan gioBatDau, TimeSpan gioKetThuc, string? noiDung)
        {
            if (maLop <= 0)
                throw new ArgumentException("Vui lòng chọn lớp học.");
            if (gioBatDau < TimeSpan.Zero ||
                gioKetThuc > TimeSpan.FromDays(1) || gioBatDau >= gioKetThuc)
                throw new ArgumentException("Giờ kết thúc phải lớn hơn giờ bắt đầu.");
            noiDung = noiDung?.Trim();
            if (noiDung?.Length > 500)
                throw new ArgumentException("Nội dung buổi học tối đa 500 ký tự.");
            return dal.TaoBuoiHoc(maLop, ngayHoc.Date,
                gioBatDau, gioKetThuc, noiDung);
        }

        public int LuuDiemDanh(int maBuoi, List<CapNhatDiemDanh> danhSach)
        {
            if (maBuoi <= 0)
                throw new ArgumentException("Vui lòng chọn buổi học.");
            if (danhSach == null || danhSach.Count == 0)
                throw new ArgumentException("Buổi học chưa có học viên để điểm danh.");

            string[] trangThaiHopLe = { "CoMat", "Vang", "CoPhep", "DiTre" };
            foreach (var item in danhSach)
            {
                if (item.MaHV <= 0 || !trangThaiHopLe.Contains(item.TrangThai))
                    throw new ArgumentException(
                        "Hãy chọn Có mặt, Vắng, Có phép hoặc Đi trễ cho tất cả học viên.");
                item.GhiChu = item.GhiChu?.Trim();
                if (item.GhiChu?.Length > 255)
                    throw new ArgumentException("Ghi chú điểm danh tối đa 255 ký tự.");
            }
            return dal.LuuDiemDanh(maBuoi, danhSach);
        }

        public bool HuyBuoiHoc(int maBuoi)
        {
            if (maBuoi <= 0)
                throw new ArgumentException("Vui lòng chọn buổi học cần hủy.");
            return dal.HuyBuoiHoc(maBuoi);
        }
    }
}
