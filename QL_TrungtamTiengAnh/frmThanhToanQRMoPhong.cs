
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using QRCoder;

namespace QL_TrungtamTiengAnh.GUI
{
    public sealed partial class frmThanhToanQRMoPhong : Form
    {
        // ========================================
        // THÔNG TIN THANH TOÁN
        // ========================================

        private readonly int maDangKy;
        private readonly string tenHocVien;
        private readonly string tenLop;
        private readonly string tenKhoaHoc;
        private readonly decimal conNoBanDau;

        // ========================================
        // CÁC CONTROL
        // ========================================

        private readonly PictureBox picQR = new();
        private readonly NumericUpDown nudSoTien = new();
        private readonly Label lblConThieu = new();
        private readonly Button btnThanhToan = new();
        private readonly Button btnDong = new();

        // ========================================
        // DỮ LIỆU TRẢ VỀ CHO frmHocPhi
        // ========================================

        public decimal SoTienThanhToan { get; private set; }

        public string GhiChuThanhToan { get; private set; } = "";

        public string MaGiaoDich { get; private set; }

        // ========================================
        // CONSTRUCTOR - 6 THAM SỐ
        // ========================================

        public frmThanhToanQRMoPhong(
            int maDangKy,
            string tenHocVien,
            string tenLop,
            string tenKhoaHoc,
            decimal conNoHienTai,
            decimal soTienGoiY = 0m)
        {
            if (maDangKy <= 0)
                throw new ArgumentException(
                    "Mã đăng ký không hợp lệ.");

            if (string.IsNullOrWhiteSpace(tenHocVien))
                throw new ArgumentException(
                    "Thiếu tên học viên.");

            if (conNoHienTai <= 0m)
                throw new ArgumentException(
                    "Đăng ký không còn công nợ để thanh toán.");

            if (soTienGoiY < 0m)
                throw new ArgumentException(
                    "Số tiền gợi ý không hợp lệ.");

            this.maDangKy = maDangKy;
            this.tenHocVien = tenHocVien;
            this.tenLop = tenLop ?? "";
            this.tenKhoaHoc = tenKhoaHoc ?? "";
            conNoBanDau = conNoHienTai;

            // Mã giao dịch phải khớp với HocPhiBLL.
            // Không dùng định dạng QRHP- cũ.
            MaGiaoDich = "DEMO-HP-" +
                Guid.NewGuid()
                    .ToString("N")[..12]
                    .ToUpperInvariant();

            TaoGiaoDien();

            nudSoTien.Maximum = conNoBanDau;

            nudSoTien.Value =
                soTienGoiY > 0m &&
                soTienGoiY <= conNoBanDau
                    ? soTienGoiY
                    : conNoBanDau;

            CapNhatQR();
        }

        // ========================================
        // THIẾT KẾ GIAO DIỆN
        // ========================================

        private void TaoGiaoDien()
        {
            Text = "Thanh Toán QR";

            StartPosition =
                FormStartPosition.CenterParent;

            ClientSize = new Size(590, 735);

            MinimumSize = new Size(530, 650);

            FormBorderStyle =
                FormBorderStyle.Sizable;

            MaximizeBox = false;
            MinimizeBox = false;

            AutoScaleMode = AutoScaleMode.Font;

            BackColor = Color.FromArgb(245, 248, 252);

            Font = new Font(
                "Segoe UI", 10, FontStyle.Regular);

            Controls.Clear();

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                ColumnCount = 1,
                RowCount = 6,
                AutoScroll = true
            };

