
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmHocPhi : Form
    {
        private readonly HocPhiBLL bll = new();

        // =========================================
        // CONTROL THÔNG TIN THANH TOÁN
        // =========================================

        private readonly ComboBox cboDangKy = new();
        private readonly ComboBox cboHinhThuc = new();

        private readonly TextBox txtHocPhi = new();
        private readonly TextBox txtDaDong = new();
        private readonly TextBox txtConNo = new();
        private readonly TextBox txtTrangThai = new();
        private readonly TextBox txtGhiChu = new();
        private readonly TextBox txtTimKiem = new();

        private readonly NumericUpDown nudSoTien = new();

        private readonly Button btnThanhToanQR = new();
        private readonly Button btnThu = new();

        // CHỈ GIỮ MỘT BẢNG LỊCH SỬ
        private readonly DataGridView dgvLichSu = new();
        private readonly Label lblLichSu = new();

        private bool dangNap;
        private int? maDKDangXem;

        // =========================================
        // DỮ LIỆU HIỂN THỊ LỊCH SỬ TỔNG HỢP
        // =========================================

        private sealed class DongLichSuTongHop
        {
            public int MaDK { get; set; }

            public string MaTT { get; set; } = "";

            public DateTime NgayThanhToan { get; set; }

            public string TenHocVien { get; set; } = "";

            public string TenLop { get; set; } = "";

            public string SoTien { get; set; } = "";

            public string HinhThuc { get; set; } = "";

            public string GhiChu { get; set; } = "";

            // Dùng nội bộ, không đưa thành cột.
            public bool CoDemo { get; set; }
        }

        public frmHocPhi()
        {
            InitializeComponent();
            TaoGiaoDien();

            Shown += (_, _) =>
                XuLyAnToan(() => TaiDuLieu());
        }

        // =========================================
        // HÀM TẠO NÚT
        // =========================================

        private static Button TaoNut(
            string ten,
            Color mau,
            int rong = 120)
        {
            var btn = new Button
            {
                Text = ten,
                Width = rong,
                Height = 40,
                BackColor = mau,
                ForeColor = Color.White,
                Font = new Font(
                    "Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 5)
            };

            btn.FlatAppearance.BorderSize = 0;

            return btn;
        }

        // =========================================
        // HÀM TẠO TRƯỜNG THÔNG TIN
        // =========================================

        private static void ThemTruong(
            TableLayoutPanel grid,
            string nhan,
            Control input,
            int dong,
            int nua)
        {
            int cot = nua * 2;

            var lbl = new Label
            {
                Text = nhan,
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = new Font(
                    "Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(45, 55, 70),
                TextAlign = ContentAlignment.MiddleLeft
            };

            grid.Controls.Add(lbl, cot, dong);

            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(4, 7, 12, 7);
            input.Font = new Font(
                "Segoe UI", 10, FontStyle.Regular);

            grid.Controls.Add(input, cot + 1, dong);
        }

        // =========================================
        // THIẾT KẾ GIAO DIỆN CHÍNH
        // =========================================

        private void TaoGiaoDien()
        {
            Text = "Quản lý học phí";
            ClientSize = new Size(1180, 780);
            MinimumSize = new Size(950, 650);
            StartPosition = FormStartPosition.CenterParent;

            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font(
                "Segoe UI", 10, FontStyle.Regular);

            Controls.Clear();

            // CHỈ CÒN 4 KHU VỰC:
            // 1. Tiêu đề
            // 2. Thông tin thanh toán
            // 3. Các nút chức năng
            // 4. Lịch sử thanh toán

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 4
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 73));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 237));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 70));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            Controls.Add(layout);

            // =====================================
            // 1. TIÊU ĐỀ
            // =====================================

            var header = new Panel
            {
                Dock = DockStyle.Fill
            };

            header.Controls.Add(new Label
            {
                Text = "QUẢN LÝ HỌC PHÍ",
                Location = new Point(5, 5),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI", 21, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });

            header.Controls.Add(new Label
            {
                Text =
                    "Thu học phí nhiều đợt, " +
                    "theo dõi thanh toán và lịch sử học phí",
                Location = new Point(7, 47),
                AutoSize = true,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.DimGray
            });

            layout.Controls.Add(header, 0, 0);

            // =====================================
            // 2. THÔNG TIN THANH TOÁN
            // =====================================

            var grp = new GroupBox
            {
                Text = "THÔNG TIN THANH TOÁN",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(23, 52, 83),
                Font = new Font(
                    "Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(12)
            };

            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 4,
                Padding = new Padding(8)
            };

            grid.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 122));

            grid.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            grid.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 122));

            grid.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50));

            for (int i = 0; i < 4; i++)
            {
                grid.RowStyles.Add(
                    new RowStyle(SizeType.Percent, 25));
            }

            cboDangKy.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cboHinhThuc.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cboHinhThuc.Items.AddRange(new object[]
            {
                "Tiền mặt",
                "Chuyển khoản",
                "Thẻ"
            });

            cboHinhThuc.SelectedIndex = 0;

            foreach (var o in new[]
            {
                txtHocPhi,
                txtDaDong,
                txtConNo,
                txtTrangThai
            })
            {
                o.ReadOnly = true;
                o.BackColor =
                    Color.FromArgb(240, 243, 247);
            }

            txtHocPhi.TextAlign =
                HorizontalAlignment.Right;

            txtDaDong.TextAlign =
                HorizontalAlignment.Right;

            txtConNo.TextAlign =
                HorizontalAlignment.Right;

            txtGhiChu.MaxLength = 255;

            nudSoTien.Minimum = 0;
            nudSoTien.Maximum = 1000000000000m;
            nudSoTien.DecimalPlaces = 0;
            nudSoTien.ThousandsSeparator = true;
            nudSoTien.Increment = 100000m;

            nudSoTien.TextAlign =
                HorizontalAlignment.Right;

            ThemTruong(
                grid, "Đăng ký học (*)",
                cboDangKy, 0, 0);

            ThemTruong(
                grid, "Hình thức (*)",
                cboHinhThuc, 0, 1);

            ThemTruong(
                grid, "Học phí phải đóng",
                txtHocPhi, 1, 0);

            ThemTruong(
                grid, "Đã thanh toán",
                txtDaDong, 1, 1);

            ThemTruong(
                grid, "Còn nợ",
                txtConNo, 2, 0);

            ThemTruong(
                grid, "Số tiền thu (*)",
                nudSoTien, 2, 1);

            ThemTruong(
                grid, "Trạng thái",
                txtTrangThai, 3, 0);

            ThemTruong(
                grid, "Ghi chú",
                txtGhiChu, 3, 1);

            grp.Controls.Add(grid);
            layout.Controls.Add(grp, 0, 1);

            // =====================================
            // 3. THANH CÔNG CỤ
            // =====================================

            var thanhCongCu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection =
                    FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 0)
            };

            CauHinhNut(
                btnThu,
                "Ghi nhận thu",
                Color.FromArgb(30, 140, 95),
                145);

            CauHinhNut(
                btnThanhToanQR,
                "Thanh toán QR",
                Color.FromArgb(40, 110, 190),
                165);

            var btnLamMoi = TaoNut(
                "Làm mới",
                Color.FromArgb(95, 110, 125));

            var btnTim = TaoNut(
                "Tìm kiếm",
                Color.FromArgb(23, 52, 83));

            var btnTatCa = TaoNut(
                "Tất cả giao dịch",
                Color.FromArgb(40, 110, 190),
                165);

            txtTimKiem.Width = 230;
            txtTimKiem.Font = new Font(
                "Segoe UI", 10);
            txtTimKiem.Margin =
                new Padding(10, 5, 6, 5);

            txtTimKiem.PlaceholderText =
                "Học viên, lớp, khóa học...";

            thanhCongCu.Controls.AddRange(new Control[]
            {
                btnThu,
                btnThanhToanQR,
                btnLamMoi,
                txtTimKiem,
                btnTim,
                btnTatCa
            });

            layout.Controls.Add(thanhCongCu, 0, 2);

            // =====================================
            // 4. CHỈ GIỮ BẢNG LỊCH SỬ
            // =====================================

            var pnlLichSu = TaoPanelBang(
                "LỊCH SỬ THANH TOÁN",
                lblLichSu,
                dgvLichSu);

            layout.Controls.Add(pnlLichSu, 0, 3);

            // KHÔNG TẠO dgvCongNo NỮA.
            // KHÔNG TẠO PANEL CÔNG NỢ NỮA.

            ThemCot(
                dgvLichSu,
                "MaTT",
                "Mã TT",
                75);

            ThemCot(
                dgvLichSu,
                "NgayThanhToan",
                "Lần thanh toán cuối",
                145,
                "dd/MM/yyyy HH:mm");

            ThemCot(
                dgvLichSu,
                "TenHocVien",
                "Học viên",
                140);

            ThemCot(
                dgvLichSu,
                "TenLop",
                "Lớp",
                100);

            ThemCot(
                dgvLichSu,
                "SoTien",
                "Tổng đã đóng",
                160);

            ThemCot(
                dgvLichSu,
                "HinhThuc",
                "Hình thức",
                155);

            ThemCot(
                dgvLichSu,
                "GhiChu",
                "Ghi chú",
                230);

            // =====================================
            // 5. SỰ KIỆN
            // =====================================

            cboDangKy.SelectedIndexChanged += (_, _) =>
            {
                if (!dangNap)
                    XuLyAnToan(ChonDangKy);
            };

            cboHinhThuc.SelectedIndexChanged += (_, _) =>
                XuLyAnToan(CapNhatPhuongThuc);

            nudSoTien.ValueChanged += (_, _) =>
            {
                if (!dangNap)
                    XuLyAnToan(CapNhatGhiChuHocPhi);
            };

            btnThu.Click += (_, _) =>
                XuLyAnToan(GhiNhanThanhToan);

            btnThanhToanQR.Click += (_, _) =>
                XuLyAnToan(MoThanhToanQR);

            btnLamMoi.Click += (_, _) =>
                XuLyAnToan(() =>
                {
                    txtTimKiem.Clear();
                    TaiDuLieu();
                });

            btnTim.Click += (_, _) =>
                XuLyAnToan(TimKiemLichSu);

            btnTatCa.Click += (_, _) =>
                XuLyAnToan(() =>
                {
                    txtTimKiem.Clear();
                    maDKDangXem = null;
                    NapLichSu(null);
                });

            txtTimKiem.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter)
                    return;

                e.SuppressKeyPress = true;
                XuLyAnToan(TimKiemLichSu);
            };

            // Nhấp vào lịch sử sẽ chọn đăng ký
            // tương ứng ở ComboBox phía trên.
            dgvLichSu.CellClick += (_, e) =>
            {
                if (e.RowIndex < 0)
                    return;

                if (dgvLichSu.Rows[e.RowIndex]
                    .DataBoundItem is not DongLichSuTongHop dong)
                    return;

                if (cboDangKy.SelectedItem
                    is LuaChonCongNo dk &&
                    dk.MaDK == dong.MaDK)
                    return;

                cboDangKy.SelectedValue = dong.MaDK;
            };

            CapNhatPhuongThuc();
            XoaThongTinThu();
        }

        // =========================================
        // HÀM CẤU HÌNH NÚT CÓ SẴN
        // =========================================

        private static void CauHinhNut(
            Button btn,
            string ten,
            Color mau,
            int rong)
        {
            btn.Text = ten;
            btn.Width = rong;
            btn.Height = 40;
            btn.BackColor = mau;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            btn.Font = new Font(
                "Segoe UI", 10, FontStyle.Bold);

            btn.Cursor = Cursors.Hand;
            btn.Margin = new Padding(0, 0, 8, 5);
        }

        // =========================================
        // PANEL LỊCH SỬ
        // =========================================

        private static Panel TaoPanelBang(
            string tieuDe,
            Label tong,
            DataGridView grid)
        {
            CauHinhGrid(grid);

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            panel.Controls.Add(grid);

            tong.Dock = DockStyle.Bottom;
            tong.Height = 30;
            tong.TextAlign =
                ContentAlignment.MiddleRight;
            tong.Font = new Font("Segoe UI", 10);

            panel.Controls.Add(tong);

            panel.Controls.Add(new Label
            {
                Text = tieuDe,
                Dock = DockStyle.Top,
                Height = 38,
                Font = new Font(
                    "Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });

            return panel;
        }

        // =========================================
        // CẤU HÌNH DATAGRIDVIEW
        // =========================================

        private static void CauHinhGrid(DataGridView grid)
        {
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;

            grid.AutoGenerateColumns = false;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;

            grid.MultiSelect = false;
            grid.RowHeadersVisible = false;

            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            grid.EnableHeadersVisualStyles = false;

            grid.ColumnHeadersHeight = 42;
            grid.RowTemplate.Height = 35;

            grid.DefaultCellStyle.Font =
                new Font("Segoe UI", 10);

            grid.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(23, 52, 83);

            grid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);

            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(210, 230, 250);

            grid.DefaultCellStyle.SelectionForeColor =
                Color.Black;

            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(245, 248, 252);
        }

        private static void ThemCot(
            DataGridView grid,
            string ten,
            string nhan,
            float doRong,
            string? dinhDang = null)
        {
            var cot = new DataGridViewTextBoxColumn
            {
                Name = ten,
                DataPropertyName = ten,
                HeaderText = nhan,
                FillWeight = doRong,
                ReadOnly = true
            };

            if (dinhDang != null)
                cot.DefaultCellStyle.Format = dinhDang;

            grid.Columns.Add(cot);
        }

        // =========================================
        // TẢI DỮ LIỆU
        // =========================================

        private void TaiDuLieu(int? maDKChon = null)
        {
            dangNap = true;

            try
            {
                cboDangKy.DataSource = null;

                cboDangKy.DisplayMember =
                    nameof(LuaChonCongNo.HienThi);

                cboDangKy.ValueMember =
                    nameof(LuaChonCongNo.MaDK);

                cboDangKy.DataSource = bll.LayDangKy();
                cboDangKy.SelectedIndex = -1;
            }
            finally
            {
                dangNap = false;
            }

            if (maDKChon.HasValue)
            {
                cboDangKy.SelectedValue =
                    maDKChon.Value;

                if (cboDangKy.SelectedItem
                    is LuaChonCongNo dk &&
                    dk.MaDK == maDKChon.Value)
                {
                    ChonDangKy();
                }
                else
                {
                    maDKDangXem = null;
                    XoaThongTinThu();
                    NapLichSu(null);
                }
            }
            else
            {
                maDKDangXem = null;
                XoaThongTinThu();
                NapLichSu(null);
            }
        }

        // =========================================
        // CHỌN ĐĂNG KÝ HỌC
        // =========================================

        private void ChonDangKy()
        {
            if (cboDangKy.SelectedItem
                is not LuaChonCongNo dk)
            {
                maDKDangXem = null;
                XoaThongTinThu();
                NapLichSu(null);
                return;
            }

            maDKDangXem = dk.MaDK;

            txtHocPhi.Text =
                $"{dk.HocPhiPhaiDong:N0}";

            // Đây là khoản thực thu, không bao gồm QR DEMO.
            txtDaDong.Text =
                $"{dk.DaThanhToan:N0}";

            txtConNo.Text =
                $"{dk.ConNo:N0}";

            txtTrangThai.Text = dk.TrangThai switch
            {
                "DangHoc" => "Đang học",
                "HoanThanh" => "Hoàn thành",
                "DaHuy" => "Đã hủy",
                _ => dk.TrangThai
            };

            dangNap = true;

            try
            {
                nudSoTien.Value = 0;
                txtGhiChu.Clear();
            }
            finally
            {
                dangNap = false;
            }

            NapLichSu(dk.MaDK);
        }

        private void XoaThongTinThu()
        {
            dangNap = true;

            try
            {
                txtHocPhi.Clear();
                txtDaDong.Clear();
                txtConNo.Clear();
                txtTrangThai.Clear();

                nudSoTien.Value = 0;
                txtGhiChu.Clear();
            }
            finally
            {
                dangNap = false;
            }
        }

        // =========================================
        // LỊCH SỬ TỔNG HỢP THEO MÃ ĐĂNG KÝ
        // =========================================

        private void NapLichSu(int? maDK)
        {
            var lichSuThuc = bll.LayLichSu(maDK);
            var lichSuDemo = bll.LayLichSuDemo(maDK);

            // Dùng danh sách đăng ký để lấy
            // học viên, lớp và học phí gốc.
            var traCuu = bll.LayCongNo()
                .ToDictionary(x => x.MaDK);

            var ketQua = new List<DongLichSuTongHop>();

            var cacMaDK = lichSuThuc
                .Select(x => x.MaDK)
                .Concat(lichSuDemo.Select(x => x.MaDK))
                .Distinct()
                .ToList();

            foreach (int ma in cacMaDK)
            {
                var cacLanThuc = lichSuThuc
                    .Where(x => x.MaDK == ma)
                    .ToList();

                var cacLanDemo = lichSuDemo
                    .Where(x => x.MaDK == ma)
                    .ToList();

                decimal tongThuc = cacLanThuc
                    .Sum(x => x.SoTien);

                decimal tongDemo = cacLanDemo
                    .Sum(x => x.SoTien);

                var lanThucCuoi = cacLanThuc
                    .OrderByDescending(x => x.NgayThanhToan)
                    .ThenByDescending(x => x.MaTT)
                    .FirstOrDefault();

                var lanDemoCuoi = cacLanDemo
                    .OrderByDescending(x => x.NgayXacNhan)
                    .ThenByDescending(x => x.MaDemo)
                    .FirstOrDefault();

                DateTime ngayThuc =
                    lanThucCuoi?.NgayThanhToan
                    ?? DateTime.MinValue;

                DateTime ngayDemo =
                    lanDemoCuoi?.NgayXacNhan
                    ?? DateTime.MinValue;

                DateTime ngayCuoi =
                    ngayThuc > ngayDemo
                        ? ngayThuc
                        : ngayDemo;

                traCuu.TryGetValue(ma, out var dk);

                string tenHocVien =
                    dk?.TenHocVien
                    ?? lanThucCuoi?.TenHocVien
                    ?? "";

                string tenLop =
                    dk?.TenLop
                    ?? lanThucCuoi?.TenLop
                    ?? "";

                decimal hocPhi =
                    dk?.HocPhiPhaiDong ?? 0m;

                decimal conNoThuc =
                    Math.Max(0m, hocPhi - tongThuc);

                decimal conNoDemo =
                    Math.Max(
                        0m,
                        hocPhi - tongThuc - tongDemo);

                string maHienThi;
                string soTienHienThi;
                string hinhThuc;
                string ghiChu;

                // =================================
                // TRƯỜNG HỢP CÓ CẢ THẬT VÀ DEMO
                // =================================

                if (tongThuc > 0m && tongDemo > 0m)
                {
                    maHienThi = $"ĐK{ma}";

                    soTienHienThi =
                        $"Thực: {tongThuc:N0} | " +
                        $"DEMO: {tongDemo:N0}";

                    hinhThuc = "Kết hợp / QR (DEMO)";

                    ghiChu =
                        $"Thực còn: {conNoThuc:N0}đ; " +
                        $"DEMO dự kiến còn: {conNoDemo:N0}đ";
                }

                // =================================
                // CHỈ CÓ GIAO DỊCH QR DEMO
                // =================================

                else if (tongDemo > 0m)
                {
                    maHienThi =
                        $"D{lanDemoCuoi?.MaDemo}";

                    soTienHienThi =
                        $"{tongDemo:N0}";

                    hinhThuc =
                        "Chuyển khoản QR (DEMO)";

                    ghiChu = conNoDemo > 0m
                        ? $"Đã nộp DEMO {tongDemo:N0}đ, " +
                          $"còn thiếu {conNoDemo:N0}đ"
                        : "Đã thanh toán đủ học phí (DEMO)";
                }

                // =================================
                // CHỈ CÓ THANH TOÁN THẬT
                // =================================

                else
                {
                    maHienThi =
                        lanThucCuoi?.MaTT.ToString()
                        ?? $"ĐK{ma}";

                    soTienHienThi =
                        $"{tongThuc:N0}";

                    var cacHinhThuc = cacLanThuc
                        .Select(x => x.TenHinhThuc)
                        .Distinct()
                        .ToList();

                    hinhThuc = cacHinhThuc.Count == 1
                        ? cacHinhThuc[0]
                        : "Nhiều phương thức";

                    ghiChu = conNoThuc > 0m
                        ? $"Đã nộp {tongThuc:N0}đ, " +
                          $"còn thiếu {conNoThuc:N0}đ"
                        : "Đã thanh toán đủ học phí";
                }

                ketQua.Add(new DongLichSuTongHop
                {
                    MaDK = ma,
                    MaTT = maHienThi,
                    NgayThanhToan = ngayCuoi,
                    TenHocVien = tenHocVien,
                    TenLop = tenLop,
                    SoTien = soTienHienThi,
                    HinhThuc = hinhThuc,
                    GhiChu = ghiChu,
                    CoDemo = tongDemo > 0m
                });
            }

            // TÌM KIẾM NGAY TRONG LỊCH SỬ
            string tuKhoa = txtTimKiem.Text.Trim();

            if (tuKhoa.Length > 0)
            {
                ketQua = ketQua.Where(x =>
                    x.TenHocVien.Contains(
                        tuKhoa,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    x.TenLop.Contains(
                        tuKhoa,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    x.MaTT.Contains(
                        tuKhoa,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    (traCuu.TryGetValue(x.MaDK, out var dk)
                     && dk.TenKhoaHoc.Contains(
                         tuKhoa,
                         StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            ketQua = ketQua
                .OrderByDescending(x => x.NgayThanhToan)
                .ThenByDescending(x => x.MaDK)
                .ToList();

            dgvLichSu.DataSource = null;
            dgvLichSu.DataSource = ketQua;

            // Phân biệt DEMO bằng màu nền,
            // không tạo thêm cột "Loại".
            foreach (DataGridViewRow row in dgvLichSu.Rows)
            {
                if (row.DataBoundItem
                    is DongLichSuTongHop dong &&
                    dong.CoDemo)
                {
                    row.DefaultCellStyle.BackColor =
                        Color.FromArgb(255, 248, 230);
                }
            }

            dgvLichSu.ClearSelection();

            lblLichSu.Text =
                $"Tổng số: {ketQua.Count} đăng ký đã thanh toán";
        }

        private void TimKiemLichSu()
        {
            NapLichSu(maDKDangXem);
        }

        // =========================================
        // CẬP NHẬT GHI CHÚ
        // =========================================

        private void CapNhatGhiChuHocPhi()
        {
            if (cboDangKy.SelectedItem
                is not LuaChonCongNo dk)
            {
                txtGhiChu.Clear();
                return;
            }

            decimal soTien = nudSoTien.Value;

            if (soTien <= 0m)
            {
                txtGhiChu.Clear();
                return;
            }

            // CHUYỂN KHOẢN QR
            if (cboHinhThuc.SelectedItem?.ToString()
                == "Chuyển khoản")
            {
                decimal daDemo = bll.LayTongDemo(dk.MaDK);

                decimal conNoDemo = Math.Max(
                    0m, dk.ConNo - daDemo);

                decimal conThieu = Math.Max(
                    0m, conNoDemo - soTien);

                txtGhiChu.Text = conThieu > 0m
                    ? $"QR DEMO dự kiến nộp {soTien:N0}đ, " +
                      $"còn thiếu {conThieu:N0}đ"
                    : "Dự kiến thanh toán đủ học phí (DEMO)";

                return;
            }

            // TIỀN MẶT / THẺ
            decimal conNoThuc = Math.Max(
                0m, dk.ConNo - soTien);

            txtGhiChu.Text = conNoThuc > 0m
                ? $"Dự kiến nộp {soTien:N0}đ, " +
                  $"còn thiếu {conNoThuc:N0}đ"
                : "Dự kiến thanh toán đủ học phí";
        }

        // =========================================
        // THAY ĐỔI PHƯƠNG THỨC THANH TOÁN
        // =========================================

        private void CapNhatPhuongThuc()
        {
            bool laChuyenKhoan =
                cboHinhThuc.SelectedItem?.ToString()
                == "Chuyển khoản";

            btnThanhToanQR.Visible = laChuyenKhoan;
            btnThu.Visible = !laChuyenKhoan;

            if (!dangNap)
                CapNhatGhiChuHocPhi();
        }

        // =========================================
        // THANH TOÁN QR MÔ PHỎNG
        // =========================================

        private void MoThanhToanQR()
        {
            if (cboHinhThuc.SelectedItem?.ToString()
                != "Chuyển khoản")
            {
                throw new InvalidOperationException(
                    "Vui lòng chọn phương thức Chuyển khoản.");
            }

            if (cboDangKy.SelectedItem
                is not LuaChonCongNo dk)
            {
                throw new ArgumentException(
                    "Vui lòng chọn đăng ký học.");
            }

            var dong = bll.LayCongNo()
                .FirstOrDefault(x => x.MaDK == dk.MaDK);

            if (dong == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy đăng ký học.");
            }

            if (dong.TrangThai != "DangHoc" &&
                dong.TrangThai != "HoanThanh")
            {
                throw new InvalidOperationException(
                    "Đăng ký không hợp lệ để thanh toán.");
            }

            decimal daDemo =
                bll.LayTongDemo(dk.MaDK);

            decimal conNoDemo =
                dong.ConNo - daDemo;

            if (conNoDemo <= 0m)
            {
                throw new InvalidOperationException(
                    "Đăng ký đã thanh toán đủ " +
                    "trong chế độ DEMO.");
            }

            decimal soTienGoiY = nudSoTien.Value;

            if (soTienGoiY > conNoDemo)
                soTienGoiY = conNoDemo;

            // Form QR constructor 6 tham số.
            using var form = new frmThanhToanQRMoPhong(
                dong.MaDK,
                dong.TenHocVien,
                dong.TenLop,
                dong.TenKhoaHoc,
                conNoDemo,
                soTienGoiY);

            if (form.ShowDialog(this) != DialogResult.OK)
                return;

            decimal soTien = form.SoTienThanhToan;

            if (soTien <= 0m || soTien > conNoDemo)
            {
                throw new InvalidOperationException(
                    "Số tiền thanh toán QR không hợp lệ.");
            }

            string maGiaoDich =
                form.MaGiaoDich?.Trim() ?? "";

            if (!maGiaoDich.StartsWith(
                    "DEMO-HP-",
                    StringComparison.Ordinal) ||
                maGiaoDich.Length > 50)
            {
                throw new InvalidOperationException(
                    "Mã giao dịch QR DEMO không hợp lệ.");
            }

            decimal conThieu =
                conNoDemo - soTien;

            string ghiChu = conThieu > 0m
                ? $"Đã nộp QR DEMO {soTien:N0}đ, " +
                  $"còn thiếu {conThieu:N0}đ"
                : "Đã thanh toán đủ học phí (DEMO)";

            // =====================================
            // LƯU SQL SERVER
            // =====================================

            bll.LuuThanhToanDemo(
                dong.MaDK,
                soTien,
                maGiaoDich,
                ghiChu);

            // =====================================
            // CẬP NHẬT GIAO DIỆN
            // =====================================

            TaiDuLieu(dong.MaDK);

            dangNap = true;

            try
            {
                nudSoTien.Value = soTien;
                txtGhiChu.Text = ghiChu;
            }
            finally
            {
                dangNap = false;
            }

            NapLichSu(dong.MaDK);

            MessageBox.Show(
                "Đã lưu giao dịch QR DEMO vào SQL Server.\n\n" +
                $"Học viên: {dong.TenHocVien}\n" +
                $"Số tiền vừa nộp DEMO: {soTien:N0}đ\n" +
                $"Tổng đã nộp DEMO: {daDemo + soTien:N0}đ\n" +
                $"Còn thiếu DEMO: {conThieu:N0}đ\n\n" +
                "Dữ liệu được tổng hợp trong Lịch sử thanh toán.",
                "Lưu giao dịch thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =========================================
        // THU TIỀN MẶT / THẺ
        // =========================================

        private void GhiNhanThanhToan()
        {
            if (cboHinhThuc.SelectedItem?.ToString()
                == "Chuyển khoản")
            {
                throw new InvalidOperationException(
                    "Với chuyển khoản, hãy dùng Thanh toán QR.");
            }

            if (cboDangKy.SelectedItem
                is not LuaChonCongNo dk)
            {
                throw new ArgumentException(
                    "Vui lòng chọn đăng ký học.");
            }

            string phuongThuc =
                cboHinhThuc.SelectedIndex switch
                {
                    0 => "TienMat",
                    2 => "The",
                    _ => ""
                };

            if (string.IsNullOrWhiteSpace(phuongThuc))
            {
                throw new ArgumentException(
                    "Phương thức thanh toán không hợp lệ.");
            }

            decimal soTien = nudSoTien.Value;

            if (soTien <= 0m)
            {
                throw new ArgumentException(
                    "Số tiền phải lớn hơn 0.");
            }

            if (soTien > dk.ConNo)
            {
                throw new ArgumentException(
                    $"Số tiền vượt công nợ {dk.ConNo:N0}đ.");
            }

            decimal conThieu =
                dk.ConNo - soTien;

            string ghiChuTuDong = conThieu > 0m
                ? $"Đã nộp {soTien:N0}đ, " +
                  $"còn thiếu {conThieu:N0}đ"
                : "Đã thanh toán đủ học phí";

            string ghiChu = txtGhiChu.Text.Trim();

            if (string.IsNullOrWhiteSpace(ghiChu) ||
                ghiChu.StartsWith(
                    "Dự kiến",
                    StringComparison.OrdinalIgnoreCase) ||
                ghiChu.Contains(
                    "DEMO",
                    StringComparison.OrdinalIgnoreCase))
            {
                ghiChu = ghiChuTuDong;
            }

            if (MessageBox.Show(
                $"Xác nhận thu {soTien:N0}đ " +
                $"cho đăng ký {dk.MaDK}?\n\n" +
                $"Ghi chú: {ghiChu}",
                "Xác nhận thu học phí",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)
                != DialogResult.Yes)
            {
                return;
            }

            int maTT = bll.GhiNhan(
                dk.MaDK,
                soTien,
                phuongThuc,
                ghiChu);

            TaiDuLieu(dk.MaDK);

            MessageBox.Show(
                $"Ghi nhận thành công!\n" +
                $"Mã thanh toán: {maTT}",
                "Thành công",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =========================================
        // XỬ LÝ LỖI
        // =========================================

        private static void XuLyAnToan(Action thaoTac)
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
                Debug.WriteLine(ex);

                MessageBox.Show(
                    ex.Message,
                    "Không thể xử lý học phí",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
