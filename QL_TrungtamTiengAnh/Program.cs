
using System;
using System.Windows.Forms;

namespace QL_TrungtamTiengAnh.GUI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Khởi động màn hình đăng nhập
            Application.Run(new frmLogin());
        }
    }
}
