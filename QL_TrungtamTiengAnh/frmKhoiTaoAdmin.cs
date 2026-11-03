
using System;
using System.Drawing;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmKhoiTaoAdmin : Form
    {
        private readonly TaiKhoanBLL taiKhoanBLL = new();

        private readonly TextBox txtTaiKhoan = new();
        private readonly TextBox txtMatKhau = new();
        private readonly TextBox txtXacNhan = new();

        public frmKhoiTaoAdmin()
        {
            InitializeComponent();
            TaoGiaoDien();
        }

        private void TaoGiaoDien()
        {
            Text = "Thiết lập quản trị viên";
            ClientSize = new Size(520, 440);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10);
            Controls.Clear();

            Controls.Add(new Label
            {
                Text = "THIẾT LẬP QUẢN TRỊ VIÊN",
                Location = new Point(45, 25),
                Size = new Size(440, 45),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(24, 55, 85)
            });

            Controls.Add(new Label
            {
                Text = "Tạo tài khoản Admin đầu tiên cho hệ thống",
                Location = new Point(45, 75),
                AutoSize = true,
                ForeColor = Color.DimGray
            });

            ThemO("Tên đăng nhập", txtTaiKhoan, 120, false);
            ThemO("Mật khẩu", txtMatKhau, 205, true);
            ThemO("Xác nhận mật khẩu", txtXacNhan, 290, true);

            Button btnTao = new Button
            {
                Text = "TẠO TÀI KHOẢN ADMIN",
                Location = new Point(45, 365),
                Size = new Size(430, 48),
                BackColor = Color.FromArgb(30, 100, 170),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnTao.FlatAppearance.BorderSize = 0;
            btnTao.Click += BtnTao_Click;

            Controls.Add(btnTao);
            AcceptButton = btnTao;
        }

        private void ThemO(
            string nhan, TextBox txt, int y, bool laMatKhau)
        {
            Controls.Add(new Label
            {
                Text = nhan,
                Location = new Point(45, y),
                Size = new Size(430, 26)
            });

            txt.Location = new Point(45, y + 30);
            txt.Size = new Size(430, 32);
            txt.Font = new Font("Segoe UI", 12);
            txt.UseSystemPasswordChar = laMatKhau;
            txt.BorderStyle = BorderStyle.FixedSingle;

            Controls.Add(txt);
        }

        private void BtnTao_Click(object? sender, EventArgs e)
        {
            if (txtMatKhau.Text != txtXacNhan.Text)
            {
                MessageBox.Show(
                    "Mật khẩu xác nhận không khớp!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                taiKhoanBLL.TaoAdminDauTien(
                    txtTaiKhoan.Text,
                    txtMatKhau.Text);

                MessageBox.Show(
                    "Đã tạo tài khoản Admin thành công!",
                    "Thành công",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Dữ liệu không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Không thể tạo tài khoản Admin. " +
                    "Vui lòng kiểm tra cơ sở dữ liệu " +
                    "hoặc tài khoản đã tồn tại.",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
