using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ClosedXML.Excel;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmKhoaHoc : Form
    {
        private readonly KhoaHocBLL bll = new();
        private readonly Color xanhDam = Color.FromArgb(23, 52, 83);
        private readonly Color nen = Color.FromArgb(245, 248, 252);

        private int? maDangChon;
        private bool dangNap;
        private List<KhoaHoc> ketQuaHienTai = new();

        private readonly TextBox txtMa = new();
        private readonly TextBox txtChungChi = new();
        private readonly ComboBox cboLoaiKhoa = new();
        private readonly NumericUpDown nudHocPhi = new();
        private readonly TextBox txtMoTa = new();
        private readonly ComboBox cboDauVao = new();
        private readonly ComboBox cboDauRa = new();
        private readonly ComboBox cboThang = new();
        private readonly NumericUpDown nudSoBuoi = new();
        private readonly NumericUpDown nudGioBuoi = new();
        private readonly TextBox txtTim = new();
        private readonly ComboBox cboLocTrangThai = new();
        private readonly ComboBox cboLocDauVao = new();
        private readonly ComboBox cboLocDauRa = new();
        private readonly DataGridView dgv = new();
        private readonly Label lblSoLuong = new();
        private readonly Button btnDoiTrangThai = new();

        // DTO hiển thị thuần chuỗi: không cần CellFormatting,
        // tránh FormatException như màn hình Giáo viên trước đây.
        private sealed class DongKhoaHoc
        {
            public KhoaHoc Goc { get; set; } = null!;
            public int STT { get; set; }
            public string MaHienThi { get; set; } = "";
            public string ChungChi { get; set; } = "";
            public string LoaiKhoa { get; set; } = "";
            public string Thang { get; set; } = "";
            public string Buoi { get; set; } = "";
            public string DauVao { get; set; } = "";
            public string DauRa { get; set; } = "";
            public string HocPhi { get; set; } = "";
            public string TrangThai { get; set; } = "";
        }

        public frmKhoaHoc()
        {
            InitializeComponent();
            TaoGiaoDien();
            Shown += (_, _) => XuLyAnToan(NapDanhSach);
        }

        private void TaoGiaoDien()
        {
            Text = "Quản lý khóa học";
            ClientSize = new Size(1240, 760);
            MinimumSize = new Size(850, 570);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10);
            BackColor = nen;
            Controls.Clear();

            var goc = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = nen,
                Padding = new Padding(16, 12, 16, 14)
            };
            goc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            goc.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            goc.RowStyles.Add(new RowStyle(SizeType.Absolute, 315));
            goc.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            goc.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(goc);

            var dauTrang = new Panel { Dock = DockStyle.Fill };
            dauTrang.Controls.Add(new Label
            {
                Text = "QUẢN LÝ KHÓA HỌC",
                Location = new Point(4, 2),
                AutoSize = true,
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                ForeColor = xanhDam
            });
            dauTrang.Controls.Add(new Label
            {
                Text = "Chứng chỉ • Lộ trình đào tạo • Học phí",
                Location = new Point(5, 46),
                AutoSize = true,
                ForeColor = Color.DimGray
            });
            goc.Controls.Add(dauTrang, 0, 0);

            var haiKhung = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(0, 0, 0, 8)
            };
            haiKhung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            haiKhung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            haiKhung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            goc.Controls.Add(haiKhung, 0, 1);

            var khungChungChi = TaoKhung("THÔNG TIN CHỨNG CHỈ");
            khungChungChi.Margin = new Padding(0, 0, 6, 0);
            var khungDaoTao = TaoKhung("LỘ TRÌNH ĐÀO TẠO");
            khungDaoTao.Margin = new Padding(6, 0, 0, 0);
            haiKhung.Controls.Add(khungChungChi, 0, 0);
            haiKhung.Controls.Add(khungDaoTao, 1, 0);

            // Thông tin chung: 4 hàng bình thường + mô tả nhiều dòng.
            var thongTin = TaoBangNhap(5, 113);
            thongTin.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
            thongTin.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
            thongTin.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
            thongTin.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
            thongTin.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            khungChungChi.Controls.Add(thongTin);

            txtMa.ReadOnly = true;
            txtMa.BackColor = Color.FromArgb(237, 242, 248);
            txtChungChi.PlaceholderText = "Ví dụ: Chứng chỉ Cambridge B1";
            txtChungChi.MaxLength = 100;
            cboLoaiKhoa.DropDownStyle = ComboBoxStyle.DropDown;
            cboLoaiKhoa.MaxLength = 60;
            cboLoaiKhoa.Items.AddRange(new object[]
            {
                "Cambridge", "IELTS", "TOEIC", "Giao tiếp", "Thiếu nhi", "Khác"
            });
            nudHocPhi.Maximum = 1_000_000_000m;
            nudHocPhi.Increment = 100_000m;
            nudHocPhi.ThousandsSeparator = true;
            nudHocPhi.DecimalPlaces = 0;
            txtMoTa.MaxLength = 500;
            txtMoTa.Multiline = true;
            txtMoTa.ScrollBars = ScrollBars.Vertical;

            ThemDong(thongTin, 0, "Mã khóa", txtMa);
            ThemDong(thongTin, 1, "Chứng chỉ (*)", txtChungChi);
            ThemDong(thongTin, 2, "Loại khóa (*)", cboLoaiKhoa);
            ThemDong(thongTin, 3, "Học phí (đ)", nudHocPhi);
            ThemDong(thongTin, 4, "Mô tả", txtMoTa);

            var loTrinh = TaoBangNhap(5, 132);
            for (int i = 0; i < 5; i++)
                loTrinh.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 5));
            khungDaoTao.Controls.Add(loTrinh);

            cboDauVao.DropDownStyle = ComboBoxStyle.DropDown;
            cboDauRa.DropDownStyle = ComboBoxStyle.DropDown;
            cboDauVao.MaxLength = 50;
            cboDauRa.MaxLength = 50;
            string[] trinhDo = { "A1", "A2", "B1", "B2", "C1", "C2" };
            cboDauVao.Items.AddRange(trinhDo);
            cboDauRa.Items.AddRange(trinhDo);
            cboThang.DropDownStyle = ComboBoxStyle.DropDownList;
            cboThang.Items.AddRange(new object[] { "3 Tháng", "6 Tháng", "9 Tháng", "12 Tháng" });
            cboThang.SelectedIndex = 0;
            nudSoBuoi.Minimum = 1;
            nudSoBuoi.Maximum = 1000;
            nudSoBuoi.Value = 24;
            nudGioBuoi.Minimum = 1;
            nudGioBuoi.Maximum = 8;
            nudGioBuoi.Value = 2;

            ThemDong(loTrinh, 0, "Đầu vào (*)", cboDauVao);
            ThemDong(loTrinh, 1, "Đầu ra (*)", cboDauRa);
            ThemDong(loTrinh, 2, "Thời lượng", cboThang);
            ThemDong(loTrinh, 3, "Số buổi (*)", nudSoBuoi);
            ThemDong(loTrinh, 4, "Giờ mỗi buổi", nudGioBuoi);

            // Hai dòng công cụ riêng, không chèn lên vùng DataGridView.
            var congCu = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 4, 0, 6)
            };
            congCu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            congCu.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            congCu.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            goc.Controls.Add(congCu, 0, 2);

            var hangNut = TaoHangCongCu();
            Button btnThem = TaoNut("Thêm", Color.FromArgb(31, 138, 95), 88);
            Button btnSua = TaoNut("Sửa", Color.FromArgb(42, 107, 185), 88);
            Button btnXoa = TaoNut("Xóa", Color.FromArgb(190, 61, 67), 88);
            Button btnLamMoi = TaoNut("Làm mới", Color.FromArgb(91, 106, 125), 105);
            btnDoiTrangThai.Text = "Đóng / Mở tuyển sinh";
            DinhDangNut(btnDoiTrangThai, Color.FromArgb(132, 96, 45), 175);
            Button btnXuat = TaoNut("Xuất Excel", Color.FromArgb(17, 118, 78), 120);
            hangNut.Controls.AddRange(new Control[]
                { btnThem, btnSua, btnXoa, btnLamMoi, btnDoiTrangThai, btnXuat });
            congCu.Controls.Add(hangNut, 0, 0);

            var hangLoc = TaoHangCongCu();
            txtTim.Width = 245;
            txtTim.PlaceholderText = "Tìm chứng chỉ, loại khóa...";
            txtTim.Margin = new Padding(0, 5, 9, 2);
            Button btnTim = TaoNut("Tìm", xanhDam, 65);
            cboLocTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocTrangThai.Items.AddRange(new object[]
                { "Tất cả trạng thái", "Đang tuyển sinh", "Ngừng tuyển sinh" });
            cboLocTrangThai.SelectedIndex = 0;
            cboLocDauVao.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocDauRa.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocDauVao.Items.AddRange(new object[] { "Đầu vào: Tất cả", "A1", "A2", "B1", "B2", "C1", "C2" });
            cboLocDauRa.Items.AddRange(new object[] { "Đầu ra: Tất cả", "A1", "A2", "B1", "B2", "C1", "C2" });
            cboLocDauVao.SelectedIndex = 0;
            cboLocDauRa.SelectedIndex = 0;
            CaiDatLoc(cboLocTrangThai, 165);
            CaiDatLoc(cboLocDauVao, 150);
            CaiDatLoc(cboLocDauRa, 150);
            hangLoc.Controls.AddRange(new Control[]
                { txtTim, btnTim, cboLocTrangThai, cboLocDauVao, cboLocDauRa });
            congCu.Controls.Add(hangLoc, 0, 1);

            btnThem.Click += (_, _) => XuLyAnToan(Them);
            btnSua.Click += (_, _) => XuLyAnToan(Sua);
            btnXoa.Click += (_, _) => XuLyAnToan(Xoa);
            btnLamMoi.Click += (_, _) => XuLyAnToan(LamMoi);
            btnDoiTrangThai.Click += (_, _) => XuLyAnToan(DoiTrangThai);
            btnXuat.Click += (_, _) => XuLyAnToan(XuatExcel);
            btnTim.Click += (_, _) => XuLyAnToan(NapDanhSach);
            cboLocTrangThai.SelectedIndexChanged += (_, _) => XuLyAnToan(NapDanhSach);
            cboLocDauVao.SelectedIndexChanged += (_, _) => XuLyAnToan(NapDanhSach);
            cboLocDauRa.SelectedIndexChanged += (_, _) => XuLyAnToan(NapDanhSach);
            txtTim.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter) return;
                e.SuppressKeyPress = true;
                XuLyAnToan(NapDanhSach);
            };

            var danhSachPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
            var lblDanhSach = new Label
            {
                Text = "DANH SÁCH CHỨNG CHỈ",
                Dock = DockStyle.Top,
                Height = 38,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = xanhDam
            };
            lblSoLuong.Dock = DockStyle.Bottom;
            lblSoLuong.Height = 28;
            lblSoLuong.TextAlign = ContentAlignment.MiddleRight;
            CauHinhBang();
            danhSachPanel.Controls.Add(dgv);
            danhSachPanel.Controls.Add(lblSoLuong);
            danhSachPanel.Controls.Add(lblDanhSach);
            goc.Controls.Add(danhSachPanel, 0, 3);
            XoaNhap();
        }

        private static GroupBox TaoKhung(string ten) => new()
        {
            Text = ten,
            Dock = DockStyle.Fill,
            BackColor = Color.White,
            ForeColor = Color.FromArgb(23, 52, 83),
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Padding = new Padding(12, 13, 12, 10)
        };

        private static TableLayoutPanel TaoBangNhap(int hang, int rongNhan)
        {
            var bang = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = hang,
                Padding = new Padding(4, 6, 4, 5)
            };
            bang.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, rongNhan));
            bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            return bang;
        }

        private static void ThemDong(TableLayoutPanel bang, int dong, string ten, Control input)
        {
            bang.Controls.Add(new Label
            {
                Text = ten,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(45, 55, 70),
                Font = new Font("Segoe UI", 10)
            }, 0, dong);
            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(3, 5, 7, 5);
            input.Font = new Font("Segoe UI", 10);
            bang.Controls.Add(input, 1, dong);
        }

        private static FlowLayoutPanel TaoHangCongCu() => new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            AutoScroll = true,
            Margin = new Padding(0),
            Padding = new Padding(0, 3, 0, 0)
        };

        private static Button TaoNut(string ten, Color mau, int rong)
        {
            var btn = new Button { Text = ten };
            DinhDangNut(btn, mau, rong);
            return btn;
        }
        private static void DinhDangNut(Button btn, Color mau, int rong)
        {
            btn.Width = rong;
            btn.Height = 36;
            btn.BackColor = mau;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Margin = new Padding(0, 0, 7, 2);
        }

        private static void CaiDatLoc(ComboBox cbo, int rong)
        {
            cbo.Width = rong;
            cbo.Height = 32;
            cbo.Margin = new Padding(5, 5, 3, 2);
        }

        private void CauHinhBang()
        {
            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.AutoGenerateColumns = false;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 44;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.RowTemplate.Height = 36;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 231, 249);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = xanhDam;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(246, 249, 252);
            dgv.ScrollBars = ScrollBars.Both;

            Cot("STT", "STT", 40);
            Cot("MaHienThi", "Mã", 57);
            Cot("ChungChi", "Chứng chỉ", 185);
            Cot("LoaiKhoa", "Loại khóa", 105);
            Cot("Thang", "Thời lượng", 85);
            Cot("Buoi", "Số buổi", 65);
            Cot("DauVao", "Đầu vào", 75);
            Cot("DauRa", "Đầu ra", 75);
            Cot("HocPhi", "Học phí (đ)", 110);
            Cot("TrangThai", "Tuyển sinh", 103);
            dgv.SelectionChanged += ChonDong;
        }

        private void Cot(string ten, string nhan, float tyLe)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ten,
                DataPropertyName = ten,
                HeaderText = nhan,
                FillWeight = tyLe,
                MinimumWidth = ten == "ChungChi" ? 130 : 48,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        private void NapDanhSach()
        {
            bool? trangThai = cboLocTrangThai.SelectedIndex switch
            {
                1 => true,
                2 => false,
                _ => null
            };
            string? dauVao = cboLocDauVao.SelectedIndex > 0 ? cboLocDauVao.Text : null;
            string? dauRa = cboLocDauRa.SelectedIndex > 0 ? cboLocDauRa.Text : null;
            ketQuaHienTai = bll.LocDanhSach(txtTim.Text.Trim(), trangThai, dauVao, dauRa);
            var view = ketQuaHienTai.Select((k, i) => new DongKhoaHoc
            {
                Goc = k,
                STT = i + 1,
                MaHienThi = $"KH{k.MaKH:D2}",
                ChungChi = k.TenKH,
                LoaiKhoa = k.LoaiKhoa ?? "—",
                Thang = k.ThoiLuongThang is int th ? $"{th} tháng" : $"{k.ThoiLuong} giờ (cũ)",
                Buoi = k.SoBuoi?.ToString() ?? "—",
                DauVao = k.TrinhDoDauVao ?? k.TrinhDo ?? "—",
                DauRa = k.TrinhDoDauRa ?? "—",
                HocPhi = $"{k.HocPhi:N0}",
                TrangThai = k.TrangThai ? "Đang tuyển" : "Ngừng tuyển"
            }).ToList();

            dangNap = true;
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = view;
                dgv.ClearSelection();
                XoaNhap();
                lblSoLuong.Text = $"Đang hiển thị: {view.Count} chứng chỉ";
            }
            finally { dangNap = false; }
        }

        // Chuyển cả giá trị số và chuỗi (ví dụ "3 Tháng") thành số tháng.
        // Không phụ thuộc kiểu dữ liệu của SelectedItem.
        private static int? DocThang(object? giaTri)
        {
            string noiDung = giaTri?.ToString()?.Trim() ?? "";
            Match hopLe = Regex.Match(noiDung,
                @"^(3|6|9|12)\s*(?:tháng)?$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return hopLe.Success && int.TryParse(hopLe.Groups[1].Value, out int soThang)
                ? soThang
                : null;
        }

        private int? LayThangDaChon() =>
            DocThang(cboThang.SelectedItem ?? cboThang.Text);

        // Khi chọn bản ghi cũ hoặc bấm Làm mới, luôn chọn đúng tháng.
        private void ChonThang(int? thang)
        {
            cboThang.SelectedIndex = -1;
            if (!thang.HasValue) return;

            for (int i = 0; i < cboThang.Items.Count; i++)
            {
                if (DocThang(cboThang.Items[i]) == thang)
                {
                    cboThang.SelectedIndex = i;
                    return;
                }
            }
        }

        private KhoaHoc LayNhap() => new()
        {
            TenKH = txtChungChi.Text,
            LoaiKhoa = cboLoaiKhoa.Text,
            TrinhDoDauVao = cboDauVao.Text,
            TrinhDoDauRa = cboDauRa.Text,
            ThoiLuongThang = LayThangDaChon(),
            SoBuoi = (int)nudSoBuoi.Value,
            SoGioMoiBuoi = (int)nudGioBuoi.Value,
            HocPhi = nudHocPhi.Value,
            MoTa = txtMoTa.Text
        };

        private void Them()
        {
            int ma = bll.Them(LayNhap());
            MessageBox.Show($"Đã thêm chứng chỉ KH{ma:D2}.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            NapDanhSach();
        }

        private void Sua()
        {
            if (!maDangChon.HasValue)
                throw new ArgumentException("Hãy chọn một chứng chỉ trong danh sách.");
            KhoaHoc kh = LayNhap();
            kh.MaKH = maDangChon.Value;
            if (!bll.Sua(kh))
                throw new InvalidOperationException("Không tìm thấy chứng chỉ cần sửa.");
            MessageBox.Show("Đã cập nhật chứng chỉ.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            NapDanhSach();
        }

        private void Xoa()
        {
            if (!maDangChon.HasValue)
                throw new ArgumentException("Hãy chọn chứng chỉ cần xóa.");
            if (MessageBox.Show("Xóa chứng chỉ đã chọn?\n" +
                    "Khóa đã có lớp liên kết sẽ không được xóa.",
                    "Xác nhận", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;
            if (!bll.Xoa(maDangChon.Value))
                throw new InvalidOperationException("Không tìm thấy chứng chỉ.");
            MessageBox.Show("Xóa thành công.");
            NapDanhSach();
        }

        private void DoiTrangThai()
        {
            if (!maDangChon.HasValue)
                throw new ArgumentException("Hãy chọn chứng chỉ cần thay đổi tuyển sinh.");
            KhoaHoc? kh = bll.LayTheoMa(maDangChon.Value);
            if (kh == null) throw new InvalidOperationException("Không tìm thấy chứng chỉ.");
            bool moi = !kh.TrangThai;
            string ten = moi ? "MỞ" : "ĐÓNG";
            if (MessageBox.Show($"{ten} tuyển sinh cho KH{kh.MaKH:D2}?",
                    "Xác nhận", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question) != DialogResult.Yes) return;
            bll.DoiTrangThai(kh.MaKH, moi);
            NapDanhSach();
        }

        private void LamMoi()
        {
            txtTim.Clear();
            cboLocTrangThai.SelectedIndex = 0;
            cboLocDauVao.SelectedIndex = 0;
            cboLocDauRa.SelectedIndex = 0;
            NapDanhSach();
        }

        private void ChonDong(object? sender, EventArgs e)
        {
            if (dangNap || dgv.SelectedRows.Count == 0 ||
                dgv.SelectedRows[0].DataBoundItem is not DongKhoaHoc dong) return;
            KhoaHoc kh = dong.Goc;
            maDangChon = kh.MaKH;
            txtMa.Text = dong.MaHienThi;
            txtChungChi.Text = kh.TenKH;
            cboLoaiKhoa.Text = kh.LoaiKhoa ?? "";
            cboDauVao.Text = kh.TrinhDoDauVao ?? kh.TrinhDo ?? "";
            cboDauRa.Text = kh.TrinhDoDauRa ?? "";
            ChonThang(kh.ThoiLuongThang);
            nudSoBuoi.Value = Math.Clamp(kh.SoBuoi ?? 24, 1, 1000);
            nudGioBuoi.Value = Math.Clamp(kh.SoGioMoiBuoi ?? 2, 1, 8);
            nudHocPhi.Value = Math.Clamp(kh.HocPhi, nudHocPhi.Minimum, nudHocPhi.Maximum);
            txtMoTa.Text = kh.MoTa ?? "";
            btnDoiTrangThai.Text = kh.TrangThai ? "Đóng tuyển sinh" : "Mở tuyển sinh";
        }

        private void XoaNhap()
        {
            maDangChon = null;
            txtMa.Clear();
            txtChungChi.Clear();
            cboLoaiKhoa.Text = "";
            cboDauVao.Text = "";
            cboDauRa.Text = "";
            ChonThang(3);
            nudSoBuoi.Value = 24;
            nudGioBuoi.Value = 2;
            nudHocPhi.Value = 0;
            txtMoTa.Clear();
            btnDoiTrangThai.Text = "Đóng / Mở tuyển sinh";
        }

        private void XuatExcel()
        {
            if (ketQuaHienTai.Count == 0)
            {
                MessageBox.Show("Danh sách đang trống, không có dữ liệu để xuất.");
                return;
            }
            using var dlg = new SaveFileDialog
            {
                Title = "Xuất danh sách chứng chỉ",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"DanhSachChungChi_{DateTime.Now:yyyyMMdd_HHmm}.xlsx",
                DefaultExt = "xlsx",
                AddExtension = true
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Danh sach chung chi");
            string[] headers =
            {
                "STT", "Mã khóa", "Chứng chỉ", "Loại khóa", "Đầu vào", "Đầu ra",
                "Thời lượng (tháng)", "Số buổi", "Giờ/buổi", "Tổng giờ (cũ)",
                 "Học phí (VNĐ)", "Tuyển sinh", "Mô tả"
            };
            for (int c = 0; c < headers.Length; c++)
                ws.Cell(1, c + 1).Value = headers[c];

            for (int i = 0; i < ketQuaHienTai.Count; i++)
            {
                KhoaHoc k = ketQuaHienTai[i];
                int r = i + 2;
                ws.Cell(r, 1).Value = i + 1;
                ws.Cell(r, 2).Value = $"KH{k.MaKH:D2}";
                ws.Cell(r, 3).Value = k.TenKH;
                ws.Cell(r, 4).Value = k.LoaiKhoa ?? "";
                ws.Cell(r, 5).Value = k.TrinhDoDauVao ?? k.TrinhDo ?? "";
                ws.Cell(r, 6).Value = k.TrinhDoDauRa ?? "";
                if (k.ThoiLuongThang.HasValue) ws.Cell(r, 7).Value = k.ThoiLuongThang.Value;
                if (k.SoBuoi.HasValue) ws.Cell(r, 8).Value = k.SoBuoi.Value;
                if (k.SoGioMoiBuoi.HasValue) ws.Cell(r, 9).Value = k.SoGioMoiBuoi.Value;
                ws.Cell(r, 10).Value = k.ThoiLuong;
                ws.Cell(r, 11).Value = k.HocPhi;
                ws.Cell(r, 12).Value = k.TrangThai ? "Đang tuyển sinh" : "Ngừng tuyển sinh";
                ws.Cell(r, 13).Value = k.MoTa ?? "";
            }
            var vung = ws.Range(1, 1, ketQuaHienTai.Count + 1, headers.Length);
            vung.SetAutoFilter();
            ws.SheetView.FreezeRows(1);
            var dau = ws.Range(1, 1, 1, headers.Length);
            dau.Style.Font.Bold = true;
            dau.Style.Fill.BackgroundColor = XLColor.FromHtml("#173453");
            dau.Style.Font.FontColor = XLColor.White;
            ws.Column(11).Style.NumberFormat.Format = "#,##0";
            ws.Columns().AdjustToContents();
            wb.SaveAs(dlg.FileName);
            MessageBox.Show("Đã xuất Excel thành công.", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void XuLyAnToan(Action congViec)
        {
            try { congViec(); }
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
                MessageBox.Show("Đã xảy ra lỗi:\n" + ex.Message, "Thông báo lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
