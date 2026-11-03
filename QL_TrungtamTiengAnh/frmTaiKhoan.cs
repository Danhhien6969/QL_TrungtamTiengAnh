using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmTaiKhoan : Form
    {
        private readonly PhienDangNhap phien;
        private readonly QuanLyTaiKhoanBLL bll = new();
        private int? maTKDangChon;
        private bool dangNap;
        private bool taiKhoanDangHoatDong;

        private readonly TextBox txtMaTK = new();
        private readonly TextBox txtTenDangNhap = new();
        private readonly ComboBox cboVaiTro = new();
        private readonly ComboBox cboGiaoVien = new();
        private readonly Label lblGiaoVien = new();
        private readonly TextBox txtMatKhauMoi = new();
        private readonly TextBox txtXacNhan = new();
        private readonly Label lblTrangThai = new();
        private readonly TextBox txtTimKiem = new();
        private readonly Label lblSoLuong = new();
        private readonly DataGridView dgv = new();
        private readonly Button btnKhoaMo = new();

        public frmTaiKhoan(PhienDangNhap phienDangNhap)
        {
            InitializeComponent();
            phien = phienDangNhap;
            TaoGiaoDien();
            Shown += (_, _) => XuLyAnToan(() =>
            {
                NapVaiTro();
                NapGiaoVien();
                NapDanhSach();
            });
        }

        private static Button TaoNut(string text, Color color, int width = 115)
        {
            var btn = new Button
            {
                Text = text,
                Width = width,
                Height = 40,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 6)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static void ThemTruong(
            TableLayoutPanel grid, string nhan, Control input, int hang, int nua)
        {
            int cot = nua * 2;
            grid.Controls.Add(new Label
            {
                Text = nhan,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(43, 53, 68)
            }, cot, hang);

            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(3, 8, 12, 8);
            input.Font = new Font("Segoe UI", 11);
            grid.Controls.Add(input, cot + 1, hang);
        }

        private void TaoGiaoDien()
        {
            Text = "Quản lý tài khoản và phân quyền";
            ClientSize = new Size(1180, 740);
            MinimumSize = new Size(950, 640);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);
            Controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(245, 248, 252)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 77));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 285));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 102));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            var header = new Panel { Dock = DockStyle.Fill };
            header.Controls.Add(new Label
            {
                Text = "QUẢN LÝ TÀI KHOẢN",
                Location = new Point(5, 2),
                AutoSize = true,
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });
            header.Controls.Add(new Label
            {
                Text = "Tạo tài khoản, phân quyền, khóa/mở và đặt lại mật khẩu",
                Location = new Point(7, 48),
                AutoSize = true,
                ForeColor = Color.DimGray
            });
            layout.Controls.Add(header, 0, 0);

            var grp = new GroupBox
            {
                Text = "THÔNG TIN TÀI KHOẢN",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(23, 52, 83),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(12)
            };
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(8)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 4; i++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25));

            txtMaTK.ReadOnly = true;
            txtMaTK.BackColor = Color.FromArgb(240, 243, 247);
            cboVaiTro.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGiaoVien.DropDownStyle = ComboBoxStyle.DropDownList;
            cboVaiTro.SelectedIndexChanged += (_, _) => CapNhatChonGiaoVien();
            txtMatKhauMoi.UseSystemPasswordChar = true;
            txtXacNhan.UseSystemPasswordChar = true;
            lblTrangThai.AutoSize = false;
            lblTrangThai.Text = "Tài khoản mới";
            lblTrangThai.TextAlign = ContentAlignment.MiddleLeft;
            lblTrangThai.ForeColor = Color.FromArgb(30, 125, 90);

            ThemTruong(grid, "Mã tài khoản", txtMaTK, 0, 0);
            ThemTruong(grid, "Tên đăng nhập (*)", txtTenDangNhap, 0, 1);
            ThemTruong(grid, "Vai trò (*)", cboVaiTro, 1, 0);
            ThemTruong(grid, "Trạng thái", lblTrangThai, 1, 1);
            ThemTruong(grid, "Mật khẩu mới (*)", txtMatKhauMoi, 2, 0);
            ThemTruong(grid, "Xác nhận mật khẩu", txtXacNhan, 2, 1);
            ThemTruong(grid, "Giáo viên liên kết (*)", cboGiaoVien, 3, 0);

            grp.Controls.Add(grid);
            layout.Controls.Add(grp, 0, 1);

            var tools = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(0, 11, 0, 0)
            };
            var btnThem = TaoNut("Tạo tài khoản", Color.FromArgb(30, 140, 95), 143);
            var btnVaiTro = TaoNut("Đổi vai trò", Color.FromArgb(40, 110, 190), 124);
            btnKhoaMo.Text = "Khóa tài khoản";
            btnKhoaMo.Width = 135;
            btnKhoaMo.Height = 40;
            btnKhoaMo.BackColor = Color.FromArgb(195, 65, 70);
            btnKhoaMo.ForeColor = Color.White;
            btnKhoaMo.FlatStyle = FlatStyle.Flat;
            btnKhoaMo.FlatAppearance.BorderSize = 0;
            btnKhoaMo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnKhoaMo.Cursor = Cursors.Hand;
            btnKhoaMo.Margin = new Padding(0, 0, 8, 6);
            var btnDatLai = TaoNut("Đặt lại mật khẩu", Color.FromArgb(95, 105, 165), 170);
            var btnLamMoi = TaoNut("Làm mới", Color.FromArgb(95, 110, 125), 112);
            var btnTimKiem = TaoNut("Tìm kiếm", Color.FromArgb(23, 52, 83), 110);

            txtTimKiem.Width = 180;
            txtTimKiem.Font = new Font("Segoe UI", 11);
            txtTimKiem.PlaceholderText = "Tài khoản, vai trò...";
            txtTimKiem.Margin = new Padding(7, 5, 8, 5);

            tools.Controls.AddRange(new Control[]
            {
                btnThem, btnVaiTro, btnKhoaMo, btnDatLai, btnLamMoi,
                txtTimKiem, btnTimKiem
            });
            layout.Controls.Add(tools, 0, 2);

            btnThem.Click += (_, _) => XuLyAnToan(TaoTaiKhoan);
            btnVaiTro.Click += (_, _) => XuLyAnToan(DoiVaiTro);
            btnKhoaMo.Click += (_, _) => XuLyAnToan(KhoaMoTaiKhoan);
            btnDatLai.Click += (_, _) => XuLyAnToan(DatLaiMatKhau);
            btnLamMoi.Click += (_, _) => XuLyAnToan(LamMoi);
            btnTimKiem.Click += (_, _) => XuLyAnToan(() => NapDanhSach(txtTimKiem.Text));
            txtTimKiem.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    XuLyAnToan(() => NapDanhSach(txtTimKiem.Text));
                }
            };

            var panelBang = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            CauHinhBang();
            panelBang.Controls.Add(dgv);
            lblSoLuong.Dock = DockStyle.Bottom;
            lblSoLuong.Height = 30;
            lblSoLuong.TextAlign = ContentAlignment.MiddleRight;
            panelBang.Controls.Add(lblSoLuong);
            panelBang.Controls.Add(new Label
            {
                Text = "DANH SÁCH TÀI KHOẢN",
                Dock = DockStyle.Top,
                Height = 38,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });
            layout.Controls.Add(panelBang, 0, 3);
            XoaLuaChon();
        }

        private void CauHinhBang()
        {
            dgv.Dock = DockStyle.Fill;
            dgv.AutoGenerateColumns = false;
            dgv.ReadOnly = true;
            dgv.MultiSelect = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 42;
            dgv.RowTemplate.Height = 36;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 52, 83);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 230, 250);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);

            ThemCot("MaTK", "Mã TK", 65);
            ThemCot("TenDangNhap", "Tên đăng nhập", 170);
            ThemCot("TenVaiTro", "Vai trò", 120);
            ThemCot("TenGiaoVien", "Giáo viên liên kết", 170);
            ThemCot("NgayTao", "Ngày tạo (UTC)", 160, "dd/MM/yyyy HH:mm");
            var cotTrangThai = new DataGridViewCheckBoxColumn
            {
                Name = "TrangThai",
                HeaderText = "Hoạt động",
                DataPropertyName = "TrangThai",
                FillWeight = 95,
                ReadOnly = true
            };
            dgv.Columns.Add(cotTrangThai);
            dgv.SelectionChanged += Dgv_SelectionChanged;
        }

        private void ThemCot(string ten, string tieuDe, float fill, string? format = null)
        {
            var cot = new DataGridViewTextBoxColumn
            {
                Name = ten,
                HeaderText = tieuDe,
                DataPropertyName = ten,
                FillWeight = fill,
                ReadOnly = true
            };
            if (format != null) cot.DefaultCellStyle.Format = format;
            dgv.Columns.Add(cot);
        }

        private void NapVaiTro()
        {
            dangNap = true;
            try
            {
                cboVaiTro.DisplayMember = nameof(LuaChonVaiTro.TenVaiTro);
                cboVaiTro.ValueMember = nameof(LuaChonVaiTro.MaVT);
                cboVaiTro.DataSource = bll.LayVaiTro(phien);
                cboVaiTro.SelectedIndex = -1;
            }
            finally { dangNap = false; }
        }

        private bool LaVaiTroGiaoVien()
        {
            return cboVaiTro.SelectedItem is LuaChonVaiTro vt &&
                string.Equals(vt.TenVaiTro, "GiaoVien", StringComparison.OrdinalIgnoreCase);
        }

        private void CapNhatChonGiaoVien()
        {
            cboGiaoVien.Enabled = LaVaiTroGiaoVien();
            if (!cboGiaoVien.Enabled) cboGiaoVien.SelectedIndex = -1;
        }

        private void NapGiaoVien(int? maGV = null)
        {
            dangNap = true;
            try
            {
                cboGiaoVien.DisplayMember = nameof(LuaChonGiaoVienTK.HienThi);
                cboGiaoVien.ValueMember = nameof(LuaChonGiaoVienTK.MaGV);
                cboGiaoVien.DataSource = bll.LayGiaoVienCoTheGan(phien, maGV);
                cboGiaoVien.SelectedValue = maGV ?? -1;
            }
            finally { dangNap = false; }
            CapNhatChonGiaoVien();
        }

        private void NapDanhSach(string tuKhoa = "")
        {
            var danhSach = bll.LayDanhSach(phien, tuKhoa);
            dangNap = true;
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = danhSach;
                dgv.ClearSelection();
            }
            finally { dangNap = false; }
            lblSoLuong.Text = $"Tổng số: {danhSach.Count} tài khoản";
            XoaLuaChon();
        }

        private void XoaLuaChon()
        {
            maTKDangChon = null;
            txtMaTK.Clear();
            txtTenDangNhap.Clear();
            txtTenDangNhap.ReadOnly = false;
            cboVaiTro.SelectedIndex = -1;
            NapGiaoVien();
            txtMatKhauMoi.Clear();
            txtXacNhan.Clear();
            lblTrangThai.Text = "Tài khoản mới";
            lblTrangThai.ForeColor = Color.FromArgb(30, 125, 90);
            btnKhoaMo.Text = "Khóa tài khoản";
            btnKhoaMo.BackColor = Color.FromArgb(195, 65, 70);
            dgv.ClearSelection();
        }

        private void Dgv_SelectionChanged(object? sender, EventArgs e)
        {
            if (dangNap || dgv.SelectedRows.Count == 0) return;
            if (dgv.SelectedRows[0].DataBoundItem is not TaiKhoanHienThi tk) return;

            maTKDangChon = tk.MaTK;
            taiKhoanDangHoatDong = tk.TrangThai;
            txtMaTK.Text = tk.MaTK.ToString();
            txtTenDangNhap.Text = tk.TenDangNhap;
            txtTenDangNhap.ReadOnly = true;
            cboVaiTro.SelectedValue = tk.MaVT;
            NapGiaoVien(tk.MaGV);
            txtMatKhauMoi.Clear();
            txtXacNhan.Clear();
            lblTrangThai.Text = tk.TrangThai ? "Đang hoạt động" : "Đã khóa";
            lblTrangThai.ForeColor = tk.TrangThai ?
                Color.FromArgb(30, 125, 90) : Color.FromArgb(190, 65, 70);
            btnKhoaMo.Text = tk.TrangThai ? "Khóa tài khoản" : "Mở tài khoản";
            btnKhoaMo.BackColor = tk.TrangThai ?
                Color.FromArgb(195, 65, 70) : Color.FromArgb(30, 140, 95);
        }

        private int LayVaiTroDaChon()
        {
            if (cboVaiTro.SelectedValue is not int maVT)
                throw new ArgumentException("Vui lòng chọn vai trò.");
            return maVT;
        }

        private int LayMaTKDaChon()
        {
            return maTKDangChon ??
                throw new ArgumentException("Vui lòng chọn tài khoản trong danh sách.");
        }

        private void KiemTraXacNhan()
        {
            if (txtMatKhauMoi.Text != txtXacNhan.Text)
                throw new ArgumentException("Xác nhận mật khẩu không khớp.");
        }

        private void TaoTaiKhoan()
        {
            if (maTKDangChon.HasValue)
                throw new InvalidOperationException("Nhấn Làm mới trước khi tạo tài khoản mới.");
            KiemTraXacNhan();
            int maMoi = bll.TaoTaiKhoan(
                phien, txtTenDangNhap.Text, txtMatKhauMoi.Text, LayVaiTroDaChon(),
                cboGiaoVien.Enabled ? cboGiaoVien.SelectedValue as int? : null);
            MessageBox.Show($"Đã tạo tài khoản, mã: {maMoi}.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            LamMoi();
        }

        private void DoiVaiTro()
        {
            int maTK = LayMaTKDaChon();
            if (MessageBox.Show("Xác nhận đổi vai trò tài khoản đã chọn?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            bll.DoiVaiTro(phien, maTK, LayVaiTroDaChon(),
                cboGiaoVien.Enabled ? cboGiaoVien.SelectedValue as int? : null);
            MessageBox.Show("Đã cập nhật vai trò.", "Thành công");
            LamMoi();
        }

        private void KhoaMoTaiKhoan()
        {
            int maTK = LayMaTKDaChon();
            bool trangThaiMoi = !taiKhoanDangHoatDong;
            string hanhDong = trangThaiMoi ? "mở" : "khóa";
            if (MessageBox.Show($"Bạn chắc chắn muốn {hanhDong} tài khoản này?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            bll.DatTrangThai(phien, maTK, trangThaiMoi);
            MessageBox.Show($"Đã {hanhDong} tài khoản.", "Thành công");
            LamMoi();
        }

        private void DatLaiMatKhau()
        {
            int maTK = LayMaTKDaChon();
            KiemTraXacNhan();
            if (MessageBox.Show("Đặt lại mật khẩu cho tài khoản đã chọn?", "Xác nhận",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            bll.DatLaiMatKhau(phien, maTK, txtMatKhauMoi.Text);
            MessageBox.Show("Đã đặt lại mật khẩu. Hãy thông báo mật khẩu mới riêng cho người dùng.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LamMoi();
        }

        private void LamMoi()
        {
            txtTimKiem.Clear();
            NapVaiTro();
            NapGiaoVien();
            NapDanhSach();
        }

        private static void XuLyAnToan(Action thaoTac)
        {
            try { thaoTac(); }
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
                MessageBox.Show("Có lỗi khi xử lý tài khoản. Vui lòng kiểm tra kết nối SQL Server.",
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
