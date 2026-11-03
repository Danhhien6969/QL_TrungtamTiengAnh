using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmLichHoc : Form
    {
        private readonly LichHocBLL bll = new();
        private int? maDangChon;
        private bool dangNap;

        private readonly ComboBox cboLopHoc = new();
        private readonly ComboBox cboPhongHoc = new();
        private readonly TextBox txtGiaoVien = new();
        private readonly CheckBox chkDieuChinhNgay = new();
        private readonly DateTimePicker dtpBatDau = new();
        private readonly DateTimePicker dtpKetThuc = new();
        private readonly DateTimePicker dtpGioBatDau = new();
        private readonly DateTimePicker dtpGioKetThuc = new();
        private readonly TextBox txtTimKiem = new();
        private readonly DataGridView dgv = new();
        private readonly Label lblSoLuong = new();
        private readonly Label lblXemTruoc = new();
        private readonly List<CheckBox> cacThu = new();

        private sealed class DongLichHoc
        {
            public int MaLH { get; init; }
            public int STT { get; init; }
            public string TenLop { get; init; } = "";
            public string TenGiaoVien { get; init; } = "";
            public string TenPhong { get; init; } = "";
            public string Thu { get; init; } = "";
            public string CaHoc { get; init; } = "";
            public string KhoangApDung { get; init; } = "";
            public int SoBuoiDuKien { get; init; }
        }

        public frmLichHoc()
        {
            InitializeComponent();
            TaoGiaoDien();
            Shown += (_, _) => XuLyAnToan(() =>
            {
                NapDanhMuc();
                NapDanhSach();
            });
        }

        private static string TenThu(int thu) => thu switch
        {
            2 => "Thứ Hai",
            3 => "Thứ Ba",
            4 => "Thứ Tư",
            5 => "Thứ Năm",
            6 => "Thứ Sáu",
            7 => "Thứ Bảy",
            8 => "Chủ Nhật",
            _ => $"Thứ {thu}"
        };

        private static int QuyDoiThu(DateTime ngay)
            => ngay.DayOfWeek == DayOfWeek.Sunday ? 8 : (int)ngay.DayOfWeek + 1;

        private static Button TaoNut(string ten, Color mau, int rong = 102)
        {
            var nut = new Button
            {
                Text = ten,
                Width = rong,
                Height = 37,
                FlatStyle = FlatStyle.Flat,
                BackColor = mau,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Margin = new Padding(0, 0, 8, 0)
            };
            nut.FlatAppearance.BorderSize = 0;
            return nut;
        }

        private static void ThemTruong(TableLayoutPanel bang,
            string nhan, Control control, int hang, int nua)
        {
            int cot = nua * 2;
            var label = new Label
            {
                Text = nhan,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(52, 67, 84),
                Font = new Font("Segoe UI", 9.5f)
            };
            bang.Controls.Add(label, cot, hang);
            control.Dock = DockStyle.Fill;
            control.Font = new Font("Segoe UI", 10);
            control.Margin = new Padding(4, 6, 12, 6);
            bang.Controls.Add(control, cot + 1, hang);
        }

        private void TaoGiaoDien()
        {
            Text = "Quản lý lịch học";
            ClientSize = new Size(1210, 770);
            MinimumSize = new Size(1040, 670);
            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);
            Controls.Clear();

            var mauXanh = Color.FromArgb(23, 52, 83);
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                // 5 vùng độc lập để phần xem trước không bị ép vào cuối GroupBox.
                RowCount = 5,
                Padding = new Padding(16, 12, 16, 12)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));   // Tiêu đề
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 300));  // Nhập lịch
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 105));  // Xem trước
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));   // Nút thao tác
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Danh sách
            Controls.Add(layout);

            var header = new Panel { Dock = DockStyle.Fill };
            header.Controls.Add(new Label
            {
                Text = "QUẢN LÝ LỊCH HỌC",
                AutoSize = true,
                Location = new Point(5, 1),
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                ForeColor = mauXanh
            });
            header.Controls.Add(new Label
            {
                Text = "Chọn lớp → chọn các thứ trong tuần → xem trước buổi học → lưu lịch",
                AutoSize = true,
                Location = new Point(8, 42),
                ForeColor = Color.DimGray
            });
            layout.Controls.Add(header, 0, 0);

            var grp = new GroupBox
            {
                Text = "THIẾT LẬP LỊCH HỌC HẰNG TUẦN",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = mauXanh,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10)
            };
            layout.Controls.Add(grp, 0, 1);

            var bang = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 5,
                Padding = new Padding(7, 10, 5, 5)
            };
            bang.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 137));
            bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            bang.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 133));
            bang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 4; i++)
                bang.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
            bang.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            grp.Controls.Add(bang);

            txtGiaoVien.ReadOnly = true;
            txtGiaoVien.BackColor = Color.FromArgb(240, 243, 247);
            cboLopHoc.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPhongHoc.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPhongHoc.Enabled = false; // Phòng được gán duy nhất tại Quản lý lớp học.

            dtpBatDau.Enabled = false;
            dtpKetThuc.Enabled = false;
            dtpBatDau.Format = DateTimePickerFormat.Custom;
            dtpBatDau.CustomFormat = "dd/MM/yyyy";
            dtpKetThuc.Format = DateTimePickerFormat.Custom;
            dtpKetThuc.CustomFormat = "dd/MM/yyyy";
            foreach (var dtp in new[] { dtpGioBatDau, dtpGioKetThuc })
            {
                dtp.Format = DateTimePickerFormat.Custom;
                dtp.CustomFormat = "HH:mm";
                dtp.ShowUpDown = true;
            }

            ThemTruong(bang, "Lớp học (*)", cboLopHoc, 0, 0);
            ThemTruong(bang, "Giáo viên", txtGiaoVien, 0, 1);
            ThemTruong(bang, "Phòng học (*)", cboPhongHoc, 1, 0);
            ThemTruong(bang, "Áp dụng từ", dtpBatDau, 2, 0);
            ThemTruong(bang, "Áp dụng đến", dtpKetThuc, 2, 1);
            ThemTruong(bang, "Giờ bắt đầu", dtpGioBatDau, 3, 0);
            ThemTruong(bang, "Giờ kết thúc", dtpGioKetThuc, 3, 1);

            var chonThu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Margin = new Padding(0, 2, 0, 0)
            };
            chonThu.Controls.Add(new Label
            {
                Text = "Ngày học mỗi tuần (*)",
                AutoSize = false,
                Width = 169,
                Height = 35,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(52, 67, 84)
            });
            for (int thu = 2; thu <= 8; thu++)
            {
                var o = new CheckBox
                {
                    Tag = thu,
                    Text = TenThu(thu),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.5f),
                    Margin = new Padding(9, 9, 10, 0)
                };
                o.CheckedChanged += (_, _) => CapNhatXemTruoc();
                cacThu.Add(o);
                chonThu.Controls.Add(o);
            }
            bang.Controls.Add(chonThu, 0, 4);
            bang.SetColumnSpan(chonThu, 4);

            // Khung xem trước ĐỘC LẬP (không đặt ở hàng cuối của bang).
            // Khi GroupBox co giãn, nội dung xem trước không còn bị cắt mất chữ.
            var khungXemTruoc = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.FromArgb(236, 244, 251),
                Margin = new Padding(0, 2, 0, 3),
                Padding = new Padding(14, 6, 10, 6)
            };
            khungXemTruoc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            khungXemTruoc.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 248));
            khungXemTruoc.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            khungXemTruoc.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            chkDieuChinhNgay.Text = "Điều chỉnh thời gian áp dụng";
            chkDieuChinhNgay.AutoSize = true;
            chkDieuChinhNgay.Font = new Font("Segoe UI", 9f);
            chkDieuChinhNgay.Dock = DockStyle.Right;
            chkDieuChinhNgay.Margin = new Padding(0, 0, 4, 0);
            chkDieuChinhNgay.CheckedChanged += (_, _) =>
            {
                dtpBatDau.Enabled = chkDieuChinhNgay.Checked;
                dtpKetThuc.Enabled = chkDieuChinhNgay.Checked;
                if (!chkDieuChinhNgay.Checked && !dangNap)
                {
                    if (cboLopHoc.SelectedItem is LopHoc lop)
                    {
                        dtpBatDau.Value = lop.NgayBatDau.Date;
                        dtpKetThuc.Value = lop.NgayKetThuc.Date;
                    }
                }
                CapNhatXemTruoc();
            };
            khungXemTruoc.Controls.Add(chkDieuChinhNgay, 1, 0);
            khungXemTruoc.Controls.Add(new Label
            {
                Text = "XEM TRƯỚC LỊCH HỌC",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = Padding.Empty,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = mauXanh
            }, 0, 0);

            lblXemTruoc.Dock = DockStyle.Fill;
            lblXemTruoc.AutoSize = false;
            lblXemTruoc.AutoEllipsis = true;
            lblXemTruoc.Margin = Padding.Empty;
            lblXemTruoc.Padding = Padding.Empty;
            lblXemTruoc.BackColor = Color.FromArgb(236, 244, 251);
            lblXemTruoc.ForeColor = mauXanh;
            lblXemTruoc.Font = new Font("Segoe UI", 9.5f);
            lblXemTruoc.TextAlign = ContentAlignment.MiddleLeft;
            khungXemTruoc.Controls.Add(lblXemTruoc, 0, 1);
            khungXemTruoc.SetColumnSpan(lblXemTruoc, 2);
            layout.Controls.Add(khungXemTruoc, 0, 2);

            var tools = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            var btnThem = TaoNut("Tạo lịch", Color.FromArgb(27, 139, 92), 110);
            var btnSua = TaoNut("Sửa", Color.FromArgb(40, 110, 190));
            var btnXoa = TaoNut("Xóa", Color.FromArgb(196, 66, 73));
            var btnMoi = TaoNut("Làm mới", Color.FromArgb(96, 110, 126));
            var btnTim = TaoNut("Tìm kiếm", mauXanh, 108);
            txtTimKiem.Width = 205;
            txtTimKiem.Height = 34;
            txtTimKiem.PlaceholderText = "Tên lớp, phòng, giáo viên...";
            txtTimKiem.Font = new Font("Segoe UI", 10);
            txtTimKiem.Margin = new Padding(12, 5, 8, 0);
            tools.Controls.AddRange(new Control[]
            {
                btnThem, btnSua, btnXoa, btnMoi, txtTimKiem, btnTim
            });
            layout.Controls.Add(tools, 0, 3);

            btnThem.Click += (_, _) => XuLyAnToan(Them);
            btnSua.Click += (_, _) => XuLyAnToan(Sua);
            btnXoa.Click += (_, _) => XuLyAnToan(Xoa);
            btnMoi.Click += (_, _) => XuLyAnToan(LamMoi);
            btnTim.Click += (_, _) => XuLyAnToan(() => NapDanhSach(txtTimKiem.Text));
            txtTimKiem.KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    XuLyAnToan(() => NapDanhSach(txtTimKiem.Text));
                }
            };

            // Bố cục 3 hàng rõ ràng, tránh DataGridView che tiêu đề hoặc tổng số.
            var content = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            content.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            content.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.Controls.Add(content, 0, 4);
            content.Controls.Add(new Label
            {
                Text = "DANH SÁCH LỊCH CỐ ĐỊNH HẰNG TUẦN",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = mauXanh,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            content.Controls.Add(dgv, 0, 1);
            lblSoLuong.Dock = DockStyle.Fill;
            lblSoLuong.TextAlign = ContentAlignment.MiddleRight;
            lblSoLuong.ForeColor = Color.FromArgb(70, 85, 100);
            content.Controls.Add(lblSoLuong, 0, 2);

            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AutoGenerateColumns = false;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.MultiSelect = false;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.None;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 42;
            dgv.RowTemplate.Height = 33;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = mauXanh;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(213, 232, 249);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(246, 249, 252);
            ThemCot("STT", "STT", 46);
            ThemCot("TenLop", "Lớp", 138);
            ThemCot("TenGiaoVien", "Giáo viên", 147);
            ThemCot("TenPhong", "Phòng", 89);
            ThemCot("Thu", "Ngày học", 91);
            ThemCot("CaHoc", "Giờ học", 99);
            ThemCot("KhoangApDung", "Khoảng áp dụng", 173);
            ThemCot("SoBuoiDuKien", "Số buổi", 69);
            dgv.SelectionChanged += ChonDong;

            cboLopHoc.SelectedValueChanged += (_, _) =>
            {
                if (dangNap) return;
                if (maDangChon == null) DongBoTheoLop();
                else CapNhatGiaoVien();
                CapNhatXemTruoc();
            };
            foreach (var dtp in new[]
            {
                dtpBatDau, dtpKetThuc, dtpGioBatDau, dtpGioKetThuc
            })
                dtp.ValueChanged += (_, _) => CapNhatXemTruoc();
        }

        private void ThemCot(string ten, string nhan, float tyLe)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = ten,
                HeaderText = nhan,
                DataPropertyName = ten,
                FillWeight = tyLe,
                SortMode = DataGridViewColumnSortMode.NotSortable
            });
        }

        private void NapDanhMuc()
        {
            dangNap = true;
            try
            {
                cboLopHoc.DisplayMember = nameof(LopHoc.TenLop);
                cboLopHoc.ValueMember = nameof(LopHoc.MaLop);
                cboLopHoc.DataSource = bll.LayLopHocs();
                cboLopHoc.SelectedIndex = -1;

                cboPhongHoc.DisplayMember = nameof(PhongHoc.TenPhong);
                cboPhongHoc.ValueMember = nameof(PhongHoc.MaPhong);
                cboPhongHoc.DataSource = bll.LayPhongHocs();
                cboPhongHoc.SelectedIndex = -1;
            }
            finally { dangNap = false; }
        }

        private void DongBoTheoLop()
        {
            if (cboLopHoc.SelectedItem is not LopHoc lop)
            {
                txtGiaoVien.Clear();
                cboPhongHoc.SelectedIndex = -1;
                return;
            }
            txtGiaoVien.Text = lop.GiaoVien?.HoTen ?? "";
            chkDieuChinhNgay.Checked = false;
            dtpBatDau.Value = lop.NgayBatDau.Date;
            dtpKetThuc.Value = lop.NgayKetThuc.Date;
            // Khi chưa có lịch, ngày kết thúc sẽ tự tính theo số buổi và ngày học.
            if (!bll.LayDanhSach().Any(l => l.MaLop == lop.MaLop))
                TuTinhNgayKetThuc();
            if (lop.MaPhongMacDinh.HasValue &&
                cboPhongHoc.Items.Cast<PhongHoc>()
                    .Any(p => p.MaPhong == lop.MaPhongMacDinh.Value))
                cboPhongHoc.SelectedValue = lop.MaPhongMacDinh.Value;
            else
                cboPhongHoc.SelectedIndex = -1;
        }

        private int LaySoBuoiCuaKhoa()
        {
            if (cboLopHoc.SelectedItem is not LopHoc lop)
                return 0;
            // Lấy KhoaHoc từ cơ sở dữ liệu để luôn đọc đúng số buổi hiện tại.
            using var db = new QL_TrungtamTA.DAL.Data.TrungTamDbContext();
            return db.KhoaHocs.Where(k => k.MaKH == lop.MaKH)
                .Select(k => k.SoBuoi).FirstOrDefault() ?? 0;
        }

        private void TuTinhNgayKetThuc()
        {
            if (dangNap || chkDieuChinhNgay.Checked || maDangChon.HasValue) return;
            var thu = LayCacThuDaChon();
            int soBuoi = LaySoBuoiCuaKhoa();
            if (thu.Count == 0 || soBuoi <= 0) return;
            DateTime ngayKetThuc = LichHocBLL.TinhNgayKetThuc(
                dtpBatDau.Value.Date, thu, soBuoi);
            if (dtpKetThuc.Value.Date != ngayKetThuc)
                dtpKetThuc.Value = ngayKetThuc;
        }

        private void CapNhatGiaoVien()
        {
            txtGiaoVien.Text = cboLopHoc.SelectedItem is LopHoc lop
                ? lop.GiaoVien?.HoTen ?? "" : "";
        }

        private List<int> LayCacThuDaChon()
            => cacThu.Where(c => c.Checked).Select(c => (int)c.Tag!).OrderBy(x => x).ToList();

        private void ChonMotThu(int thu)
        {
            foreach (var o in cacThu)
                o.Checked = (int)o.Tag! == thu;
        }

        // Đây là số buổi DỰ KIẾN theo quy tắc lặp, không phải BuoiHoc đã điểm danh.
        private void CapNhatXemTruoc()
        {
            if (dangNap) return;
            var cacNgay = LayCacThuDaChon();
            if (cboLopHoc.SelectedItem is not LopHoc lop)
            {
                lblXemTruoc.Text = "Bước 1: Chọn lớp học để tự lấy giáo viên, thời gian lớp và phòng mặc định.";
                return;
            }
            if (!lop.MaPhongMacDinh.HasValue ||
                !cboPhongHoc.Items.Cast<PhongHoc>().Any(p => p.MaPhong == lop.MaPhongMacDinh.Value))
            {
                lblXemTruoc.Text = $"Lớp {lop.TenLop} chưa có phòng chính đang hoạt động. Hãy chọn phòng trong Quản lý lớp học trước khi tạo lịch.";
                return;
            }
            if (cacNgay.Count == 0)
            {
                lblXemTruoc.Text = $"Lớp: {lop.TenLop}. Bước 2: Chọn ít nhất một thứ trong tuần để xem trước lịch.";
                return;
            }
            if (!chkDieuChinhNgay.Checked) TuTinhNgayKetThuc();
            var tu = dtpBatDau.Value.Date;
            var den = dtpKetThuc.Value.Date;
            if (den < tu)
            {
                lblXemTruoc.Text = "Khoảng ngày chưa hợp lệ: ngày kết thúc đang trước ngày bắt đầu.";
                return;
            }
            int tong = cacNgay.Sum(t => LichHocBLL.DemSoBuoi(t, tu, den));
            var gio = $"{dtpGioBatDau.Value:HH:mm}–{dtpGioKetThuc.Value:HH:mm}";
            var thu = string.Join(", ", cacNgay.Select(TenThu));
            var buoiDau = new List<DateTime>();
            for (var ngay = tu; ngay <= den && buoiDau.Count < 4; ngay = ngay.AddDays(1))
                if (cacNgay.Contains(QuyDoiThu(ngay))) buoiDau.Add(ngay);
            string ganNhat = buoiDau.Count > 0
                ? string.Join("; ", buoiDau.Select(n => n.ToString("dd/MM/yyyy")))
                : "Không có ngày nào khớp";
            int yeuCau = LaySoBuoiCuaKhoa();
            string chiTieu = yeuCau > 0 ? $" / {yeuCau} buổi theo chứng chỉ" : " (chứng chỉ chưa có số buổi)";
            lblXemTruoc.Text = $"{thu}  |  {gio}  |  Dự kiến {tong}{chiTieu}.\n" +
                $"Những buổi đầu tiên: {ganNhat}. " +
                "Lưu ý: số buổi dự kiến chưa trừ ngày lễ/nghỉ.";
        }

        private void NapDanhSach(string tuKhoa = "")
        {
            var raw = string.IsNullOrWhiteSpace(tuKhoa)
                ? bll.LayDanhSach() : bll.TimKiem(tuKhoa);
            var data = raw.Select((l, i) => new DongLichHoc
            {
                MaLH = l.MaLH,
                STT = i + 1,
                TenLop = l.LopHoc?.TenLop ?? "",
                TenGiaoVien = l.LopHoc?.GiaoVien?.HoTen ?? "",
                TenPhong = l.PhongHoc?.TenPhong ?? "",
                Thu = TenThu(l.Thu),
                CaHoc = $"{l.GioBatDau.ToString(@"hh\:mm")}–{l.GioKetThuc.ToString(@"hh\:mm")}",
                KhoangApDung = $"{l.NgayBatDau:dd/MM/yyyy} - {l.NgayKetThuc:dd/MM/yyyy}",
                SoBuoiDuKien = LichHocBLL.DemSoBuoi(l.Thu, l.NgayBatDau, l.NgayKetThuc)
            }).ToList();
            dangNap = true;
            try
            {
                dgv.DataSource = null;
                dgv.DataSource = data;
                dgv.ClearSelection();
                lblSoLuong.Text = $"{data.Count} quy tắc lịch học hàng tuần";
            }
            finally { dangNap = false; }
            XoaNhap();
        }

        private LichHoc TaoLich(int thu)
        {
            if (cboLopHoc.SelectedItem is not LopHoc lop)
                throw new ArgumentException("Vui lòng chọn lớp học.");
            if (!lop.MaPhongMacDinh.HasValue || lop.MaPhongMacDinh.Value <= 0)
                throw new InvalidOperationException($"Lớp {lop.TenLop} chưa được phân phòng. Hãy vào Quản lý lớp học để chọn phòng trước.");
            int maPhong = lop.MaPhongMacDinh.Value;
            if (!cboPhongHoc.Items.Cast<PhongHoc>().Any(p => p.MaPhong == maPhong))
                throw new InvalidOperationException("Phòng chính của lớp đã ngừng hoạt động hoặc không tồn tại.");
            int maLop = lop.MaLop;
            return new LichHoc
            {
                MaLop = maLop,
                MaPhong = maPhong,
                Thu = thu,
                NgayBatDau = dtpBatDau.Value.Date,
                NgayKetThuc = dtpKetThuc.Value.Date,
                GioBatDau = dtpGioBatDau.Value.TimeOfDay,
                GioKetThuc = dtpGioKetThuc.Value.TimeOfDay
            };
        }

        private void Them()
        {
            if (maDangChon.HasValue)
                throw new ArgumentException(
                    "Bạn đang sửa một lịch. Nhấn Làm mới trước khi tạo một bộ lịch mới.");
            var thu = LayCacThuDaChon();
            if (thu.Count == 0)
                throw new ArgumentException("Vui lòng chọn ít nhất một thứ học trong tuần.");
            var cacLich = thu.Select(TaoLich).ToList();
            int tongBuoi = cacLich.Sum(l => LichHocBLL.DemSoBuoi(
                l.Thu, l.NgayBatDau, l.NgayKetThuc));
            if (MessageBox.Show(
                $"Tạo {cacLich.Count} quy tắc lịch học ({tongBuoi} buổi dự kiến)?\n" +
                "Hệ thống sẽ kiểm tra trùng lịch trước khi lưu.",
                "Xác nhận xếp lịch", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (chkDieuChinhNgay.Checked)
                throw new InvalidOperationException(
                    "Để tự tính đủ số buổi, bỏ chọn 'Điều chỉnh thời gian áp dụng' khi tạo lịch đầu tiên.");
            int soBuoi = LaySoBuoiCuaKhoa();
            if (soBuoi <= 0)
                throw new InvalidOperationException("Chứng chỉ chưa có Số buổi. Hãy cập nhật ở Quản lý khóa học.");
            int dem = bll.TaoLichTuTinhNgayKetThuc(cacLich, soBuoi);
            MessageBox.Show($"Ngày kết thúc lớp đã tự cập nhật: {cacLich[0].NgayKetThuc:dd/MM/yyyy}.\n" +
                $"Đã tạo {dem} lịch hàng tuần, " +
                $"tương ứng {tongBuoi} buổi dự kiến.\n" +
                "Đây là lịch kế hoạch, chưa tự tạo Buổi học/Điểm danh.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LamMoi();
        }

        private void Sua()
        {
            if (!maDangChon.HasValue)
                throw new ArgumentException("Chọn một dòng lịch trong bảng để sửa.");
            var thu = LayCacThuDaChon();
            if (thu.Count != 1)
                throw new ArgumentException(
                    "Khi sửa một dòng lịch, bạn chỉ được chọn một thứ. " +
                    "Để tạo nhiều thứ, nhấn Làm mới và Tạo lịch.");
            var lich = TaoLich(thu[0]);
            lich.MaLH = maDangChon.Value;
            if (!bll.Sua(lich))
                throw new InvalidOperationException("Không tìm thấy lịch cần sửa.");
            MessageBox.Show("Đã cập nhật quy tắc lịch học.");
            LamMoi();
        }

        private void Xoa()
        {
            if (!maDangChon.HasValue)
                throw new ArgumentException("Vui lòng chọn lịch cần xóa.");
            if (MessageBox.Show(
                "Chỉ xóa quy tắc lịch của dòng đã chọn; " +
                "những buổi học/điểm danh đã tạo sẽ không bị xóa.\n" +
                "Bạn muốn tiếp tục?",
                "Xác nhận xóa lịch", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (!bll.Xoa(maDangChon.Value))
                throw new InvalidOperationException("Không tìm thấy lịch cần xóa.");
            MessageBox.Show("Đã xóa quy tắc lịch.");
            LamMoi();
        }

        private void LamMoi()
        {
            txtTimKiem.Clear();
            NapDanhMuc();
            NapDanhSach();
        }

        private void ChonDong(object? sender, EventArgs e)
        {
            if (dangNap || dgv.SelectedRows.Count == 0) return;
            if (dgv.SelectedRows[0].DataBoundItem is not DongLichHoc dong) return;
            XuLyAnToan(() =>
            {
                var lich = bll.LayTheoMa(dong.MaLH);
                if (lich == null) return;
                dangNap = true;
                try
                {
                    maDangChon = lich.MaLH;
                    cboLopHoc.SelectedValue = lich.MaLop;
                    CapNhatGiaoVien();
                    // Hiển thị phòng chính thay vì cho chọn phòng trong lịch.
                    if (cboLopHoc.SelectedItem is LopHoc lopChon &&
                        lopChon.MaPhongMacDinh.HasValue &&
                        cboPhongHoc.Items.Cast<PhongHoc>().Any(p => p.MaPhong == lopChon.MaPhongMacDinh.Value))
                        cboPhongHoc.SelectedValue = lopChon.MaPhongMacDinh.Value;
                    else
                        cboPhongHoc.SelectedIndex = -1;
                    dtpBatDau.Value = lich.NgayBatDau;
                    dtpKetThuc.Value = lich.NgayKetThuc;
                    // Chỉ bật chỉnh tay nếu lịch đã lưu khác khoảng ngày của lớp.
                    if (cboLopHoc.SelectedItem is LopHoc lop)
                        chkDieuChinhNgay.Checked =
                            lich.NgayBatDau.Date != lop.NgayBatDau.Date ||
                            lich.NgayKetThuc.Date != lop.NgayKetThuc.Date;
                    else
                        chkDieuChinhNgay.Checked = false;
                    dtpGioBatDau.Value = DateTime.Today.Add(lich.GioBatDau);
                    dtpGioKetThuc.Value = DateTime.Today.Add(lich.GioKetThuc);
                    ChonMotThu(lich.Thu);
                }
                finally { dangNap = false; }
                CapNhatXemTruoc();
            });
        }

        private void XoaNhap()
        {
            dangNap = true;
            try
            {
                maDangChon = null;
                txtGiaoVien.Clear();
                cboLopHoc.SelectedIndex = -1;
                cboPhongHoc.SelectedIndex = -1;
                foreach (var c in cacThu) c.Checked = false;
                chkDieuChinhNgay.Checked = false;
                dtpBatDau.Value = DateTime.Today;
                dtpKetThuc.Value = DateTime.Today.AddMonths(3);
                dtpGioBatDau.Value = DateTime.Today.AddHours(18);
                dtpGioKetThuc.Value = DateTime.Today.AddHours(20);
            }
            finally { dangNap = false; }
            CapNhatXemTruoc();
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
