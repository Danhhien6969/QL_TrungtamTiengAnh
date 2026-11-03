using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmLopHoc : Form
    {
        private readonly LopHocBLL bll = new();
        private readonly Color xanhDam = Color.FromArgb(23, 52, 83);
        private int? maLopDangChon;
        private bool dangNap;
        private bool dangCapNhatSiSo;
        private string tenGoiYCu = "";
        private List<DongLopHocTongHop> hienThiHienTai = new();

        private readonly TextBox txtMaLop = new();
        private readonly TextBox txtTenLop = new();
        private readonly ComboBox cboKhoaHoc = new();
        private readonly ComboBox cboGiaoVien = new();
        private readonly ComboBox cboPhong = new();
        private readonly DateTimePicker dtpBatDau = new();
        private readonly DateTimePicker dtpKetThuc = new();
        private readonly ComboBox cboTrangThai = new();
        private readonly TextBox txtGhiChu = new();
        private readonly CheckBox chkHocPhiRieng = new();
        private readonly NumericUpDown nudHocPhiRieng = new();
        private readonly TextBox txtTimKiem = new();
        private readonly ComboBox cboLocKhoa = new();
        private readonly ComboBox cboLocGV = new();
        private readonly ComboBox cboLocTrangThai = new();
        private readonly DataGridView dgvLopHoc = new();
        private readonly Label lblTong = new();

        private sealed class LuaChon
        {
            public int Ma { get; init; }
            public string Ten { get; init; } = "";
        }
        private sealed class LuaChonTrangThai
        {
            public string Ma { get; init; } = "";
            public string Ten { get; init; } = "";
        }
        private static readonly LuaChonTrangThai[] TrangThais =
        {
            new() { Ma = "SapMo", Ten = "Sắp mở" },
            new() { Ma = "DangHoc", Ten = "Đang học" },
            new() { Ma = "DaKetThuc", Ten = "Đã kết thúc" },
            new() { Ma = "DaHuy", Ten = "Đã hủy" }
        };

        public frmLopHoc()
        {
            InitializeComponent();
            TaoGiaoDien();
            Shown += (_, _) => XuLyAnToan(() =>
            {
                NapDanhMuc();
                NapBang();
            });
        }

        private void TaoGiaoDien()
        {
            Text = "Quản lý lớp học";
            ClientSize = new Size(1250, 790);
            MinimumSize = new Size(900, 620);
            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);
            Controls.Clear();

            TableLayoutPanel root = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(16),
                BackColor = BackColor
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 69));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 285));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            Panel dauTrang = new() { Dock = DockStyle.Fill };
            dauTrang.Controls.Add(new Label
            {
                Text = "QUẢN LÝ LỚP HỌC",
                AutoSize = true,
                Location = new Point(4, 0),
                ForeColor = xanhDam,
                Font = new Font("Segoe UI", 21, FontStyle.Bold)
            });
            dauTrang.Controls.Add(new Label
            {
                Text = "Chọn phòng chính tại đây; lịch học tự kế thừa phòng của lớp",
                AutoSize = true,
                Location = new Point(6, 43),
                ForeColor = Color.DimGray
            });
            root.Controls.Add(dauTrang, 0, 0);

            GroupBox nhap = new()
            {
                Text = "THÔNG TIN LỚP HỌC",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = xanhDam,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10)
            };
            var form = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 5,
                Padding = new Padding(6)
            };
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
            form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 5; ++i)
                form.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 5));

            txtMaLop.ReadOnly = true;
            txtMaLop.BackColor = Color.FromArgb(239, 242, 246);
            txtTenLop.MaxLength = 100;
            txtTenLop.PlaceholderText = "Ví dụ: Cambridge B1-01";
            txtGhiChu.Multiline = true;
            txtGhiChu.ScrollBars = ScrollBars.Vertical;
            txtGhiChu.MaxLength = 500;
            txtGhiChu.PlaceholderText = "Ghi chú thêm về lớp...";
            foreach (var cb in new[] { cboKhoaHoc, cboGiaoVien, cboPhong, cboTrangThai, cboLocKhoa, cboLocGV, cboLocTrangThai })
                cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTrangThai.DisplayMember = nameof(LuaChonTrangThai.Ten);
            cboTrangThai.ValueMember = nameof(LuaChonTrangThai.Ma);
            cboTrangThai.DataSource = TrangThais.ToList();

            dtpBatDau.Format = DateTimePickerFormat.Custom;
            dtpBatDau.CustomFormat = "dd/MM/yyyy";
            dtpKetThuc.Format = DateTimePickerFormat.Custom;
            dtpKetThuc.CustomFormat = "dd/MM/yyyy";
            dtpKetThuc.Enabled = false;
            dtpKetThuc.CalendarMonthBackground = Color.FromArgb(239, 242, 246);
            nudHocPhiRieng.Maximum = 1_000_000_000m;
            nudHocPhiRieng.ThousandsSeparator = true;
            nudHocPhiRieng.DecimalPlaces = 0;
            nudHocPhiRieng.Enabled = false;
            chkHocPhiRieng.Text = "Học phí riêng";
            chkHocPhiRieng.AutoSize = true;
            chkHocPhiRieng.CheckedChanged += (_, _) => nudHocPhiRieng.Enabled = chkHocPhiRieng.Checked;

            ThemTruong(form, "Mã lớp", txtMaLop, 0, false);
            ThemTruong(form, "Tên lớp (*)", txtTenLop, 0, true);
            ThemTruong(form, "Khóa học (*)", cboKhoaHoc, 1, false);
            ThemTruong(form, "Giáo viên (*)", cboGiaoVien, 1, true);
            ThemTruong(form, "Ngày bắt đầu", dtpBatDau, 2, false);
            ThemTruong(form, "Kết thúc (tự tính)", dtpKetThuc, 2, true);
            ThemTruong(form, "Phòng học (*)", cboPhong, 3, false);
            ThemTruong(form, "Trạng thái", cboTrangThai, 3, true);
            // Hàng cuối: ghi chú và tùy chọn học phí riêng.
            ThemTruong(form, "Ghi chú", txtGhiChu, 4, false);
            var pnlHocPhi = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            pnlHocPhi.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
            pnlHocPhi.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlHocPhi.Controls.Add(chkHocPhiRieng, 0, 0);
            pnlHocPhi.Controls.Add(nudHocPhiRieng, 1, 0);
            nudHocPhiRieng.Dock = DockStyle.Fill;
            nudHocPhiRieng.Margin = new Padding(4, 4, 5, 4);
            ThemTruong(form, "Học phí", pnlHocPhi, 4, true);
            nhap.Controls.Add(form);
            root.Controls.Add(nhap, 0, 1);

            cboKhoaHoc.SelectedValueChanged += (_, _) =>
            {
                if (!dangNap)
                    XuLyAnToan(() =>
                    {
                        if (maLopDangChon is null) GoiYTheoKhoa();
                        else TuTinhSiSo();
                    });
            };
            cboPhong.SelectedValueChanged += (_, _) =>
            {
                if (!dangNap && !dangCapNhatSiSo)
                    XuLyAnToan(TuTinhSiSo);
            };
            dtpBatDau.ValueChanged += (_, _) =>
            {
                if (!dangNap && maLopDangChon is null)
                    GoiYNgayKetThuc();
            };

            TableLayoutPanel congCu = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 6, 0, 1)
            };
            congCu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            congCu.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            congCu.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            root.Controls.Add(congCu, 0, 2);

            FlowLayoutPanel hangNut = TaoFlow();
            Button btnThem = Nut("Thêm", Color.FromArgb(23, 140, 93));
            Button btnSua = Nut("Sửa", Color.FromArgb(38, 112, 191));
            Button btnXoa = Nut("Xóa", Color.FromArgb(193, 63, 70));
            Button btnMoi = Nut("Làm mới", Color.FromArgb(95, 110, 125));
            Button btnXem = Nut("Xem học viên", xanhDam, 118);
            Button btnTrangThai = Nut("Cập nhật trạng thái", Color.FromArgb(92, 74, 148), 174);
            Button btnExcel = Nut("Xuất Excel", Color.FromArgb(22, 129, 80), 118);
            hangNut.Controls.AddRange(new Control[] { btnThem, btnSua, btnXoa, btnMoi, btnXem, btnTrangThai, btnExcel });
            congCu.Controls.Add(hangNut, 0, 0);

            FlowLayoutPanel hangLoc = TaoFlow();
            txtTimKiem.Width = 205;
            txtTimKiem.PlaceholderText = "Tìm lớp, giáo viên...";
            txtTimKiem.Margin = new Padding(2, 5, 7, 4);
            cboLocKhoa.Width = 205; cboLocGV.Width = 195; cboLocTrangThai.Width = 150;
            foreach (ComboBox c in new[] { cboLocKhoa, cboLocGV, cboLocTrangThai })
                c.Margin = new Padding(4, 4, 8, 4);
            Button btnLoc = Nut("Lọc / Tìm", xanhDam, 105);
            Button btnXoaLoc = Nut("Bỏ lọc", Color.FromArgb(95, 110, 125), 92);
            hangLoc.Controls.AddRange(new Control[] { txtTimKiem, cboLocKhoa, cboLocGV, cboLocTrangThai, btnLoc, btnXoaLoc });
            congCu.Controls.Add(hangLoc, 0, 1);

            btnThem.Click += (_, _) => XuLyAnToan(ThemLop);
            btnSua.Click += (_, _) => XuLyAnToan(SuaLop);
            btnXoa.Click += (_, _) => XuLyAnToan(XoaLop);
            btnMoi.Click += (_, _) => XuLyAnToan(LamMoi);
            btnXem.Click += (_, _) => XuLyAnToan(XemHocVien);
            btnTrangThai.Click += (_, _) => XuLyAnToan(CapNhatTrangThai);
            btnExcel.Click += (_, _) => XuLyAnToan(XuatExcel);
            btnLoc.Click += (_, _) => XuLyAnToan(ApDungLoc);
            btnXoaLoc.Click += (_, _) => XuLyAnToan(BoLoc);
            txtTimKiem.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    XuLyAnToan(ApDungLoc);
                }
            };

            Panel danhSach = new() { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(8) };
            Label lblDanhSach = new()
            {
                Text = "DANH SÁCH LỚP HỌC",
                Dock = DockStyle.Top,
                Height = 37,
                ForeColor = xanhDam,
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            lblTong.Dock = DockStyle.Bottom;
            lblTong.Height = 28;
            lblTong.TextAlign = ContentAlignment.MiddleRight;
            lblTong.Text = "Tổng số: 0 lớp";
            CauHinhBang();
            danhSach.Controls.Add(dgvLopHoc);
            danhSach.Controls.Add(lblTong);
            danhSach.Controls.Add(lblDanhSach);
            root.Controls.Add(danhSach, 0, 3);
        }

        private static FlowLayoutPanel TaoFlow() => new()
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 3, 0, 0)
        };

        private static void ThemTruong(TableLayoutPanel form, string nhan, Control input, int dong, bool benPhai)
        {
            int cot = benPhai ? 2 : 0;
            form.Controls.Add(new Label
            {
                Text = nhan,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(45, 55, 70),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f)
            }, cot, dong);
            input.Dock = DockStyle.Fill;
            input.Font = new Font("Segoe UI", 10);
            input.Margin = new Padding(4, 6, 8, 5);
            form.Controls.Add(input, cot + 1, dong);
        }

        private static Button Nut(string text, Color color, int width = 91)
        {
            var btn = new Button
            {
                Text = text,
                Width = width,
                Height = 36,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 1, 6, 1)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void CauHinhBang()
        {
            dgvLopHoc.Dock = DockStyle.Fill;
            dgvLopHoc.BackgroundColor = Color.White;
            dgvLopHoc.BorderStyle = BorderStyle.None;
            dgvLopHoc.AutoGenerateColumns = false;
            dgvLopHoc.ReadOnly = true;
            dgvLopHoc.MultiSelect = false;
            dgvLopHoc.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvLopHoc.AllowUserToAddRows = false;
            dgvLopHoc.AllowUserToDeleteRows = false;
            dgvLopHoc.AllowUserToResizeRows = false;
            dgvLopHoc.RowHeadersVisible = false;
            dgvLopHoc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLopHoc.ColumnHeadersHeight = 41;
            dgvLopHoc.RowTemplate.Height = 34;
            dgvLopHoc.EnableHeadersVisualStyles = false;
            dgvLopHoc.ColumnHeadersDefaultCellStyle.BackColor = xanhDam;
            dgvLopHoc.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvLopHoc.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvLopHoc.DefaultCellStyle.SelectionBackColor = Color.FromArgb(196, 225, 248);
            dgvLopHoc.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvLopHoc.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252);
            dgvLopHoc.Columns.Clear();
            Cot("STT", "STT", 42);
            Cot("MaHienThi", "Mã lớp", 70);
            Cot("TenLop", "Tên lớp", 155);
            Cot("TenKhoaHoc", "Chứng chỉ", 150);
            Cot("TenGiaoVien", "Giáo viên", 145);
            Cot("BatDau", "Bắt đầu", 88);
            Cot("KetThuc", "Kết thúc", 88);
            Cot("SiSo", "Sĩ số", 77);
            Cot("TenPhong", "Phòng", 95);
            Cot("TrangThaiText", "Trạng thái", 100);
            dgvLopHoc.SelectionChanged += ChonDong;
            dgvLopHoc.RowPrePaint += (_, e) =>
            {
                var r = dgvLopHoc.Rows[e.RowIndex];
                if (r.DataBoundItem is not DongLopHocTongHop d) return;
                if (d.TrangThai == "DaHuy")
                    r.DefaultCellStyle.BackColor = Color.FromArgb(255, 232, 231);
                else if (d.TrangThai == "DaKetThuc")
                    r.DefaultCellStyle.BackColor = Color.FromArgb(234, 235, 238);
                else if (d.SiSoToiDa - d.SiSoHienTai < 3)
                    r.DefaultCellStyle.BackColor = Color.FromArgb(255, 249, 210);
                else
                    r.DefaultCellStyle.BackColor = e.RowIndex % 2 == 0
                        ? Color.White : Color.FromArgb(247, 249, 252);
            };
            dgvLopHoc.DataError += (_, e) =>
            {
                // Không nuốt lỗi: hiển thị ngắn gọn cột gây lỗi nếu có.
                e.ThrowException = false;
                MessageBox.Show($"Lỗi hiển thị cột {e.ColumnIndex}: {e.Exception?.Message}",
                    "Lỗi bảng dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
        }

        private void Cot(string property, string title, float weight)
        {
            dgvLopHoc.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = property,
                DataPropertyName = property,
                HeaderText = title,
                FillWeight = weight,
                ReadOnly = true,
                MinimumWidth = 60
            });
        }

        private void NapDanhMuc(int? phongCu = null)
        {
            dangNap = true;
            try
            {
                var kh = bll.LayKhoaHocs();
                var gv = bll.LayGiaoViens();
                var phong = bll.LayPhongHocs()
                    .Where(p => p.TrangThai || p.MaPhong == phongCu)
                    .ToList();
                cboKhoaHoc.DisplayMember = nameof(KhoaHoc.TenKH);
                cboKhoaHoc.ValueMember = nameof(KhoaHoc.MaKH);
                cboKhoaHoc.DataSource = kh; cboKhoaHoc.SelectedIndex = -1;
                cboGiaoVien.DisplayMember = nameof(GiaoVien.HoTen);
                cboGiaoVien.ValueMember = nameof(GiaoVien.MaGV);
                cboGiaoVien.DataSource = gv; cboGiaoVien.SelectedIndex = -1;
                var phongList = new List<LuaChon> { new() { Ma = 0, Ten = "— Chưa chọn phòng —" } };
                phongList.AddRange(phong.Select(p => new LuaChon { Ma = p.MaPhong, Ten = p.TenPhong + (p.TrangThai ? $" ({p.SucChua} chỗ)" : " (ngừng sử dụng)") }));
                cboPhong.DisplayMember = nameof(LuaChon.Ten);
                cboPhong.ValueMember = nameof(LuaChon.Ma);
                cboPhong.DataSource = phongList; cboPhong.SelectedValue = 0;

                var danhMucKH = new List<LuaChon> { new() { Ma = 0, Ten = "Tất cả chứng chỉ" } };
                danhMucKH.AddRange(kh.Select(k => new LuaChon { Ma = k.MaKH, Ten = k.TenKH }));
                cboLocKhoa.DisplayMember = nameof(LuaChon.Ten);
                cboLocKhoa.ValueMember = nameof(LuaChon.Ma);
                cboLocKhoa.DataSource = danhMucKH;
                var danhMucGV = new List<LuaChon> { new() { Ma = 0, Ten = "Tất cả giáo viên" } };
                danhMucGV.AddRange(gv.Select(g => new LuaChon { Ma = g.MaGV, Ten = g.HoTen }));
                cboLocGV.DisplayMember = nameof(LuaChon.Ten);
                cboLocGV.ValueMember = nameof(LuaChon.Ma);
                cboLocGV.DataSource = danhMucGV;
                cboLocTrangThai.DisplayMember = nameof(LuaChonTrangThai.Ten);
                cboLocTrangThai.ValueMember = nameof(LuaChonTrangThai.Ma);
                cboLocTrangThai.DataSource = new[] { new LuaChonTrangThai { Ma = "", Ten = "Tất cả trạng thái" } }.Concat(TrangThais).ToList();
            }
            finally { dangNap = false; }
            XoaNhap();
        }

        private void NapBang() => ApDungLoc();

        private void ApDungLoc()
        {
            string s = txtTimKiem.Text.Trim();
            var rows = bll.LayBangTongHop().AsEnumerable();
            if (s.Length > 0)
                rows = rows.Where(r => r.MaHienThi.Contains(s, StringComparison.OrdinalIgnoreCase)
                    || r.TenLop.Contains(s, StringComparison.OrdinalIgnoreCase)
                    || r.TenKhoaHoc.Contains(s, StringComparison.OrdinalIgnoreCase)
                    || r.TenGiaoVien.Contains(s, StringComparison.OrdinalIgnoreCase)
                    || r.TenPhong.Contains(s, StringComparison.OrdinalIgnoreCase));
            if (cboLocKhoa.SelectedValue is int maKH && maKH > 0)
                rows = rows.Where(r => r.MaKH == maKH);
            if (cboLocGV.SelectedValue is int maGV && maGV > 0)
                rows = rows.Where(r => r.MaGV == maGV);
            if (cboLocTrangThai.SelectedValue is string trangThai && trangThai.Length > 0)
                rows = rows.Where(r => r.TrangThai == trangThai);
            hienThiHienTai = rows.ToList();
            // STT đánh lại từ 1 theo kết quả tìm kiếm/lọc hiện tại.
            for (int i = 0; i < hienThiHienTai.Count; i++)
                hienThiHienTai[i].STT = i + 1;
            dangNap = true;
            try
            {
                dgvLopHoc.DataSource = null;
                dgvLopHoc.DataSource = hienThiHienTai;
                dgvLopHoc.ClearSelection();
                lblTong.Text = $"Tổng số: {hienThiHienTai.Count} lớp học";
                XoaNhap();
            }
            finally { dangNap = false; }
        }

        private void BoLoc()
        {
            txtTimKiem.Clear(); cboLocKhoa.SelectedIndex = 0;
            cboLocGV.SelectedIndex = 0; cboLocTrangThai.SelectedIndex = 0;
            ApDungLoc();
        }

        private void GoiYTheoKhoa()
        {
            if (cboKhoaHoc.SelectedValue is not int maKH || maKH <= 0) return;
            var kh = cboKhoaHoc.SelectedItem as KhoaHoc;
            string goiY = bll.GoiYTenLop(maKH);
            if (string.IsNullOrWhiteSpace(txtTenLop.Text) || txtTenLop.Text == tenGoiYCu)
                txtTenLop.Text = goiY;
            tenGoiYCu = goiY;
            TuTinhSiSo();
            if (kh != null && kh.HocPhi >= 0)
                nudHocPhiRieng.Value = Math.Clamp(kh.HocPhi, nudHocPhiRieng.Minimum, nudHocPhiRieng.Maximum);
            GoiYNgayKetThuc();
        }

        // Hiển thị thông tin phòng qua ComboBox; sức chứa được DAL lấy khi lưu.
        private void TuTinhSiSo()
        {
            // Không còn ô nhập sĩ số. DAL sẽ dùng PhongHoc.SucChua.
        }

        private void GoiYNgayKetThuc()
        {
            if (maLopDangChon.HasValue) return;
            // Ngày này là DỰ KIẾN để tạo lớp trước khi có lịch cụ thể.
            // Khi tạo lịch đầu tiên, hệ thống sẽ cập nhật theo số buổi thực tế.
            var kh = cboKhoaHoc.SelectedItem as KhoaHoc;
            dtpKetThuc.Value = dtpBatDau.Value.Date.AddMonths(kh?.ThoiLuongThang ?? 3);
        }

        private LopHoc DocForm()
        {
            if (cboKhoaHoc.SelectedValue is not int maKH || maKH <= 0)
                throw new ArgumentException("Bạn chưa chọn khóa học.");
            if (cboGiaoVien.SelectedValue is not int maGV || maGV <= 0)
                throw new ArgumentException("Bạn chưa chọn giáo viên.");
            int? phong = cboPhong.SelectedValue is int id && id > 0 ? id : null;
            return new LopHoc
            {
                MaLop = maLopDangChon ?? 0,
                TenLop = txtTenLop.Text,
                MaKH = maKH,
                MaGV = maGV,
                MaPhongMacDinh = phong,
                NgayBatDau = dtpBatDau.Value.Date,
                NgayKetThuc = dtpKetThuc.Value.Date,
                TrangThai = cboTrangThai.SelectedValue?.ToString() ?? "SapMo",
                HocPhiRieng = chkHocPhiRieng.Checked ? nudHocPhiRieng.Value : null,
                GhiChu = txtGhiChu.Text
            };
        }

        private void ThemLop()
        {
            int ma = bll.Them(DocForm());
            MessageBox.Show($"Đã tạo lớp thành công (mã LH{ma:D2}).", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            NapBang();
        }
        private void SuaLop()
        {
            if (!maLopDangChon.HasValue)
                throw new ArgumentException("Chọn lớp cần sửa từ bảng bên dưới.");
            var thayDoi = DocForm();
            var cu = bll.LayTheoMa(maLopDangChon.Value);
            if (cu != null && cu.MaPhongMacDinh != thayDoi.MaPhongMacDinh)
            {
                string noiDung = "Bạn đang đổi phòng của lớp. Nếu lớp đã có lịch nhưng chưa có buổi học/điểm danh, " +
                    "hệ thống sẽ kiểm tra trùng phòng và chuyển đồng bộ các lịch đó sang phòng mới. " +
                    "Nếu đã có buổi học, hệ thống sẽ từ chối cập nhật hàng loạt để bảo vệ lịch sử. Tiếp tục?";
                if (MessageBox.Show(noiDung, "Xác nhận chuyển phòng", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;
            }
            if (!bll.Sua(thayDoi))
                throw new InvalidOperationException("Lớp không còn tồn tại.");
            MessageBox.Show("Đã cập nhật thông tin lớp. Lịch học mới sẽ tự lấy phòng chính vừa chọn.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            NapBang();
        }
        private void XoaLop()
        {
            if (!maLopDangChon.HasValue)
                throw new ArgumentException("Chọn lớp cần xóa.");
            if (MessageBox.Show("Chỉ xóa vĩnh viễn lớp CHƯA CÓ đăng ký, lịch hay buổi học. Tiếp tục?",
                "Xác nhận xóa", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes) return;
            if (bll.Xoa(maLopDangChon.Value))
            {
                MessageBox.Show("Đã xóa lớp chưa có dữ liệu liên quan.");
                NapBang();
            }
        }
        private void LamMoi()
        {
            NapDanhMuc();
            txtTimKiem.Clear();
            NapBang();
        }
        private void CapNhatTrangThai()
        {
            if (MessageBox.Show("Tự cập nhật trạng thái theo ngày?\nLớp Sắp mở chuyển sang Đang học theo ngày khai giảng. Lớp đã hủy không bị thay đổi.",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int count = bll.CapNhatTrangThaiTheoNgay();
            MessageBox.Show($"Đã cập nhật {count} lớp.");
            NapBang();
        }

        private void ChonDong(object? sender, EventArgs e)
        {
            if (dangNap || dgvLopHoc.SelectedRows.Count == 0 ||
                dgvLopHoc.SelectedRows[0].DataBoundItem is not DongLopHocTongHop d) return;
            XuLyAnToan(() =>
            {
                LopHoc? lop = bll.LayTheoMa(d.MaLop);
                if (lop == null) return;
                dangNap = true;
                try
                {
                    maLopDangChon = lop.MaLop;
                    txtMaLop.Text = $"LH{lop.MaLop:D2}";
                    txtTenLop.Text = lop.TenLop;
                    cboKhoaHoc.SelectedValue = lop.MaKH;
                    cboGiaoVien.SelectedValue = lop.MaGV;
                    // Nếu phòng cũ ngừng hoạt động, vẫn thêm vào lựa chọn để có thể xem/sửa.
                    if (lop.MaPhongMacDinh.HasValue &&
                        !cboPhong.Items.Cast<LuaChon>().Any(x => x.Ma == lop.MaPhongMacDinh.Value))
                    {
                        var phong = bll.LayPhongHocs().FirstOrDefault(x => x.MaPhong == lop.MaPhongMacDinh.Value);
                        if (phong != null && cboPhong.DataSource is List<LuaChon> list)
                        {
                            cboPhong.DataSource = null;
                            list.Add(new LuaChon { Ma = phong.MaPhong, Ten = phong.TenPhong + " (ngừng sử dụng)" });
                            cboPhong.DisplayMember = nameof(LuaChon.Ten);
                            cboPhong.ValueMember = nameof(LuaChon.Ma);
                            cboPhong.DataSource = list;
                        }
                    }
                    cboPhong.SelectedValue = lop.MaPhongMacDinh ?? 0;
                    dtpBatDau.Value = lop.NgayBatDau;
                    dtpKetThuc.Value = lop.NgayKetThuc;
                    cboTrangThai.SelectedValue = lop.TrangThai;
                    chkHocPhiRieng.Checked = lop.HocPhiRieng.HasValue;
                    nudHocPhiRieng.Value = Math.Clamp(lop.HocPhiRieng ?? lop.KhoaHoc.HocPhi,
                        nudHocPhiRieng.Minimum, nudHocPhiRieng.Maximum);
                    txtGhiChu.Text = lop.GhiChu ?? "";
                }
                finally { dangNap = false; }
            });
        }

        private void XoaNhap()
        {
            maLopDangChon = null;
            txtMaLop.Clear(); txtTenLop.Clear(); tenGoiYCu = "";
            cboKhoaHoc.SelectedIndex = -1; cboGiaoVien.SelectedIndex = -1;
            cboPhong.SelectedValue = 0;
            dtpBatDau.Value = DateTime.Today;
            dtpKetThuc.Value = DateTime.Today.AddMonths(3);
            cboTrangThai.SelectedValue = "SapMo";
            txtGhiChu.Clear(); chkHocPhiRieng.Checked = false;
            nudHocPhiRieng.Value = 0;
        }

        private void XemHocVien()
        {
            if (!maLopDangChon.HasValue)
                throw new ArgumentException("Chọn lớp cần xem danh sách học viên.");
            var ds = bll.LayHocVienTrongLop(maLopDangChon.Value);
            using Form form = new()
            {
                Text = "Học viên của lớp: " + txtTenLop.Text,
                StartPosition = FormStartPosition.CenterParent,
                ClientSize = new Size(750, 470),
                MinimumSize = new Size(600, 350)
            };
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoGenerateColumns = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                DataSource = ds
            };
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Mã HV", DataPropertyName = "MaHienThi" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Họ và tên", DataPropertyName = "HoTen", FillWeight = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Ngày đăng ký", DataPropertyName = "NgayDangKyText" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Trạng thái", DataPropertyName = "TrangThai" });
            var lbl = new Label
            {
                Dock = DockStyle.Top,
                Height = 45,
                Text = $"Tổng số học viên chưa hủy đăng ký: {ds.Count}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0)
            };
            form.Controls.Add(grid);
            form.Controls.Add(lbl);
            form.ShowDialog(this);
        }

        private void XuatExcel()
        {
            if (hienThiHienTai.Count == 0)
                throw new InvalidOperationException("Danh sách hiện tại không có lớp để xuất.");
            using SaveFileDialog dialog = new()
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = "DanhSachLopHoc.xlsx",
                Title = "Lưu danh sách lớp học"
            };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("LopHoc");
            string[] tieuDe = { "STT", "Mã lớp", "Tên lớp", "Chứng chỉ", "Giáo viên", "Bắt đầu", "Kết thúc", "Sĩ số", "Phòng", "Trạng thái", "Học phí", "Ghi chú" };
            for (int j = 0; j < tieuDe.Length; ++j) ws.Cell(1, j + 1).Value = tieuDe[j];
            for (int i = 0; i < hienThiHienTai.Count; ++i)
            {
                var r = hienThiHienTai[i]; int row = i + 2;
                ws.Cell(row, 1).Value = i + 1;
                ws.Cell(row, 2).Value = r.MaHienThi;
                ws.Cell(row, 3).Value = r.TenLop;
                ws.Cell(row, 4).Value = r.TenKhoaHoc;
                ws.Cell(row, 5).Value = r.TenGiaoVien;
                ws.Cell(row, 6).Value = r.NgayBatDau;
                ws.Cell(row, 7).Value = r.NgayKetThuc;
                ws.Cell(row, 8).Value = r.SiSo;
                ws.Cell(row, 9).Value = r.TenPhong;
                ws.Cell(row, 10).Value = r.TrangThaiText;
                ws.Cell(row, 11).Value = r.HocPhiApDung;
                ws.Cell(row, 12).Value = r.GhiChu;
            }
            var header = ws.Range(1, 1, 1, tieuDe.Length);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#173453");
            header.Style.Font.FontColor = XLColor.White;
            ws.Columns().AdjustToContents();
            ws.Column(6).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Column(7).Style.DateFormat.Format = "dd/MM/yyyy";
            ws.Column(11).Style.NumberFormat.Format = "#,##0";
            ws.SheetView.FreezeRows(1);
            wb.SaveAs(dialog.FileName);
            MessageBox.Show("Đã xuất Excel thành công.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void XuLyAnToan(Action action)
        {
            try { action(); }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Dữ liệu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi khi xử lý dữ liệu:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
