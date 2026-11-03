using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmGiaoVien : Form
    {
        private readonly GiaoVienBLL giaoVienBLL = new();
        private int? maGVDangChon;
        private bool dangNapDanhSach;
        private byte[]? anhChungChiDangChon;
        private const int GioiHanAnh = 5 * 1024 * 1024;

        private readonly TextBox txtMaGV = new();
        private readonly TextBox txtHoTen = new();
        private readonly DateTimePicker dtpNgaySinh = new();
        private readonly ComboBox cboGioiTinh = new();
        private readonly TextBox txtSoDienThoai = new();
        private readonly TextBox txtEmail = new();
        private readonly TextBox txtChuyenMon = new();
        private readonly DateTimePicker dtpNgayVaoLam = new();
        private readonly CheckBox chkTrangThai = new();
        private readonly TextBox txtDiaChi = new();
        private readonly TextBox txtGhiChu = new();
        private readonly TextBox txtTrinhDoHocVan = new();
        private readonly ComboBox cboXepLoai = new();
        private readonly PictureBox picChungChi = new();

        private readonly TextBox txtTimKiem = new();
        private readonly DataGridView dgvGiaoVien = new();
        private readonly Label lblSoLuong = new();

        public frmGiaoVien()
        {
            InitializeComponent();
            TaoGiaoDien();
            Shown += (_, _) => XuLyAnToan(() => NapDanhSach());
            FormClosed += (_, _) => picChungChi.Image?.Dispose();
        }

        // 1. GIAO DIỆN
        private void TaoGiaoDien()
        {
            Text = "Quản lý giáo viên";
            ClientSize = new Size(1260, 780);
            MinimumSize = new Size(1030, 690);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);
            Controls.Clear();

            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                BackColor = BackColor,
                ColumnCount = 1,
                RowCount = 4
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 390));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            Panel pnlTieuDe = new() { Dock = DockStyle.Fill };
            pnlTieuDe.Controls.Add(new Label
            {
                Text = "QUẢN LÝ GIÁO VIÊN",
                Location = new Point(5, 0),
                AutoSize = true,
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });
            pnlTieuDe.Controls.Add(new Label
            {
                Text = "Hồ sơ giáo viên, trình độ học vấn và ảnh chứng chỉ",
                Location = new Point(7, 45),
                AutoSize = true,
                ForeColor = Color.DimGray
            });
            layout.Controls.Add(pnlTieuDe, 0, 0);

            // Khung trái: thông tin cơ bản | khung phải: bằng cấp / chứng chỉ
            TableLayoutPanel haiKhung = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            haiKhung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));
            haiKhung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
            haiKhung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.Controls.Add(haiKhung, 0, 1);

            GroupBox grpThongTin = new()
            {
                Text = "THÔNG TIN GIÁO VIÊN",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(23, 52, 83),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 6, 0)
            };
            haiKhung.Controls.Add(grpThongTin, 0, 0);

            TableLayoutPanel thongTinGrid = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 5,
                Padding = new Padding(4, 6, 4, 4)
            };
            thongTinGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            thongTinGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            thongTinGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            thongTinGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 5; i++)
                thongTinGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            txtMaGV.ReadOnly = true;
            txtMaGV.BackColor = Color.FromArgb(240, 243, 247);
            dtpNgaySinh.Format = DateTimePickerFormat.Short;
            dtpNgaySinh.ShowCheckBox = true;
            dtpNgaySinh.Checked = false;
            cboGioiTinh.DropDownStyle = ComboBoxStyle.DropDownList;
            cboGioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            dtpNgayVaoLam.Format = DateTimePickerFormat.Short;
            dtpNgayVaoLam.Value = DateTime.Today;
            chkTrangThai.Text = "Đang làm việc";
            chkTrangThai.Checked = true;
            chkTrangThai.AutoSize = true;

            ThemTruong(thongTinGrid, "Mã giáo viên", txtMaGV, 0, 0);
            ThemTruong(thongTinGrid, "Họ và tên (*)", txtHoTen, 0, 1);
            ThemTruong(thongTinGrid, "Ngày sinh", dtpNgaySinh, 1, 0);
            ThemTruong(thongTinGrid, "Giới tính", cboGioiTinh, 1, 1);
            ThemTruong(thongTinGrid, "Số điện thoại", txtSoDienThoai, 2, 0);
            ThemTruong(thongTinGrid, "Email", txtEmail, 2, 1);
            ThemTruong(thongTinGrid, "Chuyên môn", txtChuyenMon, 3, 0);
            ThemTruong(thongTinGrid, "Ngày vào làm", dtpNgayVaoLam, 3, 1);
            ThemTruong(thongTinGrid, "Địa chỉ", txtDiaChi, 4, 0);
            ThemTruong(thongTinGrid, "Trạng thái", chkTrangThai, 4, 1);
            grpThongTin.Controls.Add(thongTinGrid);

            GroupBox grpBangCap = new()
            {
                Text = "HỌC VẤN – CHỨNG CHỈ",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(23, 52, 83),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10),
                Margin = new Padding(6, 0, 0, 0)
            };
            haiKhung.Controls.Add(grpBangCap, 1, 0);

            TableLayoutPanel bangCapGrid = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(4, 6, 4, 4)
            };
            bangCapGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            bangCapGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            bangCapGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            bangCapGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            bangCapGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
            bangCapGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            cboXepLoai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboXepLoai.Items.AddRange(new object[]
            {
                "Yếu", "Trung bình", "Khá", "Tốt", "Xuất sắc"
            });
            txtGhiChu.Multiline = true;
            txtGhiChu.ScrollBars = ScrollBars.Vertical;

            ThemTruongBangCap(bangCapGrid, "Trình độ học vấn", txtTrinhDoHocVan, 0);
            ThemTruongBangCap(bangCapGrid, "Loại tốt nghiệp", cboXepLoai, 1);
            ThemTruongBangCap(bangCapGrid, "Ghi chú", txtGhiChu, 2);

            Label lblAnh = new()
            {
                Text = "Ảnh chứng chỉ",
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(45, 55, 70),
                Font = new Font("Segoe UI", 10)
            };
            bangCapGrid.Controls.Add(lblAnh, 0, 3);

            FlowLayoutPanel pnlAnh = new()
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(3, 3, 3, 3)
            };
            picChungChi.Size = new Size(160, 135);
            picChungChi.SizeMode = PictureBoxSizeMode.Zoom;
            picChungChi.BorderStyle = BorderStyle.FixedSingle;
            picChungChi.BackColor = Color.FromArgb(246, 248, 251);
            picChungChi.Cursor = Cursors.Hand;
            picChungChi.Click += (_, _) => XemAnhLon();
            pnlAnh.Controls.Add(picChungChi);

            FlowLayoutPanel pnlNutAnh = new()
            {
                Width = 133,
                Height = 135,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Margin = new Padding(5, 0, 0, 0)
            };
            Button btnTaiAnh = TaoNut("Tải ảnh", Color.FromArgb(40, 110, 190), 116, 35);
            Button btnBoAnh = TaoNut("Bỏ ảnh", Color.FromArgb(95, 110, 125), 116, 35);
            btnTaiAnh.Click += (_, _) => XuLyAnToan(TaiAnh);
            btnBoAnh.Click += (_, _) => BoAnh();
            pnlNutAnh.Controls.Add(btnTaiAnh);
            pnlNutAnh.Controls.Add(btnBoAnh);
            pnlNutAnh.Controls.Add(new Label
            {
                Text = "JPG / PNG ≤ 5 MB",
                Width = 125,
                Height = 25,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.DimGray
            });
            pnlAnh.Controls.Add(pnlNutAnh);
            bangCapGrid.Controls.Add(pnlAnh, 1, 3);
            grpBangCap.Controls.Add(bangCapGrid);

            // Thanh thao tác
            FlowLayoutPanel pnlCongCu = new()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 11, 0, 0)
            };
            Button btnThem = TaoNut("Thêm", Color.FromArgb(30, 140, 95));
            Button btnSua = TaoNut("Sửa", Color.FromArgb(40, 110, 190));
            Button btnXoa = TaoNut("Xóa", Color.FromArgb(195, 65, 70));
            Button btnLamMoi = TaoNut("Làm mới", Color.FromArgb(95, 110, 125));
            Button btnTimKiem = TaoNut("Tìm kiếm", Color.FromArgb(23, 52, 83));

            txtTimKiem.Width = 245;
            txtTimKiem.Font = new Font("Segoe UI", 11);
            txtTimKiem.Margin = new Padding(12, 5, 6, 5);
            txtTimKiem.PlaceholderText = "Tên, SĐT, chuyên môn, học vấn...";
            pnlCongCu.Controls.AddRange(new Control[]
            {
                btnThem, btnSua, btnXoa, btnLamMoi, txtTimKiem, btnTimKiem
            });
            layout.Controls.Add(pnlCongCu, 0, 2);

            btnThem.Click += (_, _) => XuLyAnToan(ThemGiaoVien);
            btnSua.Click += (_, _) => XuLyAnToan(SuaGiaoVien);
            btnXoa.Click += (_, _) => XuLyAnToan(XoaGiaoVien);
            btnLamMoi.Click += (_, _) => XuLyAnToan(LamMoi);
            btnTimKiem.Click += (_, _) => XuLyAnToan(TimKiem);
            txtTimKiem.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    XuLyAnToan(TimKiem);
                }
            };

            Panel pnlDanhSach = new()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            Label lblDanhSach = new()
            {
                Text = "DANH SÁCH GIÁO VIÊN",
                Dock = DockStyle.Top,
                Height = 38,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            };
            lblSoLuong.Text = "Tổng số: 0 giáo viên";
            lblSoLuong.Dock = DockStyle.Bottom;
            lblSoLuong.Height = 32;
            lblSoLuong.TextAlign = ContentAlignment.MiddleRight;
            CauHinhDataGridView();
            pnlDanhSach.Controls.Add(dgvGiaoVien);
            pnlDanhSach.Controls.Add(lblSoLuong);
            pnlDanhSach.Controls.Add(lblDanhSach);
            layout.Controls.Add(pnlDanhSach, 0, 3);
        }

        private static void ThemTruong(TableLayoutPanel grid, string nhan,
            Control input, int dong, int viTri)
        {
            int cotNhan = viTri * 2;
            grid.Controls.Add(new Label
            {
                Text = nhan,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.2f),
                ForeColor = Color.FromArgb(45, 55, 70)
            }, cotNhan, dong);
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(3, 8, 9, 7);
            input.Font = new Font("Segoe UI", 10);
            grid.Controls.Add(input, cotNhan + 1, dong);
        }

        private static void ThemTruongBangCap(TableLayoutPanel grid,
            string nhan, Control input, int dong)
        {
            grid.Controls.Add(new Label
            {
                Text = nhan,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(45, 55, 70)
            }, 0, dong);
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(3, 5, 4, 5);
            input.Font = new Font("Segoe UI", 10);
            grid.Controls.Add(input, 1, dong);
        }

        private static Button TaoNut(string noiDung, Color mau,
            int rong = 100, int cao = 39)
        {
            Button btn = new()
            {
                Text = noiDung,
                Width = rong,
                Height = cao,
                BackColor = mau,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 7, 5)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        // 2. BẢNG DANH SÁCH - chỉ hiển thị thông tin cần xem nhanh.
        // Mọi cột đều là chuỗi; không gán int vào ô định dạng chuỗi
        // (nguyên nhân gây DataGridView FormatException ở bản trước).
        public sealed class DongGiaoVienHienThi
        {
            public int MaGV { get; init; }
            public string STT { get; init; } = "";
            public string MaHienThi { get; init; } = "";
            public string HoTen { get; init; } = "";
            public string GioiTinh { get; init; } = "";
            public string SoDienThoai { get; init; } = "";
            public string ChuyenMon { get; init; } = "";
            public string TrinhDoHocVan { get; init; } = "";
            public string XepLoaiTotNghiep { get; init; } = "";
            public string NgayVaoLam { get; init; } = "";
            public string TrangThaiHienThi { get; init; } = "";
        }

        private void CauHinhDataGridView()
        {
            dgvGiaoVien.Dock = DockStyle.Fill;
            dgvGiaoVien.BackgroundColor = Color.White;
            dgvGiaoVien.BorderStyle = BorderStyle.None;
            dgvGiaoVien.AutoGenerateColumns = false;
            dgvGiaoVien.ReadOnly = true;
            dgvGiaoVien.AllowUserToAddRows = false;
            dgvGiaoVien.AllowUserToDeleteRows = false;
            dgvGiaoVien.AllowUserToResizeRows = false;
            dgvGiaoVien.MultiSelect = false;
            dgvGiaoVien.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvGiaoVien.RowHeadersVisible = false;
            dgvGiaoVien.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvGiaoVien.ScrollBars = ScrollBars.Both;
            dgvGiaoVien.EnableHeadersVisualStyles = false;
            dgvGiaoVien.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvGiaoVien.ColumnHeadersHeight = 50;
            dgvGiaoVien.RowTemplate.Height = 36;
            dgvGiaoVien.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(23, 52, 83);
            dgvGiaoVien.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvGiaoVien.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgvGiaoVien.ColumnHeadersDefaultCellStyle.WrapMode =
                DataGridViewTriState.True;
            dgvGiaoVien.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleLeft;
            dgvGiaoVien.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(210, 230, 250);
            dgvGiaoVien.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvGiaoVien.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(245, 248, 252);

            // Chỉ giữ những cột quan trọng. Các thông tin khác
            // (email, địa chỉ, ghi chú, ảnh chứng chỉ...) có ở khung bên trên
            // khi chọn một dòng giáo viên.
            ThemCot("STT", "STT", 45, 45);
            ThemCot("MaHienThi", "Mã GV", 72, 68);
            ThemCot("HoTen", "Họ và tên", 175, 150);
            ThemCot("GioiTinh", "Giới tính", 76, 70);
            ThemCot("SoDienThoai", "Số điện thoại", 115, 100);
            ThemCot("ChuyenMon", "Chuyên môn", 130, 110);
            ThemCot("TrinhDoHocVan", "Trình độ học vấn", 145, 125);
            ThemCot("XepLoaiTotNghiep", "Loại tốt nghiệp", 125, 115);
            ThemCot("NgayVaoLam", "Ngày vào làm", 110, 102);
            ThemCot("TrangThaiHienThi", "Trạng thái", 112, 100);

            dgvGiaoVien.SelectionChanged += DgvGiaoVien_SelectionChanged;

            // Tránh hộp thoại mặc định che màn hình khi phát sinh lỗi
            // định dạng ngoài dự kiến; ghi lỗi vào Output của Visual Studio.
            dgvGiaoVien.DataError += (_, e) =>
            {
                System.Diagnostics.Debug.WriteLine(
                    "DataGridView/GiaoVien: " + e.Exception?.Message);
                e.ThrowException = false;
            };
        }

        private void ThemCot(string ten, string tieuDe,
            float tyLe, int rongToiThieu)
        {
            dgvGiaoVien.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ten,
                HeaderText = tieuDe,
                DataPropertyName = ten,
                ValueType = typeof(string),
                FillWeight = tyLe,
                MinimumWidth = rongToiThieu,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        // 3. TẢI DỮ LIỆU / LỰA CHỌN
        private void NapDanhSach(string tuKhoa = "")
        {
            var danhSach = string.IsNullOrWhiteSpace(tuKhoa)
                ? giaoVienBLL.LayDanhSach()
                : giaoVienBLL.TimKiem(tuKhoa);

            // Lập danh sách chỉ dành cho hiển thị (tất cả kiểu chuỗi).
            // MaGV số nguyên vẫn được giữ riêng để chọn đúng người trong SQL.
            var hienThi = danhSach.Select((gv, i) => new DongGiaoVienHienThi
            {
                MaGV = gv.MaGV,
                STT = (i + 1).ToString(),
                MaHienThi = $"TC{gv.MaGV:D2}",
                HoTen = gv.HoTen ?? "",
                GioiTinh = gv.GioiTinh ?? "",
                SoDienThoai = gv.SoDienThoai ?? "",
                ChuyenMon = gv.ChuyenMon ?? "",
                TrinhDoHocVan = gv.TrinhDoHocVan ?? "",
                XepLoaiTotNghiep = gv.XepLoaiTotNghiep ?? "",
                NgayVaoLam = gv.NgayVaoLam.ToString("dd/MM/yyyy"),
                TrangThaiHienThi = gv.TrangThai ? "Đang làm việc" : "Đã nghỉ"
            }).ToList();

            dangNapDanhSach = true;
            try
            {
                dgvGiaoVien.DataSource = null;
                dgvGiaoVien.DataSource = hienThi;
                dgvGiaoVien.ClearSelection();
                XoaThongTinNhap();
                lblSoLuong.Text = $"Tổng số: {danhSach.Count} giáo viên";
            }
            finally
            {
                dangNapDanhSach = false;
            }
        }

        private void TimKiem() => NapDanhSach(txtTimKiem.Text.Trim());

        private void LamMoi()
        {
            txtTimKiem.Clear();
            NapDanhSach();
        }

        private void DgvGiaoVien_SelectionChanged(object? sender, EventArgs e)
        {
            if (dangNapDanhSach || dgvGiaoVien.SelectedRows.Count == 0)
                return;
            if (dgvGiaoVien.SelectedRows[0].DataBoundItem
                is not DongGiaoVienHienThi hang)
                return;

            XuLyAnToan(() =>
            {
                GiaoVien? gv = giaoVienBLL.LayTheoMa(hang.MaGV);
                if (gv == null) return;
                maGVDangChon = gv.MaGV;
                txtMaGV.Text = $"TC{gv.MaGV:D2}";
                txtHoTen.Text = gv.HoTen;
                if (gv.NgaySinh.HasValue)
                {
                    dtpNgaySinh.Value = gv.NgaySinh.Value;
                    dtpNgaySinh.Checked = true;
                }
                else
                {
                    dtpNgaySinh.Checked = false;
                }
                cboGioiTinh.SelectedItem = gv.GioiTinh;
                txtSoDienThoai.Text = gv.SoDienThoai ?? "";
                txtEmail.Text = gv.Email ?? "";
                txtChuyenMon.Text = gv.ChuyenMon ?? "";
                dtpNgayVaoLam.Value = gv.NgayVaoLam;
                chkTrangThai.Checked = gv.TrangThai;
                txtDiaChi.Text = gv.DiaChi ?? "";
                txtGhiChu.Text = gv.GhiChu ?? "";
                txtTrinhDoHocVan.Text = gv.TrinhDoHocVan ?? "";
                cboXepLoai.SelectedItem = gv.XepLoaiTotNghiep;
                anhChungChiDangChon = gv.AnhChungChi == null
                    ? null : (byte[])gv.AnhChungChi.Clone();
                CapNhatAnhXemTruoc();
            });
        }

        // 4. ẢNH CHỨNG CHỈ - LƯU BYTE[] TRONG SQL
        private void TaiAnh()
        {
            using OpenFileDialog dlg = new()
            {
                Title = "Chọn ảnh chứng chỉ/bằng cấp của giáo viên",
                Filter = "Ảnh PNG/JPG|*.png;*.jpg;*.jpeg",
                Multiselect = false,
                CheckFileExists = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            FileInfo tep = new(dlg.FileName);
            if (tep.Length <= 0 || tep.Length > GioiHanAnh)
                throw new ArgumentException("Chỉ tải ảnh PNG/JPG tối đa 5 MB.");

            byte[] duLieu = File.ReadAllBytes(dlg.FileName);
            // Kiểm tra file có thực sự giải mã được thành hình ảnh.
            using (MemoryStream stream = new(duLieu))
            using (Image img = Image.FromStream(stream, false, true))
            {
                if (img.Width <= 0 || img.Height <= 0)
                    throw new ArgumentException("Hình ảnh không hợp lệ.");
            }
            anhChungChiDangChon = duLieu;
            CapNhatAnhXemTruoc();
        }

        private void BoAnh()
        {
            anhChungChiDangChon = null;
            CapNhatAnhXemTruoc();
        }

        private void CapNhatAnhXemTruoc()
        {
            Image? moi = null;
            if (anhChungChiDangChon != null)
            {
                using MemoryStream stream = new(anhChungChiDangChon);
                using Image img = Image.FromStream(stream, false, true);
                moi = new Bitmap(img); // Copy ra khỏi stream trước khi đóng.
            }
            Image? cu = picChungChi.Image;
            picChungChi.Image = moi;
            cu?.Dispose();
        }

        private void XemAnhLon()
        {
            if (anhChungChiDangChon == null) return;
            XuLyAnToan(() =>
            {
                using MemoryStream stream = new(anhChungChiDangChon);
                using Image img = Image.FromStream(stream, false, true);
                using Bitmap anh = new(img);
                using Form form = new()
                {
                    Text = "Xem ảnh chứng chỉ – giáo viên",
                    Size = new Size(950, 700),
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.White
                };
                using PictureBox pic = new()
                {
                    Dock = DockStyle.Fill,
                    Image = anh,
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                form.Controls.Add(pic);
                form.ShowDialog(this);
            });
        }

        // 5. THÊM / SỬA / XÓA
        private GiaoVien LayDuLieuNhap()
        {
            return new GiaoVien
            {
                HoTen = txtHoTen.Text,
                NgaySinh = dtpNgaySinh.Checked ? dtpNgaySinh.Value.Date : null,
                GioiTinh = cboGioiTinh.SelectedItem?.ToString(),
                SoDienThoai = txtSoDienThoai.Text,
                Email = txtEmail.Text,
                ChuyenMon = txtChuyenMon.Text,
                NgayVaoLam = dtpNgayVaoLam.Value.Date,
                TrangThai = chkTrangThai.Checked,
                DiaChi = txtDiaChi.Text,
                GhiChu = txtGhiChu.Text,
                TrinhDoHocVan = txtTrinhDoHocVan.Text,
                XepLoaiTotNghiep = cboXepLoai.SelectedItem?.ToString(),
                AnhChungChi = anhChungChiDangChon == null
                    ? null : (byte[])anhChungChiDangChon.Clone()
            };
        }

        private void ThemGiaoVien()
        {
            int maMoi = giaoVienBLL.Them(LayDuLieuNhap());
            MessageBox.Show($"Thêm giáo viên thành công!\nMã giáo viên: TC{maMoi:D2}",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LamMoi();
        }

        private void SuaGiaoVien()
        {
            if (!maGVDangChon.HasValue)
            {
                MessageBox.Show("Vui lòng chọn giáo viên cần sửa!", "Thông báo");
                return;
            }
            GiaoVien gv = LayDuLieuNhap();
            gv.MaGV = maGVDangChon.Value;
            if (!giaoVienBLL.Sua(gv))
            {
                MessageBox.Show("Không tìm thấy giáo viên cần sửa.");
                return;
            }
            MessageBox.Show("Cập nhật giáo viên thành công!", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            LamMoi();
        }

        private void XoaGiaoVien()
        {
            if (!maGVDangChon.HasValue)
            {
                MessageBox.Show("Vui lòng chọn giáo viên cần xóa!", "Thông báo");
                return;
            }
            DialogResult xacNhan = MessageBox.Show(
                "Bạn có chắc muốn xóa giáo viên này?\n" +
                "Dữ liệu sẽ không thể khôi phục nếu không có bản sao lưu.",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (xacNhan != DialogResult.Yes) return;
            if (!giaoVienBLL.Xoa(maGVDangChon.Value))
            {
                MessageBox.Show("Không tìm thấy giáo viên cần xóa.");
                return;
            }
            MessageBox.Show("Xóa giáo viên thành công!", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            LamMoi();
        }

        private void XoaThongTinNhap()
        {
            maGVDangChon = null;
            txtMaGV.Clear();
            txtHoTen.Clear();
            dtpNgaySinh.Value = DateTime.Today;
            dtpNgaySinh.Checked = false;
            cboGioiTinh.SelectedIndex = -1;
            txtSoDienThoai.Clear();
            txtEmail.Clear();
            txtChuyenMon.Clear();
            dtpNgayVaoLam.Value = DateTime.Today;
            chkTrangThai.Checked = true;
            txtDiaChi.Clear();
            txtGhiChu.Clear();
            txtTrinhDoHocVan.Clear();
            cboXepLoai.SelectedIndex = -1;
            BoAnh();
        }

        private static void XuLyAnToan(Action thaoTac)
        {
            try
            {
                thaoTac();
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
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi làm việc với dữ liệu:\n" + ex.Message,
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
