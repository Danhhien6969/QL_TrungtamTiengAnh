using System;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTA.BLL
{
    public class ThongKeBLL
    {
        private readonly ThongKeDAL dal = new();

        public DuLieuThongKe LayThongKe(int nam)
        {
            if (nam < 2000 || nam > 2100)
                throw new ArgumentOutOfRangeException(nameof(nam),
                    "Năm thống kê phải nằm trong khoảng 2000 đến 2100.");

            return dal.LayThongKe(nam);
        }
    }
}
