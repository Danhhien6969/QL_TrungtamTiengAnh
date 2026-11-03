
using System;
using System.Drawing;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmHocVien : Form
    {
        // ==========================================
        // 1. MÀU SẮC
        // ==========================================

        private static readonly Color Navy =
            Color.FromArgb(23, 52, 83);

        private static readonly Color Blue =
            Color.FromArgb(40, 110, 190);

        private static readonly Color Nen =
            Color.FromArgb(245, 248, 252);

        private static readonly Color Xam =
            Color.FromArgb(105, 120, 138);

        private static readonly Color Vien =
            Color.FromArgb(224, 232, 241);

        // ==========================================
        // 2. KHAI BÁO
        // ==========================================

        private readonly HocVienBLL hocVienBLL = new();

        private int? maHVDangChon;
        private bool dangNap;
        private bool dangXemThungRac;

        private Button btnThem = null!;
        private Button btnSua = null!;
        private Button btnXoa = null!;
        private Button btnKhoiPhuc = null!;
        private Button btnThungRac = null!;

        private Label lblTieuDeDanhSach = null!;

        private readonly TextBox txtMaHV = new();
        private readonly TextBox txtHoTen = new();
        private readonly NumericUpDown nudTuoi = new();
        private readonly TextBox txtLopTruong = new();
        private readonly DateTimePicker dtpNgaySinh = new();
        private readonly ComboBox cboGioiTinh = new();
        private readonly TextBox txtSoDienThoai = new();
        private readonly TextBox txtDiaChi = new();

        private readonly TextBox txtHoTenPhuHuynh = new();
        private readonly TextBox txtSdtPhuHuynh = new();

        private readonly TextBox txtTimKiem = new();

        private readonly DataGridView dgvHocVien = new();
        private readonly Label lblSoLuong = new();

        // ==========================================
        // 3. CONSTRUCTOR
        // ==========================================

        public frmHocVien()
        {
            InitializeComponent();
            TaoGiaoDien();

            Shown += (_, _) =>
                XuLyAnToan(NapDanhSach);
        }

        // ==========================================
        // 4. THIẾT KẾ GIAO DIỆN
        // ĐÃ BỎ HẲN PHẦN HỒ SƠ HỌC VIÊN
        // ==========================================

        private void TaoGiaoDien()
        {
            SuspendLayout();

            Text = "Quản lý học viên";
            ClientSize = new Size(1200, 760);
            MinimumSize = Size.Empty;

            BackColor = Nen;
            Font = new Font("Segoe UI", 10);
            AutoScaleMode = AutoScaleMode.Dpi;

            Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(18, 14, 18, 16),
                BackColor = Nen,
                Margin = Padding.Empty
            };

            root.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            // Hàng 1: Thông tin học viên, phụ huynh
            root.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 280));

            // Hàng 2: Các nút chức năng
            root.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 66));

            // Hàng 3: Danh sách
            root.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            Controls.Add(root);

            // ======================================
            // KHUNG THÔNG TIN
            // ======================================

            var khuNhap = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 10)
            };

            khuNhap.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 67));

            khuNhap.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 33));

            khuNhap.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            // ======================================
            // THÔNG TIN HỌC VIÊN
            // ======================================

            var cardHocVien = TaoCard();
            cardHocVien.Margin =
                new Padding(0, 0, 7, 0);

            var layoutHocVien = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(15, 10, 15, 10)
            };

            layoutHocVien.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            layoutHocVien.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 36));

            layoutHocVien.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layoutHocVien.Controls.Add(
                TaoTieuDeKhung("THÔNG TIN HỌC VIÊN"),
                0, 0);

            var gridHocVien = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Margin = Padding.Empty
            };

            gridHocVien.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 100));

            gridHocVien.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            gridHocVien.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 105));

            gridHocVien.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            for (int i = 0; i < 4; i++)
            {
                gridHocVien.RowStyles.Add(
                    new RowStyle(SizeType.Percent, 25));
            }

            txtMaHV.ReadOnly = true;
            txtMaHV.BackColor =
                Color.FromArgb(239, 243, 248);
            txtMaHV.PlaceholderText = "Tự động";

            nudTuoi.Minimum = 0;
            nudTuoi.Maximum = 120;

            dtpNgaySinh.Format =
                DateTimePickerFormat.Custom;
            dtpNgaySinh.CustomFormat = "dd/MM/yyyy";
            dtpNgaySinh.ShowCheckBox = true;
            dtpNgaySinh.Checked = false;

            cboGioiTinh.DropDownStyle =
                ComboBoxStyle.DropDownList;
            cboGioiTinh.Items.AddRange(
                new object[] { "Nam", "Nữ", "Khác" });

            txtHoTen.PlaceholderText =
                "Nhập họ và tên";

            txtLopTruong.PlaceholderText =
                "VD: 5A1";

            txtSoDienThoai.PlaceholderText =
                "Không bắt buộc";

            txtDiaChi.PlaceholderText =
                "Nhập địa chỉ";

            ThemTruong(gridHocVien,
                "Mã học viên", txtMaHV, 0, 0);

            ThemTruong(gridHocVien,
                "Họ và tên (*)", txtHoTen, 0, 1);

            ThemTruong(gridHocVien,
                "Tuổi", nudTuoi, 1, 0);

            ThemTruong(gridHocVien,
                "Học sinh lớp", txtLopTruong, 1, 1);

            ThemTruong(gridHocVien,
                "Ngày sinh", dtpNgaySinh, 2, 0);

            ThemTruong(gridHocVien,
                "Giới tính", cboGioiTinh, 2, 1);

            ThemTruong(gridHocVien,
                "SĐT học viên", txtSoDienThoai, 3, 0);

            ThemTruong(gridHocVien,
                "Địa chỉ", txtDiaChi, 3, 1);

            layoutHocVien.Controls.Add(
                gridHocVien, 0, 1);

            cardHocVien.Controls.Add(layoutHocVien);
            khuNhap.Controls.Add(cardHocVien, 0, 0);

            // ======================================
            // THÔNG TIN PHỤ HUYNH
            // ======================================

            var cardPhuHuynh = TaoCard();
            cardPhuHuynh.Margin =
                new Padding(7, 0, 0, 0);

            var layoutPhuHuynh = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(16, 10, 16, 10)
            };

            layoutPhuHuynh.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            layoutPhuHuynh.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 36));

            layoutPhuHuynh.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 27));

            layoutPhuHuynh.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 44));

            layoutPhuHuynh.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 27));

            layoutPhuHuynh.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 44));

            layoutPhuHuynh.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layoutPhuHuynh.Controls.Add(
                TaoTieuDeKhung("THÔNG TIN PHỤ HUYNH"),
                0, 0);

            layoutPhuHuynh.Controls.Add(
                TaoNhan("Họ và tên phụ huynh"),
                0, 1);

            txtHoTenPhuHuynh.PlaceholderText =
                "Nhập tên phụ huynh";
            CauHinhInput(txtHoTenPhuHuynh);

            layoutPhuHuynh.Controls.Add(
                txtHoTenPhuHuynh, 0, 2);

            layoutPhuHuynh.Controls.Add(
                TaoNhan("Số điện thoại phụ huynh"),
                0, 3);

            txtSdtPhuHuynh.PlaceholderText =
                "Nhập số điện thoại";
            CauHinhInput(txtSdtPhuHuynh);

            layoutPhuHuynh.Controls.Add(
                txtSdtPhuHuynh, 0, 4);

            var pnlGhiChu = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor =
                    Color.FromArgb(238, 245, 252),
                Margin = new Padding(0, 9, 0, 0),
                Padding = new Padding(10)
            };

            pnlGhiChu.Controls.Add(new Label
            {
                Text =
                    "Thông tin phụ huynh được dùng " +
                    "để liên hệ khi cần.",
                Dock = DockStyle.Fill,
                ForeColor = Xam,
                Font = new Font("Segoe UI", 9),
                TextAlign =
                    ContentAlignment.MiddleLeft
            });

            layoutPhuHuynh.Controls.Add(
                pnlGhiChu, 0, 5);

            cardPhuHuynh.Controls.Add(
                layoutPhuHuynh);

            khuNhap.Controls.Add(
                cardPhuHuynh, 1, 0);

            // ĐƯA THẲNG LÊN HÀNG ĐẦU
            root.Controls.Add(khuNhap, 0, 0);

            // ======================================
            // THANH THAO TÁC
            // ======================================

            var thanhThaoTac = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection =
                    FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(0, 8, 0, 0)
            };

            btnThem = TaoNut(
                "Thêm",
                Color.FromArgb(27, 139, 91));

            btnSua = TaoNut(
                "Sửa", Blue);

            btnXoa = TaoNut(
                "Xóa",
                Color.FromArgb(194, 63, 70));

            btnKhoiPhuc = TaoNut(
                "Khôi phục",
                Color.FromArgb(27, 139, 91));

            btnThungRac = TaoNut(
                "Thùng rác",
                Color.FromArgb(118, 86, 145));

            btnThungRac.Width = 120;

            var btnLamMoi = TaoNut(
                "Làm mới",
                Color.FromArgb(95, 110, 125));

            thanhThaoTac.Controls.AddRange(
                new Control[]
                {
                    btnThem,
                    btnSua,
                    btnXoa,
                    btnKhoiPhuc,
                    btnThungRac,
                    btnLamMoi
                });

            // ĐƯA THANH NÚT LÊN HÀNG THỨ HAI
            root.Controls.Add(thanhThaoTac, 0, 1);

            // ======================================
            // DANH SÁCH HỌC VIÊN
            // ======================================

            var cardDanhSach = TaoCard();

            var layoutDanhSach = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(14, 10, 14, 10)
            };

            layoutDanhSach.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            layoutDanhSach.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 48));

            layoutDanhSach.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layoutDanhSach.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 28));

            var thanhDanhSach = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 7)
            };

            thanhDanhSach.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            thanhDanhSach.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 420));

            lblTieuDeDanhSach = new Label
            {
                Text = "DANH SÁCH HỌC VIÊN",
                Dock = DockStyle.Fill,
                ForeColor = Navy,
                Font = new Font(
                    "Segoe UI", 12, FontStyle.Bold),
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            thanhDanhSach.Controls.Add(
                lblTieuDeDanhSach, 0, 0);

            var pnlTimKiem = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };

            pnlTimKiem.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            pnlTimKiem.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 100));

            txtTimKiem.Dock = DockStyle.Fill;
            txtTimKiem.Font =
                new Font("Segoe UI", 10);
            txtTimKiem.PlaceholderText =
                "Tìm tên, mã, lớp, SĐT...";
            txtTimKiem.Margin =
                new Padding(0, 5, 8, 5);

            var btnTimKiem = TaoNut(
                "Tìm kiếm", Navy);

            btnTimKiem.Dock = DockStyle.Fill;
            btnTimKiem.Width = 100;

            pnlTimKiem.Controls.Add(
                txtTimKiem, 0, 0);

            pnlTimKiem.Controls.Add(
                btnTimKiem, 1, 0);

            thanhDanhSach.Controls.Add(
                pnlTimKiem, 1, 0);

            layoutDanhSach.Controls.Add(
                thanhDanhSach, 0, 0);

            // ======================================
            // BẢNG
            // ======================================

            CauHinhBang();

            layoutDanhSach.Controls.Add(
                dgvHocVien, 0, 1);

            lblSoLuong.Text =
                "Tổng số: 0 học viên";

            lblSoLuong.Dock = DockStyle.Fill;
            lblSoLuong.ForeColor = Xam;

            lblSoLuong.Font =
                new Font("Segoe UI", 9.5f);

            lblSoLuong.TextAlign =
                ContentAlignment.MiddleRight;

            layoutDanhSach.Controls.Add(
                lblSoLuong, 0, 2);

            cardDanhSach.Controls.Add(
                layoutDanhSach);

            // ĐƯA BẢNG LÊN HÀNG THỨ BA
            root.Controls.Add(cardDanhSach, 0, 2);

            // ======================================
            // SỰ KIỆN
            // ======================================

            btnThem.Click += (_, _) =>
                XuLyAnToan(ThemHocVien);

            btnSua.Click += (_, _) =>
                XuLyAnToan(SuaHocVien);

            btnXoa.Click += (_, _) =>
                XuLyAnToan(XoaHocVien);

            btnKhoiPhuc.Click += (_, _) =>
                XuLyAnToan(KhoiPhucHocVien);

            btnThungRac.Click += (_, _) =>
                XuLyAnToan(ChuyenCheDoDanhSach);

            btnLamMoi.Click += (_, _) =>
                XuLyAnToan(LamMoi);

            btnTimKiem.Click += (_, _) =>
                XuLyAnToan(TimKiem);

            txtTimKiem.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter)
                    return;

                e.SuppressKeyPress = true;
                XuLyAnToan(TimKiem);
            };

            CapNhatCheDoDanhSach();
            XoaThongTinNhap();

            ResumeLayout(true);
        }

        // ==========================================
        // 5. HÀM GIAO DIỆN
        // ==========================================

        private static Panel TaoCard()
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Margin = Padding.Empty
            };

            card.Paint += (_, e) =>
            {
                if (card.Width < 2 ||
                    card.Height < 2)
                    return;

                using var pen = new Pen(Vien);

                e.Graphics.DrawRectangle(
                    pen,
                    0, 0,
                    card.Width - 1,
                    card.Height - 1);
            };

            return card;
        }

        private static Label TaoTieuDeKhung(
            string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                ForeColor = Navy,
                Font = new Font(
                    "Segoe UI", 10, FontStyle.Bold),
                TextAlign =
                    ContentAlignment.MiddleLeft
            };
        }

        private static Label TaoNhan(
            string text)
        {
            return new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                ForeColor =
                    Color.FromArgb(55, 69, 85),
                Font = new Font(
                    "Segoe UI", 9.5f),
                TextAlign =
                    ContentAlignment.MiddleLeft
            };
        }

        private static void CauHinhInput(
            Control input)
        {
            input.Dock = DockStyle.Fill;
            input.Font =
                new Font("Segoe UI", 10.5f);

            input.Margin =
                new Padding(0, 4, 0, 7);
        }

        private static void ThemTruong(
            TableLayoutPanel grid,
            string nhan,
            Control input,
            int dong,
            int nua)
        {
            int cot = nua * 2;

            grid.Controls.Add(
                TaoNhan(nhan), cot, dong);

            input.Dock = DockStyle.Fill;
            input.Margin =
                new Padding(4, 7, 11, 7);

            input.Font =
                new Font("Segoe UI", 10);

            grid.Controls.Add(
                input, cot + 1, dong);
        }

        private static Button TaoNut(
            string ten,
            Color mau)
        {
            var btn = new Button
            {
                Text = ten,
                Width = 105,
                Height = 38,
                BackColor = mau,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font(
                    "Segoe UI", 9.5f,
                    FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(
                    0, 0, 7, 4),
                UseVisualStyleBackColor = false
            };

            btn.FlatAppearance.BorderSize = 0;

            return btn;
        }

        // ==========================================
        // 6. CẤU HÌNH DATAGRIDVIEW
        // ==========================================

        private void CauHinhBang()
        {
            dgvHocVien.Dock = DockStyle.Fill;
            dgvHocVien.BackgroundColor = Color.White;
            dgvHocVien.BorderStyle = BorderStyle.None;

            dgvHocVien.AutoGenerateColumns = false;
            dgvHocVien.ReadOnly = true;

            dgvHocVien.AllowUserToAddRows = false;
            dgvHocVien.AllowUserToDeleteRows = false;
            dgvHocVien.AllowUserToResizeRows = false;

            dgvHocVien.MultiSelect = false;

            dgvHocVien.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvHocVien.RowHeadersVisible = false;

            dgvHocVien.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.None;

            dgvHocVien.ScrollBars = ScrollBars.Both;
            dgvHocVien.EnableHeadersVisualStyles = false;

            dgvHocVien.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode
                    .DisableResizing;

            dgvHocVien.ColumnHeadersHeight = 42;
            dgvHocVien.RowTemplate.Height = 34;

            dgvHocVien.GridColor =
                Color.FromArgb(230, 235, 241);

            dgvHocVien.ColumnHeadersDefaultCellStyle
                .BackColor = Navy;

            dgvHocVien.ColumnHeadersDefaultCellStyle
                .ForeColor = Color.White;

            dgvHocVien.ColumnHeadersDefaultCellStyle
                .Font = new Font(
                    "Segoe UI", 9.5f,
                    FontStyle.Bold);

            dgvHocVien.DefaultCellStyle.Font =
                new Font("Segoe UI", 9.5f);

            dgvHocVien.DefaultCellStyle
                .SelectionBackColor =
                Color.FromArgb(218, 233, 250);

            dgvHocVien.DefaultCellStyle
                .SelectionForeColor = Color.Black;

            dgvHocVien.AlternatingRowsDefaultCellStyle
                .BackColor =
                Color.FromArgb(248, 250, 253);

            dgvHocVien.Columns.Clear();

            dgvHocVien.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = "STT",
                    HeaderText = "STT",
                    Width = 55,
                    MinimumWidth = 55,
                    ReadOnly = true,
                    SortMode =
                        DataGridViewColumnSortMode.NotSortable
                });

            ThemCot("MaHV", "Mã (VT)", 95);
            ThemCot("HoTen", "Họ và tên", 175);
            ThemCot("Tuoi", "Tuổi", 60);
            ThemCot("LopTruong", "Học sinh lớp", 100);
            ThemCot("NgaySinh", "Ngày sinh", 115,
                "dd/MM/yyyy");
            ThemCot("GioiTinh", "Giới tính", 85);
            ThemCot("SoDienThoai", "SĐT học viên", 130);

            ThemCot(
                "HoTenPhuHuynh",
                "Họ tên phụ huynh", 175);

            ThemCot(
                "SoDienThoaiPhuHuynh",
                "SĐT phụ huynh", 135);

            ThemCot("DiaChi", "Địa chỉ", 185);

            ThemCot(
                "NgayDangKy",
                "Ngày đăng ký", 125,
                "dd/MM/yyyy");

            dgvHocVien.CellFormatting +=
                DgvHocVien_CellFormatting;

            dgvHocVien.SelectionChanged +=
                DgvHocVien_SelectionChanged;
        }

        private void ThemCot(
            string ten,
            string nhan,
            int doRong,
            string? dinhDang = null)
        {
            var cot = new DataGridViewTextBoxColumn
            {
                Name = ten,
                DataPropertyName = ten,
                HeaderText = nhan,
                Width = doRong,
                MinimumWidth = doRong,
                ReadOnly = true,
                SortMode =
                    DataGridViewColumnSortMode.NotSortable
            };

            if (dinhDang != null)
            {
                cot.DefaultCellStyle.Format =
                    dinhDang;
            }

            dgvHocVien.Columns.Add(cot);
        }

        // ==========================================
        // 7. MÃ HỌC VIÊN
        // ==========================================

        private static string DinhDangMaHocVien(
            int maHV)
        {
            return $"VT{maHV:D2}";
        }

        private void DgvHocVien_CellFormatting(
            object? sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex < 0 ||
                dgvHocVien.Columns[e.ColumnIndex].Name
                    != "MaHV" ||
                e.Value is not int maHV)
            {
                return;
            }

            e.Value =
                DinhDangMaHocVien(maHV);

            e.FormattingApplied = true;
        }

        // ==========================================
        // 8. NẠP DANH SÁCH
        // ==========================================

        private void NapDanhSach()
        {
            NapDanhSach(string.Empty);
        }

        private void NapDanhSach(
            string tuKhoa)
        {
            var danhSach = dangXemThungRac
                ? (string.IsNullOrWhiteSpace(tuKhoa)
                    ? hocVienBLL.LayDanhSachDaXoa()
                    : hocVienBLL.TimKiemDaXoa(tuKhoa))
                : (string.IsNullOrWhiteSpace(tuKhoa)
                    ? hocVienBLL.LayDanhSach()
                    : hocVienBLL.TimKiem(tuKhoa));

            dangNap = true;

            try
            {
                dgvHocVien.DataSource = null;
                dgvHocVien.DataSource = danhSach;

                for (int i = 0;
                     i < dgvHocVien.Rows.Count;
                     i++)
                {
                    dgvHocVien.Rows[i]
                        .Cells["STT"].Value = i + 1;
                }

                dgvHocVien.ClearSelection();
            }
            finally
            {
                dangNap = false;
            }

            lblSoLuong.Text =
                dangXemThungRac
                    ? $"Đã lưu trữ: {danhSach.Count} học viên"
                    : $"Tổng số: {danhSach.Count} học viên";

            XoaThongTinNhap();
        }

        // ==========================================
        // 9. THÙNG RÁC
        // ==========================================

        private void ChuyenCheDoDanhSach()
        {
            dangXemThungRac =
                !dangXemThungRac;

            txtTimKiem.Clear();

            CapNhatCheDoDanhSach();
            NapDanhSach();
        }

        private void CapNhatCheDoDanhSach()
        {
            btnThem.Enabled =
                !dangXemThungRac;

            btnSua.Enabled =
                !dangXemThungRac;

            btnXoa.Visible =
                !dangXemThungRac;

            btnKhoiPhuc.Visible =
                dangXemThungRac;

            btnThungRac.Text =
                dangXemThungRac
                    ? "Danh sách chính"
                    : "Thùng rác";

            btnThungRac.Width =
                dangXemThungRac
                    ? 150
                    : 120;

            lblTieuDeDanhSach.Text =
                dangXemThungRac
                    ? "THÙNG RÁC – HỌC VIÊN ĐÃ LƯU TRỮ"
                    : "DANH SÁCH HỌC VIÊN";
        }

        private void TimKiem()
        {
            NapDanhSach(
                txtTimKiem.Text.Trim());
        }

        private void LamMoi()
        {
            txtTimKiem.Clear();
            NapDanhSach();
        }

        // ==========================================
        // 10. LẤY DỮ LIỆU NHẬP
        // ==========================================

        private HocVien LayDuLieuNhap()
        {
            return new HocVien
            {
                HoTen = txtHoTen.Text,

                Tuoi = nudTuoi.Value == 0
                    ? null
                    : (int)nudTuoi.Value,

                LopTruong = txtLopTruong.Text,

                NgaySinh = dtpNgaySinh.Checked
                    ? dtpNgaySinh.Value.Date
                    : null,

                GioiTinh =
                    cboGioiTinh.SelectedItem?.ToString(),

                SoDienThoai =
                    txtSoDienThoai.Text,

                DiaChi =
                    txtDiaChi.Text,

                HoTenPhuHuynh =
                    txtHoTenPhuHuynh.Text,

                SoDienThoaiPhuHuynh =
                    txtSdtPhuHuynh.Text
            };
        }

        // ==========================================
        // 11. THÊM HỌC VIÊN
        // ==========================================

        private void ThemHocVien()
        {
            if (dangXemThungRac)
            {
                throw new InvalidOperationException(
                    "Hãy trở về danh sách chính " +
                    "để thêm học viên.");
            }

            if (maHVDangChon.HasValue)
            {
                throw new InvalidOperationException(
                    "Bạn đang chọn học viên cũ. " +
                    "Nhấn Làm mới trước khi thêm.");
            }

            int maMoi =
                hocVienBLL.Them(LayDuLieuNhap());

            MessageBox.Show(
                "Thêm học viên thành công!\n" +
                $"Mã: {DinhDangMaHocVien(maMoi)}",
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LamMoi();
        }

        // ==========================================
        // 12. SỬA HỌC VIÊN
        // ==========================================

        private void SuaHocVien()
        {
            if (dangXemThungRac)
            {
                throw new InvalidOperationException(
                    "Hãy khôi phục học viên " +
                    "trước khi sửa.");
            }

            if (!maHVDangChon.HasValue)
            {
                throw new ArgumentException(
                    "Vui lòng chọn học viên cần sửa.");
            }

            HocVien hv = LayDuLieuNhap();
            hv.MaHV = maHVDangChon.Value;

            if (!hocVienBLL.Sua(hv))
            {
                MessageBox.Show(
                    "Không tìm thấy học viên cần sửa.");
                return;
            }

            MessageBox.Show(
                "Cập nhật học viên thành công!",
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LamMoi();
        }

        // ==========================================
        // 13. XÓA HỌC VIÊN
        // ==========================================

        private void XoaHocVien()
        {
            if (dangXemThungRac)
            {
                throw new InvalidOperationException(
                    "Bạn đang xem thùng rác.");
            }

            if (!maHVDangChon.HasValue)
            {
                throw new ArgumentException(
                    "Vui lòng chọn học viên cần xóa.");
            }

            var xacNhan = MessageBox.Show(
                "Chuyển học viên vào thùng rác?\n" +
                "Thông tin và mã VT vẫn được giữ " +
                "trong SQL Server.",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (xacNhan != DialogResult.Yes)
                return;

            if (!hocVienBLL.Xoa(
                maHVDangChon.Value))
            {
                MessageBox.Show(
                    "Không tìm thấy học viên cần xóa.");
                return;
            }

            MessageBox.Show(
                "Đã chuyển học viên vào thùng rác.",
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            LamMoi();
        }

        // ==========================================
        // 14. KHÔI PHỤC HỌC VIÊN
        // ==========================================

        private void KhoiPhucHocVien()
        {
            if (!dangXemThungRac)
            {
                throw new InvalidOperationException(
                    "Hãy mở thùng rác trước.");
            }

            if (!maHVDangChon.HasValue)
            {
                throw new ArgumentException(
                    "Vui lòng chọn học viên cần khôi phục.");
            }

            int maHV = maHVDangChon.Value;

            var xacNhan = MessageBox.Show(
                $"Khôi phục học viên " +
                $"{DinhDangMaHocVien(maHV)}?",
                "Xác nhận khôi phục",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (xacNhan != DialogResult.Yes)
                return;

            if (!hocVienBLL.KhoiPhuc(maHV))
            {
                MessageBox.Show(
                    "Không tìm thấy học viên đã lưu trữ.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            MessageBox.Show(
                "Khôi phục học viên thành công!",
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            NapDanhSach();
        }

        // ==========================================
        // 15. CHỌN HỌC VIÊN
        // ==========================================

        private void DgvHocVien_SelectionChanged(
            object? sender,
            EventArgs e)
        {
            if (dangNap ||
                dgvHocVien.SelectedRows.Count == 0)
            {
                return;
            }

            if (dgvHocVien.SelectedRows[0]
                .DataBoundItem is not HocVien hv)
            {
                return;
            }

            maHVDangChon = hv.MaHV;

            txtMaHV.Text =
                DinhDangMaHocVien(hv.MaHV);

            txtHoTen.Text = hv.HoTen;

            nudTuoi.Value =
                hv.Tuoi.HasValue &&
                hv.Tuoi.Value >= 1 &&
                hv.Tuoi.Value <= 120
                    ? hv.Tuoi.Value
                    : 0;

            txtLopTruong.Text =
                hv.LopTruong ?? "";

            dtpNgaySinh.Checked =
                hv.NgaySinh.HasValue;

            if (hv.NgaySinh.HasValue)
            {
                dtpNgaySinh.Value =
                    hv.NgaySinh.Value;
            }

            cboGioiTinh.SelectedItem =
                hv.GioiTinh;

            txtSoDienThoai.Text =
                hv.SoDienThoai ?? "";

            txtDiaChi.Text =
                hv.DiaChi ?? "";

            txtHoTenPhuHuynh.Text =
                hv.HoTenPhuHuynh ?? "";

            txtSdtPhuHuynh.Text =
                hv.SoDienThoaiPhuHuynh ?? "";
        }

        // ==========================================
        // 16. LÀM MỚI Ô NHẬP
        // ==========================================

        private void XoaThongTinNhap()
        {
            maHVDangChon = null;

            txtMaHV.Clear();
            txtHoTen.Clear();

            nudTuoi.Value = 0;
            txtLopTruong.Clear();

            dtpNgaySinh.Value = DateTime.Today;
            dtpNgaySinh.Checked = false;

            cboGioiTinh.SelectedIndex = -1;

            txtSoDienThoai.Clear();
            txtDiaChi.Clear();

            txtHoTenPhuHuynh.Clear();
            txtSdtPhuHuynh.Clear();

            dgvHocVien.ClearSelection();
        }

        // ==========================================
        // 17. XỬ LÝ LỖI
        // ==========================================

        private static void XuLyAnToan(
            Action thaoTac)
        {
            try
            {
                thaoTac();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Dữ liệu không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Không thể thực hiện",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Có lỗi khi làm việc với dữ liệu:\n" +
                    ex.Message,
                    "Lỗi hệ thống",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
