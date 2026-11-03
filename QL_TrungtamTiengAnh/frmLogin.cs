
using System;
using System.Drawing;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmLogin : Form
    {
        private readonly TaiKhoanBLL taiKhoanBLL = new();

        private readonly TextBox txtTenDangNhap = new();
        private readonly TextBox txtMatKhau = new();

        public frmLogin()
        {
            InitializeComponent();
            TaoGiaoDien();
        }

        private void TaoGiaoDien()
        {
            Text = "Đăng nhập - Quản lý trung tâm tiếng Anh";
            ClientSize = new Size(900, 520);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);
            Controls.Clear();

            // Bên trái: Giới thiệu hệ thống
            Panel pnlLeft = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(350, 520),
                BackColor = Color.FromArgb(23, 52, 83)
            };
            Controls.Add(pnlLeft);

            pnlLeft.Controls.Add(TaoLabel(
                "ENGLISH CENTER",
                35, 145, 310, 55,
                23, Color.White, true));

            pnlLeft.Controls.Add(TaoLabel(
                "MANAGEMENT SYSTEM",
                35, 205, 310, 40,
                13, Color.LightBlue, false));

            pnlLeft.Controls.Add(TaoLabel(
                "PHẦN MỀM QUẢN LÝ\nTRUNG TÂM TIẾNG ANH",
                35, 295, 300, 80,
                14, Color.White, true));

            pnlLeft.Controls.Add(TaoLabel(
                "C# WinForms  |  .NET 10",
                35, 465, 300, 30,
                10, Color.LightGray, false));

            // Bên phải: Form đăng nhập
            Controls.Add(TaoLabel(
                "ĐĂNG NHẬP",
                425, 95, 420, 50,
                25, Color.FromArgb(23, 52, 83), true));

            Controls.Add(TaoLabel(
                "Vui lòng nhập thông tin tài khoản",
                425, 150, 420, 30,
                11, Color.DimGray, false));

            Controls.Add(TaoLabel(
                "Tên đăng nhập",
                425, 215, 420, 30,
                11, Color.Black, true));

            txtTenDangNhap.Location = new Point(425, 250);
            txtTenDangNhap.Size = new Size(400, 35);
            txtTenDangNhap.Font = new Font("Segoe UI", 13);
            txtTenDangNhap.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(txtTenDangNhap);

            Controls.Add(TaoLabel(
                "Mật khẩu",
                425, 305, 420, 30,
                11, Color.Black, true));

            txtMatKhau.Location = new Point(425, 340);
            txtMatKhau.Size = new Size(400, 35);
            txtMatKhau.Font = new Font("Segoe UI", 13);
            txtMatKhau.UseSystemPasswordChar = true;
            txtMatKhau.BorderStyle = BorderStyle.FixedSingle;
            Controls.Add(txtMatKhau);

            CheckBox chkHienMatKhau = new CheckBox
            {
                Text = "Hiện mật khẩu",
                Location = new Point(425, 385),
                AutoSize = true
            };

            chkHienMatKhau.CheckedChanged += (s, e) =>
            {
                txtMatKhau.UseSystemPasswordChar =
                    !chkHienMatKhau.Checked;
            };

            Controls.Add(chkHienMatKhau);

            Button btnDangNhap = new Button
            {
                Text = "ĐĂNG NHẬP",
                Location = new Point(425, 430),
                Size = new Size(400, 48),
                BackColor = Color.FromArgb(30, 100, 170),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnDangNhap.FlatAppearance.BorderSize = 0;
            btnDangNhap.Click += BtnDangNhap_Click;
            Controls.Add(btnDangNhap);

            AcceptButton = btnDangNhap;
        }

        private static Label TaoLabel(
            string text, int x, int y, int w, int h,
            float size, Color color, bool bold)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                ForeColor = color,
                Font = new Font(
                    "Segoe UI", size,
                    bold ? FontStyle.Bold : FontStyle.Regular)
            };
        }

        // Khi mở phần mềm, kiểm tra tài khoản Admin
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            try
            {
                if (taiKhoanBLL.CanKhoiTaoAdmin())
                {
                    using var form = new frmKhoiTaoAdmin();

                    if (form.ShowDialog(this) != DialogResult.OK)
                    {
                        Close();
                        return;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Không thể kiểm tra tài khoản. " +
                    "Vui lòng kiểm tra kết nối SQL Server.",
                    "Lỗi kết nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                Close();
            }
        }

        private void BtnDangNhap_Click(
            object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTenDangNhap.Text) ||
                string.IsNullOrWhiteSpace(txtMatKhau.Text))
            {
                MessageBox.Show(
                    "Vui lòng nhập tên đăng nhập và mật khẩu!");
                return;
            }

            try
            {
                PhienDangNhap? phien =
                    taiKhoanBLL.DangNhap(
                        txtTenDangNhap.Text,
                        txtMatKhau.Text);

                if (phien == null)
                {
                    MessageBox.Show(
                        "Tài khoản hoặc mật khẩu không đúng, " +
                        "hoặc tài khoản đã bị khóa.",
                        "Đăng nhập thất bại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                Hide();

                using var main = new frmMain(phien);
                DialogResult result = main.ShowDialog();

                if (result == DialogResult.Retry)
                {
                    txtMatKhau.Clear();
                    Show();
                    txtTenDangNhap.Focus();
                }
                else
                {
                    Close();
                }
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Không thể đăng nhập. " +
                    "Vui lòng kiểm tra kết nối SQL Server.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
