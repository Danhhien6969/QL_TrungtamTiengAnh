using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmPhongHoc : Form
    {
        private readonly PhongHocBLL bll = new();
        private int? maDangChon;
        private bool dangNap;

        private readonly TextBox txtMaPhong = new();
        private readonly TextBox txtTenPhong = new();
        private readonly NumericUpDown nudSucChua = new();
        private readonly CheckBox chkTrangThai = new();
        private readonly TextBox txtTimKiem = new();
        private readonly DataGridView dgv = new();
        private readonly Label lblSoLuong = new();
        private readonly Label lblTrangThaiTong = new();
        private readonly ComboBox cboLocTrangThai = new();

        // DTO hiển thị chữ trạng thái; không bind bool trực tiếp vào cột TextBox.
        private sealed class DongPhongHoc
        {
            public int STT { get; init; }
            // Giữ mã số thật để truy vấn/cập nhật bản ghi trong SQL Server.
            public int MaPhong { get; init; }
            // Chỉ dùng cho cột hiển thị, ví dụ: PH01, PH02, PH100.
            public string MaPhongHienThi { get; init; } = "";
            public string TenPhong { get; init; } = "";
            public int SucChua { get; init; }
            public string TrangThaiText { get; init; } = "";
        }

        public frmPhongHoc()
        {
            InitializeComponent();
            TaoGiaoDien();
            Shown += (_, _) => XuLyAnToan(() => NapDanhSach());
        }

        // Giữ MaPhong trong SQL là int/IDENTITY; chỉ định dạng lúc hiển thị.
        private static string DinhDangMaPhong(int maPhong) => $"PH{maPhong:D2}";

        private static Button Nut(string ten, Color mau)
        {
            var btn = new Button
            {
                Text = ten,
                Width = 105,
                Height = 39,
                FlatStyle = FlatStyle.Flat,
                BackColor = mau,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 5)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static void ThemTruong(TableLayoutPanel grid,
            string nhan, Control input, int dong, int nua)
        {
            int cot = nua * 2;
            grid.Controls.Add(new Label
            {
                Text = nhan,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(45, 55, 70)
            }, cot, dong);
            input.Dock = DockStyle.Fill;
            input.Font = new Font("Segoe UI", 11);
            input.Margin = new Padding(4, 7, 12, 7);
            grid.Controls.Add(input, cot + 1, dong);
        }

        private void TaoGiaoDien()
        {
            Text = "Quản lý phòng học";
            ClientSize = new Size(1180, 740);
            MinimumSize = new Size(900, 620);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);
            Controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(20)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 155));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            var header = new Panel { Dock = DockStyle.Fill };
            header.Controls.Add(new Label
            {
                Text = "QUẢN LÝ PHÒNG HỌC",
                Location = new Point(5, 5),
                AutoSize = true,
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });
            header.Controls.Add(new Label
            {
                Text = "Chỉ phòng đang dùng mới được chọn khi xếp lớp hoặc lịch học",
                Location = new Point(7, 47),
                AutoSize = true,
                ForeColor = Color.DimGray
            });
            layout.Controls.Add(header, 0, 0);

            var grp = new GroupBox
            {
                Text = "THÔNG TIN PHÒNG HỌC",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12),
                ForeColor = Color.FromArgb(23, 52, 83),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2,
                Padding = new Padding(8)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            txtMaPhong.ReadOnly = true;
            txtMaPhong.BackColor = Color.FromArgb(240, 243, 247);
            txtTenPhong.PlaceholderText = "Ví dụ: P101";
            txtTenPhong.MaxLength = 50;
            nudSucChua.Minimum = 1;
            nudSucChua.Maximum = 200;
            nudSucChua.Value = 30;
            chkTrangThai.Text = "Đang dùng (cho phép chọn ở Lớp học / Lịch học)";
            chkTrangThai.Checked = true;
            chkTrangThai.AutoSize = true;

            ThemTruong(grid, "Mã phòng", txtMaPhong, 0, 0);
            ThemTruong(grid, "Tên phòng (*)", txtTenPhong, 0, 1);
            ThemTruong(grid, "Sức chứa", nudSucChua, 1, 0);
            ThemTruong(grid, "Trạng thái", chkTrangThai, 1, 1);
            grp.Controls.Add(grid);
            layout.Controls.Add(grp, 0, 1);

            var tools = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(0, 12, 0, 0)
            };
            var btnThem = Nut("Thêm", Color.FromArgb(30, 140, 95));
            var btnSua = Nut("Sửa", Color.FromArgb(40, 110, 190));
            var btnXoa = Nut("Xóa", Color.FromArgb(195, 65, 70));
            var btnLamMoi = Nut("Làm mới", Color.FromArgb(95, 110, 125));
            var btnTim = Nut("Tìm kiếm", Color.FromArgb(23, 52, 83));
            txtTimKiem.Width = 220;
            txtTimKiem.Font = new Font("Segoe UI", 11);
            txtTimKiem.Margin = new Padding(12, 5, 6, 5);
            txtTimKiem.PlaceholderText = "Tìm theo tên phòng...";

            cboLocTrangThai.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLocTrangThai.Items.AddRange(new object[]
            {
                "Tất cả trạng thái", "Đang dùng", "Ngừng"
            });
            cboLocTrangThai.Width = 160;
            cboLocTrangThai.Height = 32;
            cboLocTrangThai.Margin = new Padding(8, 5, 6, 5);
            cboLocTrangThai.Font = new Font("Segoe UI", 10);
            cboLocTrangThai.SelectedIndex = 0;

            tools.Controls.AddRange(new Control[]
            {
                btnThem, btnSua, btnXoa, btnLamMoi, txtTimKiem, btnTim,
                cboLocTrangThai
            });
            cboLocTrangThai.SelectedIndexChanged += (_, _) =>
                XuLyAnToan(() => NapDanhSach(txtTimKiem.Text));
            layout.Controls.Add(tools, 0, 2);
            btnThem.Click += (_, _) => XuLyAnToan(Them);
            btnSua.Click += (_, _) => XuLyAnToan(Sua);
            btnXoa.Click += (_, _) => XuLyAnToan(Xoa);
            btnLamMoi.Click += (_, _) => XuLyAnToan(LamMoi);
            btnTim.Click += (_, _) => XuLyAnToan(() => NapDanhSach(txtTimKiem.Text));
            txtTimKiem.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    XuLyAnToan(() => NapDanhSach(txtTimKiem.Text));
                }
            };

            var content = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            content.Controls.Add(dgv);
            lblSoLuong.Dock = DockStyle.Bottom;
            lblSoLuong.Height = 32;
            lblSoLuong.TextAlign = ContentAlignment.MiddleRight;
            content.Controls.Add(lblSoLuong);
            var thanhTieuDe = new Panel { Dock = DockStyle.Top, Height = 38 };
            lblTrangThaiTong.Dock = DockStyle.Right;
            lblTrangThaiTong.Width = 270;
            lblTrangThaiTong.TextAlign = ContentAlignment.MiddleRight;
            lblTrangThaiTong.ForeColor = Color.FromArgb(65, 95, 120);
            lblTrangThaiTong.Font = new Font("Segoe UI", 10);
            thanhTieuDe.Controls.Add(lblTrangThaiTong);
            thanhTieuDe.Controls.Add(new Label
            {
                Text = "DANH SÁCH PHÒNG HỌC",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83),
                TextAlign = ContentAlignment.MiddleLeft
            });
            content.Controls.Add(thanhTieuDe);
            layout.Controls.Add(content, 0, 3);

            dgv.Dock = DockStyle.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.ReadOnly = true;
            dgv.AutoGenerateColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 42;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 52, 83);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(210, 230, 250);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
            ThemCot("STT", "STT", 45);
            ThemCot("MaPhongHienThi", "Mã phòng", 75);
            ThemCot("TenPhong", "Tên phòng", 200);
            ThemCot("SucChua", "Sức chứa", 100);
            ThemCot("TrangThaiText", "Trạng thái", 120);
            dgv.SelectionChanged += ChonDong;
        }

        private void ThemCot(string ma, string ten, float rong)
            => dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ma,
                HeaderText = ten,
                DataPropertyName = ma,
                FillWeight = rong
            });

        private void NapDanhSach(string tuKhoa = "")
        {
            // Trạng thái được chuyển sang chữ TRƯỚC KHI bind vào DataGridView,
            // không sử dụng CellFormatting (tránh sai kiểu dữ liệu).
            var danhSach = string.IsNullOrWhiteSpace(tuKhoa)
                ? bll.LayDanhSach() : bll.TimKiem(tuKhoa);

            int dangDung = danhSach.Count(p => p.TrangThai);
            int ngung = danhSach.Count - dangDung;
            var daLoc = cboLocTrangThai.SelectedIndex switch
            {
                1 => danhSach.Where(p => p.TrangThai),
                2 => danhSach.Where(p => !p.TrangThai),
                _ => danhSach.AsEnumerable()
            };
            var data = daLoc.Select((p, index) => new DongPhongHoc
            {
                STT = index + 1,
                MaPhong = p.MaPhong,
                MaPhongHienThi = DinhDangMaPhong(p.MaPhong),
                TenPhong = p.TenPhong,
                SucChua = p.SucChua,
                TrangThaiText = p.TrangThai ? "Đang dùng" : "Ngừng"
            }).ToList();

            dangNap = true;
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = data;
                dgv.ClearSelection();
                XoaNhap();
                lblTrangThaiTong.Text = $"Đang dùng: {dangDung}   |   Ngừng: {ngung}";
                lblSoLuong.Text = $"Hiển thị: {data.Count} / {danhSach.Count} phòng học";
            }
            finally { dangNap = false; }
        }

        private PhongHoc LayNhap() => new()
        {
            TenPhong = txtTenPhong.Text,
            SucChua = decimal.ToInt32(nudSucChua.Value),
            TrangThai = chkTrangThai.Checked
        };

        private void Them()
        {
            int ma = bll.Them(LayNhap());
            MessageBox.Show($"Thêm phòng thành công! Mã phòng: {DinhDangMaPhong(ma)}",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LamMoi();
        }

        private void Sua()
        {
            if (!maDangChon.HasValue)
                throw new ArgumentException("Vui lòng chọn phòng cần sửa.");
            var data = LayNhap();
            data.MaPhong = maDangChon.Value;
            if (!bll.Sua(data))
                throw new InvalidOperationException("Không tìm thấy phòng cần sửa.");
            MessageBox.Show("Cập nhật phòng thành công!");
            LamMoi();
        }

        private void Xoa()
        {
            if (!maDangChon.HasValue)
                throw new ArgumentException("Vui lòng chọn phòng cần xóa.");

            int maPhong = maDangChon.Value;
            if (bll.DaDuocSuDung(maPhong))
            {
                var xacNhan = MessageBox.Show(
                    "Phòng này đang được gán cho lớp hoặc lịch học, nên không được xóa vĩnh viễn.\n\n" +
                    "Bạn có muốn chuyển phòng sang NGỪNG SỬ DỤNG không?\n" +
                    "Các lớp/lịch đã lưu vẫn giữ phòng cũ, nhưng phòng sẽ không còn trong danh sách chọn mới.",
                    "Giữ lại dữ liệu liên kết", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (xacNhan != DialogResult.Yes) return;

                if (!bll.NgungSuDung(maPhong))
                    throw new InvalidOperationException("Không tìm thấy phòng cần cập nhật.");
                MessageBox.Show("Đã chuyển phòng sang Ngừng sử dụng.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var xacNhan = MessageBox.Show(
                    "Phòng chưa có lớp hoặc lịch học liên kết. Bạn chắc chắn muốn XÓA VĨNH VIỄN phòng này?",
                    "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (xacNhan != DialogResult.Yes) return;

                // DAL kiểm tra liên kết một lần nữa, phòng trường hợp vừa có lớp/lịch mới.
                if (!bll.Xoa(maPhong))
                    throw new InvalidOperationException("Không tìm thấy phòng cần xóa.");
                MessageBox.Show("Đã xóa phòng không có dữ liệu liên quan.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            LamMoi();
        }

        private void LamMoi()
        {
            txtTimKiem.Clear();
            NapDanhSach();
        }

        private void ChonDong(object? sender, EventArgs e)
        {
            if (dangNap || dgv.SelectedRows.Count == 0) return;
            if (dgv.SelectedRows[0].DataBoundItem is not DongPhongHoc dong) return;
            var p = bll.LayTheoMa(dong.MaPhong);
            if (p == null) return;
            maDangChon = p.MaPhong;
            txtMaPhong.Text = DinhDangMaPhong(p.MaPhong);
            txtTenPhong.Text = p.TenPhong;
            nudSucChua.Value = Math.Clamp(p.SucChua, 1, 200);
            chkTrangThai.Checked = p.TrangThai;
        }

        private void XoaNhap()
        {
            maDangChon = null;
            txtMaPhong.Clear();
            txtTenPhong.Clear();
            nudSucChua.Value = 30;
            chkTrangThai.Checked = true;
        }

        private static void XuLyAnToan(Action action)
        {
            try { action(); }
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
                MessageBox.Show("Lỗi truy cập dữ liệu:\n" + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