            root.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            root.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 60));

            root.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 170));

            root.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 56));

            root.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            root.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 48));

            root.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 100));

            Controls.Add(root);

            // ========================================
            // 1. TIÊU ĐỀ
            // ========================================

            var lblTieuDe = new Label
            {
                Text = "THANH TOÁN QR",
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(
                    "Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            };

            root.Controls.Add(lblTieuDe, 0, 0);

            // ========================================
            // 2. THÔNG TIN HỌC VIÊN
            // ========================================

            var bangThongTin = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(14, 6, 14, 6),
                BackColor = Color.White,
                Margin = new Padding(2, 2, 2, 6)
            };

            bangThongTin.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 5; i++)
            {
                bangThongTin.RowStyles.Add(
                    new RowStyle(SizeType.Percent, 20));
            }

            bangThongTin.Controls.Add(
                TaoNhan($"Học viên: {tenHocVien}"),
                0, 0);

            bangThongTin.Controls.Add(
                TaoNhan($"Lớp: {tenLop}"),
                0, 1);

            bangThongTin.Controls.Add(
                TaoNhan($"Đóng khóa học: {tenKhoaHoc}"),
                0, 2);

            bangThongTin.Controls.Add(
                TaoNhan($"Mã giao dịch: {MaGiaoDich}"),
                0, 3);

            bangThongTin.Controls.Add(
                TaoNhan(
                    $"Số tiền nợ còn lại: " +
                    $"{conNoBanDau:N0} VNĐ"),
                0, 4);

            root.Controls.Add(bangThongTin, 0, 1);

            // ========================================
            // 3. SỐ TIỀN THANH TOÁN
            // ========================================

            var bangSoTien = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(2, 6, 2, 6)
            };

            bangSoTien.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 45));

            bangSoTien.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 55));

            var lblSoTien = new Label
            {
                Text = "Số tiền thanh toán:",
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = new Font(
                    "Segoe UI", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            bangSoTien.Controls.Add(lblSoTien, 0, 0);

            nudSoTien.Dock = DockStyle.Fill;
            nudSoTien.DecimalPlaces = 0;
            nudSoTien.Minimum = 0m;
            nudSoTien.Maximum = 1000000000000m;
            nudSoTien.Increment = 100000m;
            nudSoTien.ThousandsSeparator = true;

            nudSoTien.Font = new Font(
                "Segoe UI", 12, FontStyle.Bold);

            nudSoTien.TextAlign =
                HorizontalAlignment.Right;

            nudSoTien.ValueChanged += (_, _) =>
            {
                CapNhatQR();
            };

            bangSoTien.Controls.Add(nudSoTien, 1, 0);
            root.Controls.Add(bangSoTien, 0, 2);

            // ========================================
            // 4. KHUNG HIỂN THỊ QR
            // ========================================

            var khungQR = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(14),
                Margin = new Padding(14, 4, 14, 4)
            };

            picQR.Dock = DockStyle.Fill;
            picQR.BackColor = Color.White;

            picQR.SizeMode =
                PictureBoxSizeMode.Zoom;

            khungQR.Controls.Add(picQR);
            root.Controls.Add(khungQR, 0, 3);

            // ========================================
            // 5. HIỂN THỊ TIỀN CÒN THIẾU
            // ========================================

            lblConThieu.Dock = DockStyle.Fill;

            lblConThieu.TextAlign =
                ContentAlignment.MiddleCenter;

            lblConThieu.Font = new Font(
                "Segoe UI", 11, FontStyle.Bold);

            lblConThieu.ForeColor =
                Color.FromArgb(180, 90, 20);

            root.Controls.Add(lblConThieu, 0, 4);

            // ========================================
            // 6. CÁC NÚT CHỨC NĂNG
            // ========================================

            var bangNut = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            bangNut.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            bangNut.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50));

            bangNut.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50));

            btnThanhToan.Text = "Thanh toán Thành Công";
            btnThanhToan.Dock = DockStyle.Fill;

            btnThanhToan.BackColor =
                Color.FromArgb(30, 145, 100);

            btnThanhToan.ForeColor = Color.White;

            btnThanhToan.FlatStyle =
                FlatStyle.Flat;

            btnThanhToan.FlatAppearance.BorderSize = 0;

            btnThanhToan.Font = new Font(
                "Segoe UI", 10, FontStyle.Bold);

            btnThanhToan.Cursor = Cursors.Hand;
            btnThanhToan.Margin = new Padding(2, 2, 2, 4);

            btnThanhToan.Click += BtnThanhToan_Click;

            bangNut.Controls.Add(btnThanhToan, 0, 0);

            btnDong.Text = "Đóng";
            btnDong.Dock = DockStyle.Fill;

            btnDong.BackColor =
                Color.FromArgb(95, 110, 125);

            btnDong.ForeColor = Color.White;

            btnDong.FlatStyle =
                FlatStyle.Flat;

            btnDong.FlatAppearance.BorderSize = 0;

            btnDong.Font = new Font(
                "Segoe UI", 10, FontStyle.Regular);

            btnDong.Cursor = Cursors.Hand;
            btnDong.Margin = new Padding(2, 3, 2, 2);

            btnDong.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            bangNut.Controls.Add(btnDong, 0, 1);

            root.Controls.Add(bangNut, 0, 5);

            CancelButton = btnDong;
        }

        // ========================================
        // TẠO NHÃN THÔNG TIN
        // ========================================

        private static Label TaoNhan(string noiDung)
        {
            return new Label
            {
                Text = noiDung,
                Dock = DockStyle.Fill,
                AutoSize = false,

                Font = new Font(
                    "Segoe UI", 10, FontStyle.Regular),

                ForeColor = Color.FromArgb(40, 50, 65),

                TextAlign =
                    ContentAlignment.MiddleLeft
            };
        }

        // ========================================
        // THAY ẢNH QR AN TOÀN
        // ========================================

        private void ThayAnhQR(Bitmap? anhMoi)
        {
            Image? anhHienTai = picQR.Image;

            picQR.Image = anhMoi;

            if (anhHienTai != null)
            {
                anhHienTai.Dispose();
            }
        }

        // ========================================
        // TẠO MÃ QR THEO SỐ TIỀN
        // ========================================

        private void CapNhatQR()
        {
            decimal soTien = nudSoTien.Value;
            decimal conThieu = conNoBanDau - soTien;

            if (soTien <= 0m)
            {
                btnThanhToan.Enabled = false;

                lblConThieu.Text =
                    $"Còn thiếu: {conNoBanDau:N0} VNĐ";

                ThayAnhQR(null);
                return;
            }

            btnThanhToan.Enabled = true;

            if (conThieu > 0m)
            {
                lblConThieu.Text =
                    $"Dự kiến còn thiếu: {conThieu:N0} VNĐ";
            }
            else
            {
                lblConThieu.Text =
                    "Dự kiến thanh toán đủ học phí";
            }

            // QR chỉ chứa thông tin thử nghiệm.
            // Không phải VietQR ngân hàng.

            string noiDungQR =
                "ENGLISH CENTER - PAYMENT DEMO ONLY\n" +
                $"REGISTER: {maDangKy}\n" +
                $"STUDENT: {tenHocVien}\n" +
                $"CLASS: {tenLop}\n" +
                $"COURSE: {tenKhoaHoc}\n" +
                $"AMOUNT: {soTien:0} VND\n" +
                $"REFERENCE: {MaGiaoDich}\n" +
                "NO REAL BANK TRANSFER";

            using var generator = new QRCodeGenerator();

            using var qrData = generator.CreateQrCode(
                noiDungQR,
                QRCodeGenerator.ECCLevel.Q);

            var qr = new PngByteQRCode(qrData);

            byte[] duLieuAnh = qr.GetGraphic(8);

            using var boNho = new MemoryStream(duLieuAnh);

            using var anhTam = Image.FromStream(boNho);

            Bitmap anhQR = new Bitmap(anhTam);

            // Không dùng lại biến anhCu ở các khối khác.
            ThayAnhQR(anhQR);
        }

        // ========================================
        // XỬ LÝ NÚT THANH TOÁN
        // ========================================

        private void BtnThanhToan_Click(
            object? sender,
            EventArgs e)
        {
            decimal soTien = nudSoTien.Value;

            if (soTien <= 0m || soTien > conNoBanDau)
            {
                MessageBox.Show(
                    "Số tiền thanh toán không hợp lệ.",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            decimal conThieu = conNoBanDau - soTien;

            var xacNhan = MessageBox.Show(
                "Xác nhận giao dịch QR mô phỏng?\n\n" +
                $"Học viên: {tenHocVien}\n" +
                $"Số tiền: {soTien:N0} VNĐ\n" +
                $"Dự kiến còn thiếu: {conThieu:N0} VNĐ\n\n" +
                "Không có giao dịch ngân hàng thật.",
                "Xác nhận thanh toán DEMO",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (xacNhan != DialogResult.Yes)
                return;

            SoTienThanhToan = soTien;

            if (conThieu > 0m)
            {
                GhiChuThanhToan =
                    $"Đã nộp QR DEMO {soTien:N0}đ, " +
                    $"còn thiếu {conThieu:N0}đ";
            }
            else
            {
                GhiChuThanhToan =
                    "Đã thanh toán đủ học phí bằng QR DEMO";
            }

            // Xóa ảnh QR khi xác nhận.
            ThayAnhQR(null);

            // Trả kết quả cho frmHocPhi.cs.
            // frmHocPhi.cs gọi BLL/DAL để lưu SQL.
            DialogResult = DialogResult.OK;

            Close();
        }

        // ========================================
        // GIẢI PHÓNG BỘ NHỚ
        // ========================================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ThayAnhQR(null);
            }

            base.Dispose(disposing);
        }
    }
}
