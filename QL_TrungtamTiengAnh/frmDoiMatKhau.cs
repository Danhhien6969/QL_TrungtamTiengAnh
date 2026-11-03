using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmDoiMatKhau : Form
    {
        private readonly PhienDangNhap phien;
        private readonly QuanLyTaiKhoanBLL bll = new();
        private readonly TextBox txtMatKhauCu = new();
        private readonly TextBox txtMatKhauMoi = new();
        private readonly TextBox txtXacNhan = new();


        public frmDoiMatKhau(PhienDangNhap phienDangNhap)
        {
            phien = phienDangNhap;
            TaoGiaoDien();
        }


        private void TaoGiaoDien()
        {
            Text = "Đổi mật khẩu";
            ClientSize = new Size(850, 580);
            MinimumSize = new Size(650, 500);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);
            Controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                ColumnCount = 1,
                RowCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            var header = new Panel { Dock = DockStyle.Fill };
            header.Controls.Add(new Label
            {
                Text = "ĐỔI MẬT KHẨU",
                Location = new Point(5, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 23, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });
            header.Controls.Add(new Label
            {
                Text = $"Tài khoản: {phien.TenDangNhap} · Mật khẩu mới tối thiểu 12 ký tự",
                Location = new Point(7, 52),
                AutoSize = true,
                ForeColor = Color.DimGray
            });
            layout.Controls.Add(header, 0, 0);

            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(30)
            };
            layout.Controls.Add(pnl, 0, 1);

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 325,
                ColumnCount = 2,
                RowCount = 4
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 68));
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 85));
            pnl.Controls.Add(grid);

            ThemTruong(grid, "Mật khẩu hiện tại (*)", txtMatKhauCu, 0);
            ThemTruong(grid, "Mật khẩu mới (*)", txtMatKhauMoi, 1);
            ThemTruong(grid, "Xác nhận mật khẩu (*)", txtXacNhan, 2);

            var chkHien = new CheckBox
            {
                Text = "Hiện mật khẩu",
                AutoSize = true,
                Margin = new Padding(5, 12, 0, 0)
            };
            chkHien.CheckedChanged += (_, _) =>
            {
                txtMatKhauCu.UseSystemPasswordChar = !chkHien.Checked;
                txtMatKhauMoi.UseSystemPasswordChar = !chkHien.Checked;
                txtXacNhan.UseSystemPasswordChar = !chkHien.Checked;
            };
            grid.Controls.Add(chkHien, 1, 3);

            var btn = new Button
            {
                Text = "LƯU MẬT KHẨU MỚI",
                Dock = DockStyle.Bottom,
                Height = 48,
                BackColor = Color.FromArgb(30, 100, 170),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += (_, _) => LuuMatKhau();
            pnl.Controls.Add(btn);
            AcceptButton = btn;
        }

        private static void ThemTruong(TableLayoutPanel grid, string nhan, TextBox input, int dong)
        {
            grid.Controls.Add(new Label
            {
                Text = nhan,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(43, 53, 68)
            }, 0, dong);

            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(4, 16, 10, 15);
            input.Font = new Font("Segoe UI", 11);
            input.UseSystemPasswordChar = true;
            grid.Controls.Add(input, 1, dong);
        }

        private void LuuMatKhau()
        {
            if (txtMatKhauMoi.Text != txtXacNhan.Text)
            {
                MessageBox.Show("Xác nhận mật khẩu mới không khớp.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                bll.DoiMatKhauCaNhan(phien, txtMatKhauCu.Text, txtMatKhauMoi.Text);
                txtMatKhauCu.Clear();
                txtMatKhauMoi.Clear();
                txtXacNhan.Clear();
                MessageBox.Show("Đổi mật khẩu thành công. Hãy dùng mật khẩu mới khi đăng nhập lại.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Dữ liệu không hợp lệ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể thực hiện",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message, "Không có quyền",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("Không thể đổi mật khẩu. Vui lòng kiểm tra kết nối SQL Server.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
