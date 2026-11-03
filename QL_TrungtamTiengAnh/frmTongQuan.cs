
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTiengAnh.GUI
{
    public sealed partial class frmTongQuan : Form
    {
        // =========================================
        // 1. MÀU SẮC
        // =========================================

        private static readonly Color Navy =
            Color.FromArgb(23, 52, 83);

        private static readonly Color Blue =
            Color.FromArgb(40, 110, 190);

        private static readonly Color Nen =
            Color.FromArgb(245, 248, 252);

        private static readonly Color Xam =
            Color.FromArgb(105, 121, 140);

        private readonly PhienDangNhap phien;
        private readonly TongQuanDAL dal = new();

        // =========================================
        // 2. CONTROL
        // =========================================

        private readonly DateTimePicker dtpNgay = new();

        private readonly Label lblXinChao = new();
        private readonly Label lblBuoi = new();
        private readonly Label lblDa = new();
        private readonly Label lblChua = new();

        private readonly Label lblNgayDuocChon = new();

        private readonly Label lblRong = new();
        private readonly Label lblSapToiRong = new();
        private readonly Label lblChuYRong = new();

        private readonly DataGridView dgvBuoi = new();
        private readonly DataGridView dgvSapToi = new();
        private readonly DataGridView dgvChuY = new();

        private readonly Panel pnlCuon = new();
        private readonly TableLayoutPanel body = new();

        private bool dangTaiDuLieu;

        // =========================================
        // 3. CONSTRUCTOR
        // =========================================

        public frmTongQuan(PhienDangNhap phienDangNhap)
        {
            phien = phienDangNhap
                ?? throw new ArgumentNullException(
                    nameof(phienDangNhap));

            Text = "Tổng quan trung tâm";

            BackColor = Nen;
            Font = new Font("Segoe UI", 10);
            AutoScaleMode = AutoScaleMode.Dpi;

            MinimumSize = Size.Empty;

            TaoGiaoDien();

            Shown += (_, _) => TaiDuLieu();
        }

        // =========================================
        // 4. TẠO GIAO DIỆN CHÍNH
        // =========================================

        private void TaoGiaoDien()
        {
            SuspendLayout();

            Controls.Clear();

            // Một thanh cuộn cho toàn bộ trang.
            pnlCuon.Dock = DockStyle.Fill;
            pnlCuon.AutoScroll = true;
            pnlCuon.BackColor = Nen;

            Controls.Add(pnlCuon);

            // Không dùng AutoSize = true.
            // Chiều rộng được tính theo vùng chứa.
            body.Dock = DockStyle.None;
            body.Location = Point.Empty;

            body.AutoSize = false;
            body.ColumnCount = 1;
            body.RowCount = 6;

            body.Padding = new Padding(
                20, 16, 20, 20);

            body.Margin = Padding.Empty;
            body.BackColor = Nen;

            body.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            // Chiều cao từng khu vực.
            body.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 82));

            body.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 65));

            body.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 120));

            body.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 290));

            body.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 290));

            body.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 180));

            body.Height = 1060;

            pnlCuon.Controls.Add(body);

            // Tự điều chỉnh chiều ngang khi
            // Form được mở hoặc thay đổi kích thước.
            pnlCuon.Resize += (_, _) =>
                CanChinhChieuRong();

            // =====================================
            // HÀNG 1: TIÊU ĐỀ
            // =====================================

            body.Controls.Add(
                TaoHeader(), 0, 0);

            // =====================================
            // HÀNG 2: CHỌN NGÀY
            // =====================================

            body.Controls.Add(
                TaoBoLoc(), 0, 1);

            // =====================================
            // HÀNG 3: THỐNG KÊ
            // =====================================

            body.Controls.Add(
                TaoThongKe(), 0, 2);

            // =====================================
            // HÀNG 4: LỊCH TRONG NGÀY
            // =====================================

            CauHinhBang(dgvBuoi);

            lblRong.Text =
                "Không có lịch học trong ngày được chọn.";

            body.Controls.Add(
                TaoKhungBang(
                    lblNgayDuocChon,
                    dgvBuoi,
                    lblRong),
                0, 3);

            // =====================================
            // HÀNG 5: LỊCH 7 NGÀY
            // =====================================

            CauHinhBang(dgvSapToi);

            lblSapToiRong.Text =
                "Không có lịch học trong 7 ngày tiếp theo.";

            body.Controls.Add(
                TaoKhungBang(
                    "LỊCH HỌC 7 NGÀY TIẾP THEO",
                    dgvSapToi,
                    lblSapToiRong),
                0, 4);

            // =====================================
            // HÀNG 6: CÔNG VIỆC CẦN CHÚ Ý
            // =====================================

            CauHinhBang(dgvChuY);

            lblChuYRong.Text =
                "Không có công việc cần chú ý.";

            body.Controls.Add(
                TaoKhungBang(
                    "CÔNG VIỆC CẦN CHÚ Ý",
                    dgvChuY,
                    lblChuYRong),
                0, 5);

            ResumeLayout(true);

            CanChinhChieuRong();
        }

        // =========================================
        // 5. CHỐNG LỖI CO GIAO DIỆN
        // =========================================

        private void CanChinhChieuRong()
        {
            if (pnlCuon.IsDisposed)
                return;

            // Trừ bề rộng thanh cuộn dọc.
            int rong = pnlCuon.ClientSize.Width
                - SystemInformation.VerticalScrollBarWidth
                - 2;

            // Không cho nội dung co thành cột hẹp.
            body.Width = Math.Max(650, rong);
        }

        // =========================================
        // 6. TIÊU ĐỀ
        // =========================================

        private Control TaoHeader()
        {
            var card = TaoCard();

            card.Margin = new Padding(0, 0, 0, 10);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(18, 9, 12, 7)
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 38));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100));

            var lblTieuDe = new Label
            {
                Text = "TỔNG QUAN TRUNG TÂM",
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = new Font(
                    "Segoe UI", 19, FontStyle.Bold),
                ForeColor = Navy,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            lblXinChao.Dock = DockStyle.Fill;

            lblXinChao.Font =
                new Font("Segoe UI", 9.5f);

            lblXinChao.ForeColor = Xam;

            lblXinChao.TextAlign =
                ContentAlignment.MiddleLeft;

            layout.Controls.Add(lblTieuDe, 0, 0);
            layout.Controls.Add(lblXinChao, 0, 1);

            card.Controls.Add(layout);

            return card;
        }

        // =========================================
        // 7. BỘ LỌC NGÀY
        // =========================================

        private Control TaoBoLoc()
        {
            var card = TaoCard();

            card.Margin = new Padding(0, 0, 0, 10);

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection =
                    FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(15, 8, 8, 5)
            };

            var lbl = new Label
            {
                Text = "Xem lịch ngày:",
                AutoSize = true,
                Font = new Font(
                    "Segoe UI", 10, FontStyle.Bold),
                ForeColor = Navy,
                Margin = new Padding(0, 9, 12, 0)
            };

            dtpNgay.Format =
                DateTimePickerFormat.Custom;

            dtpNgay.CustomFormat = "dd/MM/yyyy";

            dtpNgay.Width = 150;
            dtpNgay.Value = DateTime.Today;

            dtpNgay.Margin =
                new Padding(0, 3, 10, 0);

            var btnHomNay = TaoNut(
                "Hôm nay",
                100,
                Blue);

            var btnLamMoi = TaoNut(
                "Làm mới dữ liệu",
                160,
                Color.FromArgb(26, 135, 95));

            flow.Controls.Add(lbl);
            flow.Controls.Add(dtpNgay);
            flow.Controls.Add(btnHomNay);
            flow.Controls.Add(btnLamMoi);

            dtpNgay.ValueChanged += (_, _) =>
                TaiDuLieu();

            btnHomNay.Click += (_, _) =>
            {
                if (dtpNgay.Value.Date != DateTime.Today)
                    dtpNgay.Value = DateTime.Today;
                else
                    TaiDuLieu();
            };

            btnLamMoi.Click += (_, _) =>
                TaiDuLieu();

            card.Controls.Add(flow);

            return card;
        }

        // =========================================
        // 8. THỐNG KÊ
        // =========================================

        private Control TaoThongKe()
        {
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 10)
            };

            for (int i = 0; i < 3; i++)
            {
                layout.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent, 100f / 3));
            }

            layout.Controls.Add(
                TaoThe(
                    "LỊCH HỌC TRONG NGÀY",
                    lblBuoi,
                    Blue),
                0, 0);

            layout.Controls.Add(
                TaoThe(
                    "ĐÃ HOÀN TẤT ĐIỂM DANH",
                    lblDa,
                    Color.FromArgb(22, 145, 100)),
                1, 0);

            layout.Controls.Add(
                TaoThe(
                    "CHƯA HOÀN TẤT",
                    lblChua,
                    Color.FromArgb(210, 90, 60)),
                2, 0);

            return layout;
        }

        private static Control TaoThe(
            string ten,
            Label giaTri,
            Color mau)
        {
            var card = TaoCard();

            card.Margin = new Padding(0, 0, 10, 0);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(14, 14, 10, 10)
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute, 7));

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 33));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100));

            var vachMau = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = mau,
                Margin = new Padding(0, 0, 0, 0)
            };

            layout.Controls.Add(vachMau, 0, 0);

            layout.SetRowSpan(vachMau, 2);

            layout.Controls.Add(new Label
            {
                Text = ten,
                Dock = DockStyle.Fill,
                Font = new Font(
                    "Segoe UI", 9, FontStyle.Bold),
                ForeColor = Xam,
                Padding = new Padding(12, 0, 0, 0),
                TextAlign =
                    ContentAlignment.MiddleLeft
            }, 1, 0);

            giaTri.Dock = DockStyle.Fill;

            giaTri.Text = "0";

            giaTri.Font = new Font(
                "Segoe UI", 25, FontStyle.Bold);

            giaTri.ForeColor = Navy;

            giaTri.Padding =
                new Padding(12, 0, 0, 0);

            giaTri.TextAlign =
                ContentAlignment.MiddleLeft;

            layout.Controls.Add(giaTri, 1, 1);

            card.Controls.Add(layout);

            return card;
        }

        // =========================================
        // 9. KHUNG BẢNG DỮ LIỆU
        // =========================================

        private static Control TaoKhungBang(
            string tieuDe,
            DataGridView grid,
            Label lblRong)
        {
            var lbl = new Label
            {
                Text = tieuDe,
                Dock = DockStyle.Fill,
                Font = new Font(
                    "Segoe UI", 12, FontStyle.Bold),
                ForeColor = Navy,
                TextAlign =
                    ContentAlignment.MiddleLeft
            };

            return TaoKhungBang(lbl, grid, lblRong);
        }

        private static Control TaoKhungBang(
            Label lblTieuDe,
            DataGridView grid,
            Label lblRong)
        {
            var card = TaoCard();

            card.Margin = new Padding(0, 0, 0, 12);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(15, 10, 15, 12)
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 36));

            layout.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100));

            lblTieuDe.Dock = DockStyle.Fill;

            lblTieuDe.Font = new Font(
                "Segoe UI", 12, FontStyle.Bold);

            lblTieuDe.ForeColor = Navy;

            lblTieuDe.TextAlign =
                ContentAlignment.MiddleLeft;

            layout.Controls.Add(lblTieuDe, 0, 0);

            var pnlBang = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            grid.Dock = DockStyle.Fill;

            pnlBang.Controls.Add(grid);

            // Thông báo trống chỉ che phần dữ liệu,
            // không che dòng tiêu đề cột.
            lblRong.Dock = DockStyle.None;
            lblRong.AutoSize = false;
            lblRong.Visible = false;
            lblRong.BackColor = Color.White;
            lblRong.ForeColor = Xam;

            lblRong.Font = new Font(
                "Segoe UI", 10, FontStyle.Italic);

            lblRong.TextAlign =
                ContentAlignment.MiddleCenter;

            pnlBang.Controls.Add(lblRong);

            void CanChinhThongBao()
            {
                int dau = grid.ColumnHeadersHeight;

                lblRong.Bounds = new Rectangle(
                    0,
                    dau,
                    Math.Max(0, pnlBang.ClientSize.Width),
                    Math.Max(
                        0,
                        pnlBang.ClientSize.Height - dau));

                lblRong.BringToFront();
            }

            pnlBang.Resize += (_, _) =>
                CanChinhThongBao();

            grid.ColumnHeadersHeightChanged += (_, _) =>
                CanChinhThongBao();

            layout.Controls.Add(pnlBang, 0, 1);

            card.Controls.Add(layout);

            return card;
        }

        // =========================================
        // 10. CARD VÀ BUTTON
        // =========================================

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
                if (card.Width <= 1 || card.Height <= 1)
                    return;

                using var pen = new Pen(
                    Color.FromArgb(224, 232, 241));

                e.Graphics.DrawRectangle(
                    pen,
                    0,
                    0,
                    card.Width - 1,
                    card.Height - 1);
            };

            return card;
        }

        private static Button TaoNut(
            string ten,
            int rong,
            Color mau)
        {
            var nut = new Button
            {
                Text = ten,
                Width = rong,
                Height = 35,
                BackColor = mau,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font(
                    "Segoe UI", 9, FontStyle.Bold),
                Margin = new Padding(0, 3, 8, 0)
            };

            nut.FlatAppearance.BorderSize = 0;

            return nut;
        }

        // =========================================
        // 11. CẤU HÌNH BẢNG
        // =========================================

        private static void CauHinhBang(DataGridView grid)
        {
            grid.Dock = DockStyle.Fill;

            grid.ReadOnly = true;

            grid.AutoGenerateColumns = true;

            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;

            grid.RowHeadersVisible = false;
            grid.MultiSelect = false;

            grid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            grid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;

            grid.GridColor =
                Color.FromArgb(230, 235, 243);

            grid.CellBorderStyle =
                DataGridViewCellBorderStyle.SingleHorizontal;

            grid.EnableHeadersVisualStyles = false;

            grid.ColumnHeadersHeight = 39;
            grid.RowTemplate.Height = 33;

            grid.ColumnHeadersDefaultCellStyle.BackColor =
                Navy;

            grid.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            grid.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI", 9.5f, FontStyle.Bold);

            grid.DefaultCellStyle.Font =
                new Font("Segoe UI", 9.5f);

            grid.DefaultCellStyle.ForeColor =
                Color.FromArgb(42, 55, 75);

            grid.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(216, 231, 249);

            grid.DefaultCellStyle.SelectionForeColor =
                Color.Black;

            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(248, 250, 253);

            grid.DataBindingComplete += (_, _) =>
            {
                grid.ClearSelection();
            };
        }

        // =========================================
        // 12. LẤY DỮ LIỆU TỪ SQL
        // =========================================

        private void TaiDuLieu()
        {
            if (dangTaiDuLieu)
                return;

            dangTaiDuLieu = true;

            try
            {
                DateTime ngay = dtpNgay.Value.Date;

                var ds = dal.LayDuLieu(
                    phien.MaTK,
                    ngay);

                lblXinChao.Text =
                    $"Xin chào, {phien.TenDangNhap} " +
                    $"· {phien.TenVaiTro}";

                lblNgayDuocChon.Text =
                    $"LỊCH HỌC NGÀY " +
                    $"{ngay:dd/MM/yyyy} " +
                    $"({VietHoaThu(ngay)})";

                lblBuoi.Text =
                    ds.SoBuoiHomNay.ToString();

                lblDa.Text =
                    ds.SoBuoiDaDiemDanh.ToString();

                lblChua.Text =
                    ds.SoBuoiChuaDiemDanh.ToString();

                dgvBuoi.DataSource = null;
                dgvSapToi.DataSource = null;
                dgvChuY.DataSource = null;

                dgvBuoi.DataSource = ds.BuoiHomNay;
                dgvSapToi.DataSource = ds.LichSapToi;
                dgvChuY.DataSource = ds.CanChuY;

                // Đổi tên cột.
                DoiTieuDe(
                    dgvBuoi, "Lop", "Lớp học");

                DoiTieuDe(
                    dgvBuoi, "GiaoVien", "Giáo viên");

                DoiTieuDe(
                    dgvBuoi, "Phong", "Phòng");

                DoiTieuDe(
                    dgvBuoi, "Gio", "Giờ học");

                DoiTieuDe(
                    dgvBuoi, "SiSo", "Sĩ số");

                DoiTieuDe(
                    dgvBuoi, "DiemDanh", "Điểm danh");

                DoiTieuDe(
                    dgvSapToi, "Ngay", "Ngày");

                DoiTieuDe(
                    dgvSapToi, "Lop", "Lớp học");

                DoiTieuDe(
                    dgvSapToi, "GiaoVien", "Giáo viên");

                DoiTieuDe(
                    dgvSapToi, "Phong", "Phòng");

                DoiTieuDe(
                    dgvSapToi, "Gio", "Giờ học");

                DoiTieuDe(
                    dgvSapToi, "DiemDanh", "Điểm danh");

                DoiTieuDe(
                    dgvChuY, "NoiDung", "Cần chú ý");

                DoiTieuDe(
                    dgvChuY, "ChiTiet", "Chi tiết");

                if (dgvChuY.Columns.Contains("ChiTiet"))
                {
                    dgvChuY.Columns["ChiTiet"]!
                        .FillWeight = 180;
                }

                lblRong.Visible =
                    ds.BuoiHomNay.Count == 0;

                lblSapToiRong.Visible =
                    ds.LichSapToi.Count == 0;

                lblChuYRong.Visible =
                    ds.CanChuY.Count == 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Không thể tải Tổng quan",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            finally
            {
                dangTaiDuLieu = false;
            }
        }

        // =========================================
        // 13. HÀM HỖ TRỢ
        // =========================================

        private static void DoiTieuDe(
            DataGridView grid,
            string tenCot,
            string tieuDe)
        {
            if (grid.Columns.Contains(tenCot))
            {
                grid.Columns[tenCot]!.HeaderText = tieuDe;
            }
        }

        private static string VietHoaThu(DateTime ngay)
        {
            string thu = ngay.ToString(
                "dddd",
                CultureInfo.GetCultureInfo("vi-VN"));

            if (string.IsNullOrWhiteSpace(thu))
                return "";

            return char.ToUpper(thu[0]) +
                   thu.Substring(1);
        }
    }
}
