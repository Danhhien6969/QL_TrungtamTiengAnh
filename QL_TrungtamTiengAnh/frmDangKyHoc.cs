
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
    public partial class frmDangKyHoc : Form
    {
        private readonly DangKyHocBLL bll = new();

        private readonly TextBox txtMaDK = new();
        private readonly ComboBox cboLopHoc = new();
        private readonly TextBox txtHocPhi = new();
        private readonly TextBox txtNgayDangKy = new();
        private readonly TextBox txtTrangThai = new();

        private readonly TextBox txtTimHocVien = new();
        private readonly CheckedListBox clbHocVien = new();
        private readonly CheckBox chkChonTatCa = new();
        private readonly Label lblDaChon = new();
        private readonly Label lblGoiY = new();

        private readonly TextBox txtTimKiem = new();
        private readonly DataGridView dgv = new();
        private readonly Label lblSoLuong = new();

        private readonly Button btnThem = new();
        private readonly Button btnHoanThanh = new();
        private readonly Button btnHuy = new();
        private readonly Button btnKhoiPhuc = new();
        private readonly Button btnLamMoi = new();
        private readonly Button btnTimKiem = new();

        private readonly List<LuaChonHocVien> danhSachHocVien = new();
        private readonly HashSet<int> maHocVienDaChon = new();

        // Học viên đã có đăng ký còn hiệu lực.
        // Không cho xuất hiện trong danh sách chọn.
        private readonly HashSet<int> maHocVienDaCoLop = new();

        private int? maDangChon;
        private bool dangNap;
        private bool dangCapNhatTichChon;

        public frmDangKyHoc()
        {
            InitializeComponent();
            TaoGiaoDien();

            Shown += (_, _) =>
                XuLyAnToan(LamMoi);
        }

        // ========================================
        // 1. HÀM HỖ TRỢ
        // ========================================

        private static void CauHinhNut(
            Button nut,
            string noiDung,
            Color mau,
            int rong)
        {
            nut.Text = noiDung;
            nut.Width = rong;
            nut.Height = 39;
            nut.BackColor = mau;
            nut.ForeColor = Color.White;
            nut.FlatStyle = FlatStyle.Flat;
            nut.FlatAppearance.BorderSize = 0;
            nut.Font = new Font(
                "Segoe UI", 10, FontStyle.Bold);
            nut.Cursor = Cursors.Hand;
            nut.Margin = new Padding(0, 0, 8, 5);
        }

        private static void ThemDong(
            TableLayoutPanel bang,
            string nhan,
            Control input,
            int dong)
        {
            bang.Controls.Add(new Label
            {
                Text = nhan,
                Dock = DockStyle.Fill,
                AutoSize = false,
                Font = new Font(
                    "Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.FromArgb(45, 55, 70),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, dong);

            input.Dock = DockStyle.Fill;
            input.Margin = new Padding(4, 6, 8, 6);
            input.Font = new Font(
                "Segoe UI", 10, FontStyle.Regular);

            bang.Controls.Add(input, 1, dong);
        }

        // Một đăng ký chưa hủy được xem là đang chiếm lớp.
        private static bool LaDangKyConHieuLuc(
            string? trangThai)
        {
            return !string.Equals(
                trangThai?.Trim(),
                "DaHuy",
                StringComparison.OrdinalIgnoreCase);
        }

        private static string TenTrangThai(string? trangThai)
        {
            return trangThai switch
            {
                "DangHoc" => "Đang học",
                "HoanThanh" => "Hoàn thành",
                "DaHuy" => "Đã hủy",
                _ => trangThai ?? ""
            };
        }

        // ========================================
        // 2. THIẾT KẾ GIAO DIỆN
        // ========================================

        private void TaoGiaoDien()
        {
            Text = "Quản lý đăng ký học";
            ClientSize = new Size(1180, 780);
            MinimumSize = new Size(950, 680);
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

            layout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 73));
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 285));
            layout.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 65));
            layout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            Controls.Add(layout);

            // TIÊU ĐỀ

            var header = new Panel
            {
                Dock = DockStyle.Fill
            };

            header.Controls.Add(new Label
            {
                Text = "QUẢN LÝ ĐĂNG KÝ HỌC",
                Location = new Point(5, 5),
                AutoSize = true,
                Font = new Font(
                    "Segoe UI", 21, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });

            header.Controls.Add(new Label
            {
                Text = "Đăng ký nhiều học viên chưa có lớp",
                Location = new Point(7, 47),
                AutoSize = true,
                ForeColor = Color.DimGray
            });

            layout.Controls.Add(header, 0, 0);

            // HAI KHUNG THÔNG TIN

            var khungThongTin = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };

            khungThongTin.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 43));

            khungThongTin.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 57));

            layout.Controls.Add(khungThongTin, 0, 1);

            // BÊN TRÁI: THÔNG TIN ĐĂNG KÝ

            var grpDangKy = new GroupBox
            {
                Text = "THÔNG TIN ĐĂNG KÝ",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(23, 52, 83),
                Font = new Font(
                    "Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10),
                Margin = new Padding(0, 0, 8, 0)
            };

            var bangThongTin = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(8)
            };

            bangThongTin.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 120));

            bangThongTin.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            for (int i = 0; i < 5; i++)
            {
                bangThongTin.RowStyles.Add(
                    new RowStyle(SizeType.Percent, 20));
            }

            foreach (var input in new[]
            {
                txtMaDK, txtHocPhi,
                txtNgayDangKy, txtTrangThai
            })
            {
                input.ReadOnly = true;
                input.BackColor =
                    Color.FromArgb(240, 243, 247);
            }

            cboLopHoc.DropDownStyle =
                ComboBoxStyle.DropDownList;

            txtHocPhi.TextAlign =
                HorizontalAlignment.Right;

            ThemDong(
                bangThongTin, "Mã đăng ký", txtMaDK, 0);
            ThemDong(
                bangThongTin, "Lớp học (*)", cboLopHoc, 1);
            ThemDong(
                bangThongTin, "Học phí (VNĐ)", txtHocPhi, 2);
            ThemDong(
                bangThongTin, "Ngày đăng ký", txtNgayDangKy, 3);
            ThemDong(
                bangThongTin, "Trạng thái", txtTrangThai, 4);

            grpDangKy.Controls.Add(bangThongTin);
            khungThongTin.Controls.Add(grpDangKy, 0, 0);

            // BÊN PHẢI: CHỌN NHIỀU HỌC VIÊN

            var grpHocVien = new GroupBox
            {
                Text = "CHỌN HỌC VIÊN CHƯA CÓ LỚP",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(23, 52, 83),
                Font = new Font(
                    "Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10),
                Margin = new Padding(8, 0, 0, 0)
            };

            var bangChon = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(8)
            };

            bangChon.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100));

            bangChon.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 38));

            bangChon.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 33));

            bangChon.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));

            bangChon.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 25));

            txtTimHocVien.Dock = DockStyle.Fill;
            txtTimHocVien.PlaceholderText =
                "Tìm học viên theo tên hoặc mã...";
            txtTimHocVien.Margin =
                new Padding(0, 1, 0, 5);

            bangChon.Controls.Add(txtTimHocVien, 0, 0);

            var thanhChon = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            chkChonTatCa.Text =
                "Chọn tất cả đang hiển thị";
            chkChonTatCa.AutoSize = true;
            chkChonTatCa.Font = new Font("Segoe UI", 9);

            lblDaChon.Text = "Đã chọn: 0 học viên";
            lblDaChon.AutoSize = true;
            lblDaChon.Font = new Font(
                "Segoe UI", 9, FontStyle.Bold);
            lblDaChon.ForeColor =
                Color.FromArgb(30, 110, 170);
            lblDaChon.Margin =
                new Padding(15, 4, 0, 0);

            thanhChon.Controls.Add(chkChonTatCa);
            thanhChon.Controls.Add(lblDaChon);

            bangChon.Controls.Add(thanhChon, 0, 1);

            clbHocVien.Dock = DockStyle.Fill;
            clbHocVien.CheckOnClick = true;
            clbHocVien.IntegralHeight = false;
            clbHocVien.BorderStyle =
                BorderStyle.FixedSingle;
            clbHocVien.Font = new Font("Segoe UI", 10);

            // Sửa lỗi hiển thị tên class.
            clbHocVien.DisplayMember =
                nameof(LuaChonHocVien.HienThi);

            clbHocVien.ValueMember =
                nameof(LuaChonHocVien.MaHV);

            bangChon.Controls.Add(clbHocVien, 0, 2);

            lblGoiY.Dock = DockStyle.Fill;
            lblGoiY.ForeColor =
                Color.FromArgb(80, 100, 120);
            lblGoiY.Font = new Font("Segoe UI", 8);
            lblGoiY.TextAlign =
                ContentAlignment.MiddleLeft;

            bangChon.Controls.Add(lblGoiY, 0, 3);

            grpHocVien.Controls.Add(bangChon);
            khungThongTin.Controls.Add(grpHocVien, 1, 0);

            // THANH NÚT CHỨC NĂNG

            var thanhNut = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(0, 10, 0, 0)
            };

            CauHinhNut(
                btnThem, "Đăng ký đã chọn",
                Color.FromArgb(30, 140, 95), 155);

            CauHinhNut(
                btnHoanThanh, "Hoàn thành",
                Color.FromArgb(40, 110, 190), 120);

            CauHinhNut(
                btnHuy, "Hủy đăng ký",
                Color.FromArgb(195, 65, 70), 128);

            CauHinhNut(
                btnKhoiPhuc, "Khôi phục",
                Color.FromArgb(90, 105, 150), 115);

            CauHinhNut(
                btnLamMoi, "Làm mới",
                Color.FromArgb(95, 110, 125), 112);

            CauHinhNut(
                btnTimKiem, "Tìm kiếm",
                Color.FromArgb(23, 52, 83), 112);

            txtTimKiem.Width = 180;
            txtTimKiem.Margin =
                new Padding(10, 5, 6, 5);
            txtTimKiem.PlaceholderText =
                "Học viên, lớp, khóa học...";

            thanhNut.Controls.AddRange(new Control[]
            {
                btnThem, btnHoanThanh,
                btnHuy, btnKhoiPhuc,
                btnLamMoi, txtTimKiem,
                btnTimKiem
            });

            layout.Controls.Add(thanhNut, 0, 2);

            // BẢNG ĐĂNG KÝ HỌC

            var panelBang = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            CauHinhBang();

            panelBang.Controls.Add(dgv);

            lblSoLuong.Dock = DockStyle.Bottom;
            lblSoLuong.Height = 30;
            lblSoLuong.TextAlign =
                ContentAlignment.MiddleRight;

            panelBang.Controls.Add(lblSoLuong);

            panelBang.Controls.Add(new Label
            {
                Text = "DANH SÁCH ĐĂNG KÝ HỌC",
                Dock = DockStyle.Top,
                Height = 36,
                Font = new Font(
                    "Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });

            layout.Controls.Add(panelBang, 0, 3);

            // =====================================
            // SỰ KIỆN
            // =====================================

            txtTimHocVien.TextChanged += (_, _) =>
            {
                if (!dangNap)
                    HienThiDanhSachHocVien();
            };

            clbHocVien.ItemCheck += ClbHocVien_ItemCheck;

            chkChonTatCa.CheckedChanged +=
                ChkChonTatCa_CheckedChanged;

            cboLopHoc.SelectedIndexChanged += (_, _) =>
            {
                if (!dangNap && !maDangChon.HasValue)
                    HienThiHocPhiDuKien();
            };

            btnThem.Click += (_, _) =>
                XuLyAnToan(ThemNhieuHocVien);

            btnHoanThanh.Click += (_, _) =>
                XuLyAnToan(HoanThanh);

            btnHuy.Click += (_, _) =>
                XuLyAnToan(Huy);

            btnKhoiPhuc.Click += (_, _) =>
                XuLyAnToan(KhoiPhuc);

            btnLamMoi.Click += (_, _) =>
                XuLyAnToan(LamMoi);

            btnTimKiem.Click += (_, _) =>
                XuLyAnToan(() =>
                    NapDanhSach(txtTimKiem.Text));

            txtTimKiem.KeyDown += (_, e) =>
            {
                if (e.KeyCode != Keys.Enter)
                    return;

                e.SuppressKeyPress = true;

                XuLyAnToan(() =>
                    NapDanhSach(txtTimKiem.Text));
            };

            XoaLuaChon();
        }

        // ========================================
        // 3. CẤU HÌNH BẢNG
        // ========================================

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

            dgv.MultiSelect = false;
            dgv.RowHeadersVisible = false;

            dgv.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgv.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 42;
            dgv.RowTemplate.Height = 36;

            dgv.ColumnHeadersDefaultCellStyle.BackColor =
                Color.FromArgb(23, 52, 83);

            dgv.ColumnHeadersDefaultCellStyle.ForeColor =
                Color.White;

            dgv.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10, FontStyle.Bold);

            dgv.DefaultCellStyle.SelectionBackColor =
                Color.FromArgb(210, 230, 250);

            dgv.DefaultCellStyle.SelectionForeColor =
                Color.Black;

            dgv.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(245, 248, 252);

            ThemCot("MaDK", "Mã ĐK", 65);
            ThemCot("TenHocVien", "Học viên", 150);
            ThemCot("TenLop", "Lớp học", 125);
            ThemCot("TenKhoaHoc", "Khóa học", 125);

            ThemCot(
                "NgayDangKy", "Ngày đăng ký",
                110, "dd/MM/yyyy");

            ThemCot(
                "HocPhiPhaiDong", "Học phí",
                110, "N0", true);

            ThemCot(
                "DaThanhToan", "Đã đóng",
                110, "N0", true);

            ThemCot(
                "ConNo", "Còn nợ",
                110, "N0", true);

            ThemCot("TrangThai", "Trạng thái", 95);

            dgv.SelectionChanged += Dgv_SelectionChanged;
        }

        private void ThemCot(
            string ten,
            string nhan,
            float tyLe,
            string? dinhDang = null,
            bool canPhai = false)
        {
            var cot = new DataGridViewTextBoxColumn
            {
                Name = ten,
                HeaderText = nhan,
                DataPropertyName = ten,
                FillWeight = tyLe,
                ReadOnly = true
            };

            if (dinhDang != null)
                cot.DefaultCellStyle.Format = dinhDang;

            if (canPhai)
            {
                cot.DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }

            dgv.Columns.Add(cot);
        }

        // ========================================
        // 4. KIỂM TRA HỌC VIÊN ĐÃ CÓ LỚP
        // ========================================

        private void CapNhatHocVienDaCoLop()
        {
            // Lấy TẤT CẢ đăng ký,
            // không giới hạn theo lớp đang chọn.
            var tatCaDangKy = bll.LayDanhSach();

            maHocVienDaCoLop.Clear();

            foreach (var dk in tatCaDangKy)
            {
                if (LaDangKyConHieuLuc(dk.TrangThai))
                {
                    maHocVienDaCoLop.Add(dk.MaHV);
                }
            }

            // Nếu học viên vừa có lớp,
            // tự loại khỏi các ô đã tích chọn.
            maHocVienDaChon.RemoveWhere(
                maHV => maHocVienDaCoLop.Contains(maHV));
        }

        // ========================================
        // 5. NẠP DANH MỤC
        // ========================================

        private void NapDanhMuc()
        {
            dangNap = true;

            try
            {
                danhSachHocVien.Clear();

                danhSachHocVien.AddRange(
                    bll.LayHocViens());

                // Phải kiểm tra trước khi hiển thị.
                CapNhatHocVienDaCoLop();

                cboLopHoc.DataSource = null;

                cboLopHoc.DisplayMember =
                    nameof(LuaChonLopHoc.HienThi);

                cboLopHoc.ValueMember =
                    nameof(LuaChonLopHoc.MaLop);

                cboLopHoc.DataSource =
                    bll.LayLopHocs();

                cboLopHoc.SelectedIndex = -1;

                HienThiDanhSachHocVien();
            }
            finally
            {
                dangNap = false;
            }
        }

        // ========================================
        // 6. CHỈ HIỂN THỊ HỌC VIÊN CHƯA CÓ LỚP
        // ========================================

        private void HienThiDanhSachHocVien()
        {
            string tuKhoa = txtTimHocVien.Text.Trim();

            var danhSachLoc = danhSachHocVien
                .Where(hv =>
                    // Điều kiện chính:
                    // chưa có đăng ký còn hiệu lực.
                    !maHocVienDaCoLop.Contains(hv.MaHV)
                    &&
                    hv.HienThi.Contains(
                        tuKhoa,
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

            dangCapNhatTichChon = true;
            clbHocVien.BeginUpdate();

            try
            {
                clbHocVien.Items.Clear();

                clbHocVien.DisplayMember =
                    nameof(LuaChonHocVien.HienThi);

                clbHocVien.ValueMember =
                    nameof(LuaChonHocVien.MaHV);

                foreach (var hv in danhSachLoc)
                {
                    bool daChon =
                        maHocVienDaChon.Contains(hv.MaHV);

                    clbHocVien.Items.Add(hv, daChon);
                }
            }
            finally
            {
                clbHocVien.EndUpdate();
                dangCapNhatTichChon = false;
            }

            int soChuaCoLop = danhSachHocVien
                .Count(hv =>
                    !maHocVienDaCoLop.Contains(hv.MaHV));

            lblGoiY.Text =
                $"Chưa có lớp: {soChuaCoLop} học viên " +
                $"| Đang hiển thị: {danhSachLoc.Count}";

            CapNhatSoLuongChon();
        }

        // ========================================
        // 7. TÍCH CHỌN HỌC VIÊN
        // ========================================

        private void ClbHocVien_ItemCheck(
            object? sender,
            ItemCheckEventArgs e)
        {
            if (dangCapNhatTichChon)
                return;

            if (e.Index < 0 ||
                e.Index >= clbHocVien.Items.Count)
                return;

            if (clbHocVien.Items[e.Index]
                is not LuaChonHocVien hv)
                return;

            // Chặn nếu học viên đã có lớp.
            if (maHocVienDaCoLop.Contains(hv.MaHV))
            {
                e.NewValue = CheckState.Unchecked;
                maHocVienDaChon.Remove(hv.MaHV);
                return;
            }

            if (e.NewValue == CheckState.Checked)
                maHocVienDaChon.Add(hv.MaHV);
            else
                maHocVienDaChon.Remove(hv.MaHV);

            if (IsHandleCreated)
            {
                BeginInvoke(
                    (Action)CapNhatSoLuongChon);
            }
        }

        // ========================================
        // 8. CHỌN TẤT CẢ ĐANG HIỂN THỊ
        // ========================================

        private void ChkChonTatCa_CheckedChanged(
            object? sender,
            EventArgs e)
        {
            if (dangCapNhatTichChon ||
                maDangChon.HasValue)
                return;

            bool chon = chkChonTatCa.Checked;

            dangCapNhatTichChon = true;
            clbHocVien.BeginUpdate();

            try
            {
                for (int i = 0; i < clbHocVien.Items.Count; i++)
                {
                    if (clbHocVien.Items[i]
                        is not LuaChonHocVien hv)
                        continue;

                    if (maHocVienDaCoLop.Contains(hv.MaHV))
                        continue;

                    if (chon)
                        maHocVienDaChon.Add(hv.MaHV);
                    else
                        maHocVienDaChon.Remove(hv.MaHV);

                    clbHocVien.SetItemChecked(i, chon);
                }
            }
            finally
            {
                clbHocVien.EndUpdate();
                dangCapNhatTichChon = false;
            }

            CapNhatSoLuongChon();
        }

        // ========================================
        // 9. HIỂN THỊ SỐ LƯỢNG ĐÃ CHỌN
        // ========================================

        private void CapNhatSoLuongChon()
        {
            lblDaChon.Text =
                $"Đã chọn: {maHocVienDaChon.Count} học viên";

            bool coHocVien =
                clbHocVien.Items.Count > 0;

            bool tatCaDaChon = coHocVien;

            foreach (var item in clbHocVien.Items)
            {
                if (item is LuaChonHocVien hv &&
                    !maHocVienDaChon.Contains(hv.MaHV))
                {
                    tatCaDaChon = false;
                    break;
                }
            }

            dangCapNhatTichChon = true;

            try
            {
                chkChonTatCa.Checked = tatCaDaChon;

                chkChonTatCa.Enabled =
                    coHocVien && !maDangChon.HasValue;
            }
            finally
            {
                dangCapNhatTichChon = false;
            }
        }

        // ========================================
        // 10. NẠP DANH SÁCH ĐĂNG KÝ
        // ========================================

        private void NapDanhSach(string tuKhoa = "")
        {
            var danhSach = bll.LayDanhSach(tuKhoa);

            dangNap = true;

            try
            {
                dgv.DataSource = null;
                dgv.DataSource = danhSach;
                dgv.ClearSelection();
            }
            finally
            {
                dangNap = false;
            }

            lblSoLuong.Text =
                $"Tổng số: {danhSach.Count} đăng ký";
        }

        private void HienThiHocPhiDuKien()
        {
            txtHocPhi.Text =
                cboLopHoc.SelectedItem is LuaChonLopHoc lop
                    ? $"{lop.HocPhi:N0}"
                    : "";
        }

        // ========================================
        // 11. CHỌN MỘT ĐĂNG KÝ CŨ
        // ========================================

        private void Dgv_SelectionChanged(
            object? sender,
            EventArgs e)
        {
            if (dangNap || dgv.SelectedRows.Count == 0)
                return;

            if (dgv.SelectedRows[0].DataBoundItem
                is not DongDangKyHoc dk)
                return;

            maDangChon = dk.MaDK;

            txtMaDK.Text = dk.MaDK.ToString();

            cboLopHoc.SelectedValue = dk.MaLop;
            cboLopHoc.Enabled = false;

            txtHocPhi.Text =
                $"{dk.HocPhiPhaiDong:N0}";

            txtNgayDangKy.Text =
                dk.NgayDangKy.ToString(
                    "dd/MM/yyyy HH:mm");

            txtTrangThai.Text =
                TenTrangThai(dk.TrangThai);

            // Khi xem đăng ký cũ,
            // tạm khóa chức năng thêm.
            txtTimHocVien.Enabled = false;
            clbHocVien.Enabled = false;
            chkChonTatCa.Enabled = false;
            btnThem.Enabled = false;
        }

        // ========================================
        // 12. THÊM NHIỀU HỌC VIÊN
        // ========================================

        private void ThemNhieuHocVien()
        {
            if (maDangChon.HasValue)
            {
                throw new InvalidOperationException(
                    "Bạn đang xem một đăng ký cũ. " +
                    "Hãy nhấn Làm mới trước khi thêm.");
            }

            if (cboLopHoc.SelectedItem
                is not LuaChonLopHoc lop)
            {
                throw new ArgumentException(
                    "Vui lòng chọn lớp học.");
            }

            // KIỂM TRA LẠI TOÀN BỘ DỮ LIỆU
            // NGAY TRƯỚC KHI THÊM.
            // Không chỉ dựa vào danh sách đang hiển thị.
            CapNhatHocVienDaCoLop();

            var maDaChon = maHocVienDaChon
                .OrderBy(x => x)
                .ToList();

            if (maDaChon.Count == 0)
            {
                HienThiDanhSachHocVien();

                throw new ArgumentException(
                    "Không có học viên chưa có lớp được chọn. " +
                    "Hãy kiểm tra lại danh sách.");
            }

            // Kiểm tra toàn bộ lớp, không chỉ lớp đang chọn.
            var danhSachDangKy = bll.LayDanhSach();

            var maDangHoc = danhSachDangKy
                .Where(d =>
                    LaDangKyConHieuLuc(d.TrangThai))
                .Select(d => d.MaHV)
                .ToHashSet();

            var dsTrung = maDaChon
                .Where(x => maDangHoc.Contains(x))
                .ToList();

            var dsCanThem = maDaChon
                .Where(x => !maDangHoc.Contains(x))
                .ToList();

            if (dsCanThem.Count == 0)
            {
                HienThiDanhSachHocVien();

                throw new InvalidOperationException(
                    "Các học viên đã chọn đều đã có lớp học.");
            }

            var xacNhan = MessageBox.Show(
                $"Lớp đăng ký: {lop.HienThi}\n\n" +
                $"Học viên được chọn: {maDaChon.Count}\n" +
                $"Đã có lớp (bỏ qua): {dsTrung.Count}\n" +
                $"Sẽ đăng ký: {dsCanThem.Count}\n\n" +
                "Hệ thống sẽ kiểm tra từng học viên " +
                "trước khi lưu.\n\n" +
                "Tiếp tục đăng ký?",
                "Xác nhận đăng ký hàng loạt",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (xacNhan != DialogResult.Yes)
                return;

            int thanhCong = 0;
            int boQua = dsTrung.Count;

            var dsThatBai = new List<string>();

            btnThem.Enabled = false;

            try
            {
                foreach (int maHV in dsCanThem)
                {
                    // Kiểm tra lại ngay trước từng lần thêm.
                    // Hạn chế đăng ký trùng khi danh sách
                    // thay đổi trong lúc xử lý.
                    bool daCoLop = bll.LayDanhSach()
                        .Any(d =>
                            d.MaHV == maHV &&
                            LaDangKyConHieuLuc(d.TrangThai));

                    if (daCoLop)
                    {
                        boQua++;
                        continue;
                    }

                    try
                    {
                        // BLL cần tiếp tục kiểm tra
                        // sức chứa lớp và các ràng buộc.
                        bll.Them(maHV, lop.MaLop);
                        thanhCong++;
                    }
                    catch (Exception ex)
                    {
                        string ten = danhSachHocVien
                            .FirstOrDefault(
                                x => x.MaHV == maHV)
                            ?.HienThi ?? $"HV {maHV}";

                        dsThatBai.Add(
                            $"{ten}: {ex.Message}");
                    }
                }
            }
            finally
            {
                btnThem.Enabled = true;
            }

            LamMoi();

            string thongBao =
                $"Đăng ký thành công: {thanhCong}\n" +
                $"Bỏ qua vì đã có lớp: {boQua}\n" +
                $"Không thêm được: {dsThatBai.Count}";

            if (dsThatBai.Count > 0)
            {
                thongBao += "\n\nChi tiết:\n" +
                    string.Join(
                        "\n",
                        dsThatBai.Take(10));
            }

            MessageBox.Show(
                thongBao,
                "Kết quả đăng ký",
                MessageBoxButtons.OK,
                dsThatBai.Count > 0
                    ? MessageBoxIcon.Warning
                    : MessageBoxIcon.Information);
        }

        // ========================================
        // 13. HOÀN THÀNH
        // ========================================

        private void HoanThanh()
        {
            if (!maDangChon.HasValue)
            {
                throw new ArgumentException(
                    "Vui lòng chọn đăng ký cần hoàn thành.");
            }

            if (MessageBox.Show(
                "Xác nhận học viên đã hoàn thành lớp học?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)
                != DialogResult.Yes)
                return;

            if (bll.HoanThanh(maDangChon.Value))
                LamMoi();
        }

        // ========================================
        // 14. HỦY ĐĂNG KÝ
        // ========================================

        private void Huy()
        {
            if (!maDangChon.HasValue)
            {
                throw new ArgumentException(
                    "Vui lòng chọn đăng ký cần hủy.");
            }

            if (MessageBox.Show(
                "Bạn chắc chắn muốn hủy đăng ký?\n" +
                "Dữ liệu lịch sử sẽ được giữ lại.",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning)
                != DialogResult.Yes)
                return;

            if (bll.Huy(maDangChon.Value))
                LamMoi();
        }

        // ========================================
        // 15. KHÔI PHỤC
        // ========================================

        private void KhoiPhuc()
        {
            if (!maDangChon.HasValue)
            {
                throw new ArgumentException(
                    "Vui lòng chọn đăng ký cần khôi phục.");
            }

            // Trước khi khôi phục, phải kiểm tra
            // học viên đã có lớp khác hay chưa.
            var dangKy = bll.LayDanhSach()
                .FirstOrDefault(
                    x => x.MaDK == maDangChon.Value);

            if (dangKy == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy đăng ký.");
            }

            if (dangKy.TrangThai == "DaHuy")
            {
                bool daCoLopKhac = bll.LayDanhSach()
                    .Any(x =>
                        x.MaHV == dangKy.MaHV &&
                        x.MaDK != dangKy.MaDK &&
                        LaDangKyConHieuLuc(x.TrangThai));

                if (daCoLopKhac)
                {
                    throw new InvalidOperationException(
                        "Không thể khôi phục vì học viên " +
                        "đang có đăng ký lớp khác.");
                }
            }

            if (bll.KhoiPhuc(maDangChon.Value))
                LamMoi();
        }

        // ========================================
        // 16. XÓA LỰA CHỌN
        // ========================================

        private void XoaLuaChon()
        {
            maDangChon = null;

            txtMaDK.Clear();
            txtNgayDangKy.Text = "Tự động khi thêm";
            txtTrangThai.Text = "Đang học (mới)";

            cboLopHoc.Enabled = true;
            cboLopHoc.SelectedIndex = -1;

            txtHocPhi.Clear();

            txtTimHocVien.Enabled = true;
            clbHocVien.Enabled = true;
            chkChonTatCa.Enabled = true;

            btnThem.Enabled = true;

            maHocVienDaChon.Clear();

            HienThiDanhSachHocVien();
            dgv.ClearSelection();
        }

        // ========================================
        // 17. LÀM MỚI
        // ========================================

        private void LamMoi()
        {
            dangNap = true;

            try
            {
                txtTimKiem.Clear();
                txtTimHocVien.Clear();

                maHocVienDaChon.Clear();
                maDangChon = null;

                NapDanhMuc();
                NapDanhSach();

                XoaLuaChon();
            }
            finally
            {
                dangNap = false;
            }
        }

        // ========================================
        // 18. XỬ LÝ LỖI
        // ========================================

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
                    "Không thể xử lý đăng ký học.\n\n" +
                    $"Chi tiết: {ex.Message}",
                    "Lỗi hệ thống",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
