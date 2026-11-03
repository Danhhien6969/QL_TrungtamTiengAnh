
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmQuanLyLichGiaoVien : Form
    {
        private readonly LichGiaoVienBLL bll = new();

        private readonly ComboBox cboGiaoVien = new();
        private readonly DateTimePicker dtpTuan = new();
        private readonly DataGridView dgv = new();

        private readonly Label lblKhoangNgay = new();
        private readonly Label lblSoBuoi = new();
        private readonly Label lblSoLop = new();
        private readonly Label lblCanhBao = new();
        private readonly Label lblTong = new();

        private readonly Button btnTuanTruoc = new();
        private readonly Button btnTuanSau = new();
        private readonly Button btnHomNay = new();
        private readonly Button btnLamMoi = new();

        // Null: Admin xem được tất cả giáo viên.
        // Có mã: Chỉ hiển thị lịch giáo viên đó.
        private readonly int? maGiaoVienCoDinh;

        private bool dangNap;

        public frmQuanLyLichGiaoVien(
            int? maGiaoVienCoDinh = null)
        {
            if (maGiaoVienCoDinh.HasValue &&
                maGiaoVienCoDinh.Value <= 0)
            {
                throw new ArgumentException(
                    "Mã giáo viên không hợp lệ.");
            }

            this.maGiaoVienCoDinh = maGiaoVienCoDinh;

            InitializeComponent();
            TaoGiaoDien();

            Shown += (_, _) => TaiDuLieu();
        }

        // ========================================
        // 1. CẤU HÌNH NÚT
        // ĐÃ SỬA LỖI CauHinhNut
        // ========================================

        private static void CauHinhNut(
            Button btn,
            string noiDung,
            Color mau,
            int rong)
        {
            btn.Text = noiDung;
            btn.Width = rong;
            btn.Height = 37;

            btn.BackColor = mau;
            btn.ForeColor = Color.White;

            btn.Font = new Font(
                "Segoe UI", 10, FontStyle.Bold);

            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            btn.Cursor = Cursors.Hand;
            btn.Margin = new Padding(0, 0, 8, 0);
        }

        // ========================================
        // 2. TẠO GIAO DIỆN
        // ========================================

        private void TaoGiaoDien()
        {
            SuspendLayout();

            Text = maGiaoVienCoDinh.HasValue
                ? "Lịch dạy của tôi"
                : "Quản lý lịch giáo viên";

            ClientSize = new Size(1200, 760);
            MinimumSize = new Size(850, 600);

            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);

            AutoScaleMode = AutoScaleMode.Dpi;

            Controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                ColumnCount = 1,
                RowCount = 5
            };

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 78));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 65));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 95));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 60));

            Controls.Add(layout);

            // ====================================
            // TIÊU ĐỀ
            // ====================================

            var pnlTieuDe = new Panel
            {
                Dock = DockStyle.Fill
            };

            var lblTieuDe = new Label
            {
                Text = maGiaoVienCoDinh.HasValue
                    ? "LỊCH DẠY CỦA TÔI"
                    : "QUẢN LÝ LỊCH GIÁO VIÊN",

                AutoSize = true,
                Location = new Point(4, 3),

                ForeColor = Color.FromArgb(23, 52, 83),

                Font = new Font(
                    "Segoe UI", 21, FontStyle.Bold)
            };

            var lblMoTa = new Label
            {
                Text = "Theo dõi lịch dạy, lớp học, " +
                       "chứng chỉ, phòng và thời gian",

                AutoSize = true,
                Location = new Point(6, 48),
                ForeColor = Color.DimGray
            };

            pnlTieuDe.Controls.Add(lblTieuDe);
            pnlTieuDe.Controls.Add(lblMoTa);

            layout.Controls.Add(pnlTieuDe, 0, 0);

            // ====================================
            // BỘ LỌC GIÁO VIÊN VÀ TUẦN
            // ====================================

            var pnlLoc = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 0)
            };

            var lblGiaoVien = new Label
            {
                Text = "Giáo viên:",
                AutoSize = true,
                Margin = new Padding(0, 9, 8, 0)
            };

            pnlLoc.Controls.Add(lblGiaoVien);

            cboGiaoVien.Width = 220;

            cboGiaoVien.DropDownStyle =
                ComboBoxStyle.DropDownList;

            cboGiaoVien.Margin =
                new Padding(0, 2, 20, 0);

            pnlLoc.Controls.Add(cboGiaoVien);

            var lblChonTuan = new Label
            {
                Text = "Chọn tuần:",
                AutoSize = true,
                Margin = new Padding(0, 9, 8, 0)
            };

            pnlLoc.Controls.Add(lblChonTuan);

            dtpTuan.Format = DateTimePickerFormat.Custom;
            dtpTuan.CustomFormat = "dd/MM/yyyy";
            dtpTuan.Width = 135;
            dtpTuan.Value = DateTime.Today;

            dtpTuan.Margin =
                new Padding(0, 2, 10, 0);

            pnlLoc.Controls.Add(dtpTuan);

            // ====================================
            // 4 NÚT ĐÃ SỬA LỖI
            // ====================================

            CauHinhNut(
                btnTuanTruoc,
                "Tuần trước",
                Color.FromArgb(95, 110, 125),
                118);

            CauHinhNut(
                btnTuanSau,
                "Tuần sau",
                Color.FromArgb(95, 110, 125),
                105);

            CauHinhNut(
                btnHomNay,
                "Hôm nay",
                Color.FromArgb(40, 110, 190),
                100);

            CauHinhNut(
                btnLamMoi,
                "Làm mới",
                Color.FromArgb(30, 140, 95),
                100);

            pnlLoc.Controls.AddRange(new Control[]
            {
                btnTuanTruoc,
                btnTuanSau,
                btnHomNay,
                btnLamMoi
            });

            layout.Controls.Add(pnlLoc, 0, 1);

            // ====================================
            // 3 THẺ THỐNG KÊ
            // ====================================

            var pnlThongKe = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };

            for (int i = 0; i < 3; i++)
            {
                pnlThongKe.ColumnStyles.Add(
                    new ColumnStyle(
                        SizeType.Percent, 100f / 3));
            }

            pnlThongKe.Controls.Add(
                TaoTheThongKe(
                    "KHOẢNG THỜI GIAN",
                    lblKhoangNgay),
                0, 0);

            pnlThongKe.Controls.Add(
                TaoTheThongKe(
                    "SỐ BUỔI DẠY",
                    lblSoBuoi),
                1, 0);

            pnlThongKe.Controls.Add(
                TaoTheThongKe(
                    "SỐ LỚP",
                    lblSoLop),
                2, 0);

            layout.Controls.Add(pnlThongKe, 0, 2);

            // ====================================
            // DANH SÁCH LỊCH DẠY
            // ====================================

            var pnlBang = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            CauHinhBang();

            ThemCot("Ngay", "Ngày", 100);
            ThemCot("Thu", "Thứ", 95);

            // Admin xem được tên giáo viên.
            // Giáo viên xem lịch của mình thì ẩn cột này.
            if (!maGiaoVienCoDinh.HasValue)
            {
                ThemCot(
                    "GiaoVien",
                    "Giáo viên",
                    180);
            }

            ThemCot(
                "LopHoc",
                "Lớp học",
                140);

            ThemCot(
                "ChungChi",
                "Chứng chỉ",
                160);

            ThemCot(
                "PhongHoc",
                "Phòng",
                90);

            ThemCot(
                "GioHoc",
                "Giờ dạy",
                125);

            ThemCot(
                "TrangThai",
                "Trạng thái",
                145);

            // Dùng bảng chứa 3 hàng để tránh
            // DataGridView đè lên tiêu đề và tổng số.
            var bangLich = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3
            };

            bangLich.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            bangLich.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 40));

            bangLich.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            bangLich.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 30));

            bangLich.Controls.Add(new Label
            {
                Text = "DANH SÁCH LỊCH DẠY TRONG TUẦN",
                Dock = DockStyle.Fill,
                Font = new Font(
                    "Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            bangLich.Controls.Add(dgv, 0, 1);

            lblTong.Dock = DockStyle.Fill;
            lblTong.TextAlign =
                ContentAlignment.MiddleRight;

            bangLich.Controls.Add(lblTong, 0, 2);

            pnlBang.Controls.Add(bangLich);

            layout.Controls.Add(pnlBang, 0, 3);

            // ====================================
            // CẢNH BÁO TRÙNG LỊCH
            // ====================================

            lblCanhBao.Dock = DockStyle.Fill;
            lblCanhBao.BackColor = Color.White;
            lblCanhBao.ForeColor =
                Color.FromArgb(160, 70, 20);

            lblCanhBao.TextAlign =
                ContentAlignment.MiddleLeft;

            lblCanhBao.Padding = new Padding(12);
            lblCanhBao.AutoEllipsis = true;

            layout.Controls.Add(lblCanhBao, 0, 4);

            // ====================================
            // ĐĂNG KÝ SỰ KIỆN
            // ====================================

            cboGiaoVien.SelectedIndexChanged += (_, _) =>
            {
                if (!dangNap)
                    NapLich();
            };

            dtpTuan.ValueChanged += (_, _) =>
            {
                if (!dangNap)
                    NapLich();
            };

            btnTuanTruoc.Click += (_, _) =>
            {
                ChuyenTuan(-7);
            };

            btnTuanSau.Click += (_, _) =>
            {
                ChuyenTuan(7);
            };

            btnHomNay.Click += (_, _) =>
            {
                dtpTuan.Value = DateTime.Today;
            };

            btnLamMoi.Click += (_, _) =>
            {
                TaiDuLieu();
            };

            ResumeLayout(true);
        }

        // ========================================
        // 3. TẠO THẺ THỐNG KÊ
        // ========================================

        private static Panel TaoTheThongKe(
            string tieuDe,
            Label giaTri)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12),
                Margin = new Padding(0, 0, 8, 8)
            };

            var bang = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            bang.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            bang.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 26));

            bang.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            bang.Controls.Add(new Label
            {
                Text = tieuDe,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(80, 105, 135),
                Font = new Font(
                    "Segoe UI", 9, FontStyle.Bold)
            }, 0, 0);

            giaTri.Dock = DockStyle.Fill;

            giaTri.Font = new Font(
                "Segoe UI", 16, FontStyle.Bold);

            giaTri.ForeColor =
                Color.FromArgb(23, 52, 83);

            giaTri.TextAlign =
                ContentAlignment.MiddleLeft;

            bang.Controls.Add(giaTri, 0, 1);
            panel.Controls.Add(bang);

            return panel;
        }

        // ========================================
        // 4. CẤU HÌNH DATAGRIDVIEW
        // ========================================

        private void CauHinhBang()
        {
            dgv.Dock = DockStyle.Fill;

            dgv.AutoGenerateColumns = false;
            dgv.ReadOnly = true;

            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;

            dgv.MultiSelect = false;
            dgv.RowHeadersVisible = false;

            dgv.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgv.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 40;
            dgv.RowTemplate.Height = 34;

            dgv.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(23, 52, 83);

            dgv.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dgv.ColumnHeadersDefaultCellStyle.Font =
                new Font(
                    "Segoe UI", 10, FontStyle.Bold);

            dgv.DefaultCellStyle.Font =
                new Font("Segoe UI", 10);

            dgv.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(210, 230, 250);

            dgv.DefaultCellStyle.SelectionForeColor =
                Color.Black;

            dgv.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(245, 248, 252);
        }

        // ========================================
        // 5. THÊM CỘT BẢNG
        // ========================================

        private void ThemCot(
            string property,
            string tieuDe,
            float doRong)
        {
            dgv.Columns.Add(
                new DataGridViewTextBoxColumn
                {
                    Name = property,
                    DataPropertyName = property,
                    HeaderText = tieuDe,
                    FillWeight = doRong,
                    ReadOnly = true
                });
        }

        // ========================================
        // 6. CHUYỂN TUẦN
        // ========================================

        private void ChuyenTuan(int soNgay)
        {
            DateTime ngayMoi =
                dtpTuan.Value.AddDays(soNgay);

            if (ngayMoi < dtpTuan.MinDate ||
                ngayMoi > dtpTuan.MaxDate)
            {
                MessageBox.Show(
                    "Tuần được chọn vượt phạm vi ngày cho phép.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            dtpTuan.Value = ngayMoi;
        }

        // ========================================
        // 7. TẢI GIÁO VIÊN
        // ========================================

        private void TaiDuLieu()
        {
            try
            {
                dangNap = true;

                var danhSach =
                    bll.LayGiaoViens();

                cboGiaoVien.DataSource = null;

                cboGiaoVien.DisplayMember =
                    nameof(GiaoVien.HoTen);

                cboGiaoVien.ValueMember =
                    nameof(GiaoVien.MaGV);

                if (maGiaoVienCoDinh.HasValue)
                {
                    var giaoVien = danhSach
                        .FirstOrDefault(x =>
                            x.MaGV ==
                            maGiaoVienCoDinh.Value);

                    if (giaoVien == null)
                    {
                        throw new InvalidOperationException(
                            "Không tìm thấy hồ sơ giáo viên " +
                            "của tài khoản đang đăng nhập.");
                    }

                    cboGiaoVien.DataSource =
                        new List<GiaoVien>
                        {
                            giaoVien
                        };

                    cboGiaoVien.SelectedIndex = 0;
                    cboGiaoVien.Enabled = false;
                }
                else
                {
                    // Admin có thể xem toàn bộ giáo viên.
                    var tatCa = new GiaoVien
                    {
                        MaGV = 0,
                        HoTen = "Tất cả giáo viên"
                    };

                    danhSach.Insert(0, tatCa);

                    cboGiaoVien.DataSource = danhSach;

                    cboGiaoVien.SelectedIndex = 0;
                    cboGiaoVien.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Không tải được giáo viên",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }
            finally
            {
                dangNap = false;
            }

            NapLich();
        }

        // ========================================
        // 8. NẠP LỊCH DẠY THEO TUẦN
        // ========================================

        private void NapLich()
        {
            try
            {
                DateTime dauTuan =
                    LichGiaoVienBLL.LayDauTuan(
                        dtpTuan.Value);

                DateTime cuoiTuan =
                    dauTuan.AddDays(6);

                int? maGV = maGiaoVienCoDinh;

                // Admin lọc theo giáo viên.
                if (!maGV.HasValue &&
                    cboGiaoVien.SelectedItem
                    is GiaoVien giaoVien &&
                    giaoVien.MaGV > 0)
                {
                    maGV = giaoVien.MaGV;
                }

                var danhSach =
                    bll.LayLichTheoTuan(
                        dauTuan, maGV);

                dgv.DataSource = null;
                dgv.DataSource = danhSach;

                dgv.ClearSelection();

                // Thống kê khoảng tuần.
                lblKhoangNgay.Text =
                    $"{dauTuan:dd/MM/yyyy} - " +
                    $"{cuoiTuan:dd/MM/yyyy}";

                // Thống kê tổng số buổi.
                lblSoBuoi.Text =
                    danhSach.Count.ToString();

                // Thống kê số lớp khác nhau.
                lblSoLop.Text =
                    danhSach
                        .Select(x => x.MaLop)
                        .Distinct()
                        .Count()
                        .ToString();

                lblTong.Text =
                    $"Tổng số: {danhSach.Count} buổi dạy";

                // Kiểm tra xung đột lịch.
                var canhBao =
                    bll.KiemTraTrungLich(danhSach);

                if (canhBao.Count == 0)
                {
                    lblCanhBao.Text =
                        "Không phát hiện giáo viên " +
                        "trùng giờ trong lịch tuần đang hiển thị.";
                }
                else
                {
                    lblCanhBao.Text =
                        "CẢNH BÁO TRÙNG LỊCH: " +
                        string.Join(
                            " | ",
                            canhBao.Take(3));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Không tải được lịch giáo viên",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
