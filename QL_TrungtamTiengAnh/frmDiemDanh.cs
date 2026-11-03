using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmDiemDanh : Form
    {
        private readonly DiemDanhBLL bll = new();
        // Chỉ đọc sức chứa phòng của lớp để hiển thị sĩ số.
        private readonly DiemDanhDAL thongTinPhongDal = new();
        private readonly ComboBox cboLop = new();
        private readonly TextBox txtMaBuoi = new();
        private readonly DateTimePicker dtpNgayHoc = new();
        private readonly DateTimePicker dtpBatDau = new();
        private readonly DateTimePicker dtpKetThuc = new();
        private readonly TextBox txtNoiDung = new();
        private readonly DataGridView dgvBuoiHoc = new();
        private readonly DataGridView dgvDiemDanh = new();
        private readonly Label lblSoBuoi = new();
        private readonly Label lblSoHocVien = new();
        private readonly Label lblThongKe = new();
        private readonly BindingSource nguonDiemDanh = new();

        private int? maBuoiDangChon;
        private int? sucChuaPhongDangChon;
        private bool dangNap;

        // Phiên đăng nhập được truyền từ frmMain.
        // Việc phân quyền theo MaGV phải được kiểm tra tiếp ở BLL/DAL.
        private readonly PhienDangNhap? phienDangNhap;

        public frmDiemDanh(PhienDangNhap phien) : this()
        {
            phienDangNhap = phien ?? throw new ArgumentNullException(nameof(phien));
        }

        // Giữ constructor cũ để tương thích code hiện tại;
        // chưa sử dụng constructor này để xác thực quyền giáo viên.
        public frmDiemDanh()
        {
            InitializeComponent();
            TaoGiaoDien();
            Shown += (_, _) => XuLyAnToan(TaiBanDau);
        }

        private static Button TaoNut(string text, Color mau, int rong = 132)
        {
            var btn = new Button
            {
                Text = text,
                Size = new Size(rong, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = mau,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 6)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static void ThemTruong(TableLayoutPanel bang,
            string tieuDe, Control input, int dong, int nua)
        {
            int cot = nua * 2;
            bang.Controls.Add(new Label
            {
                Text = tieuDe,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(42, 56, 70),
                Font = new Font("Segoe UI", 10)
            }, cot, dong);

            input.Dock = DockStyle.Fill;
            input.Font = new Font("Segoe UI", 11);
            input.Margin = new Padding(5, 6, 13, 6);
            bang.Controls.Add(input, cot + 1, dong);
        }

        private void TaoGiaoDien()
        {
            Text = "Quản lý điểm danh";
            ClientSize = new Size(1200, 800);
            MinimumSize = Size.Empty; // Để frmMain nhúng Form trong Panel
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);
            Controls.Clear();

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(18),
                ColumnCount = 1,
                RowCount = 4
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 75));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 156));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(layout);

            var header = new Panel { Dock = DockStyle.Fill };
            header.Controls.Add(new Label
            {
                Text = "ĐIỂM DANH LỚP HỌC",
                Location = new Point(5, 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });
            header.Controls.Add(new Label
            {
                Text = "Chọn lớp và buổi học, sau đó ghi nhận tình hình chuyên cần",
                Location = new Point(7, 47),
                AutoSize = true,
                ForeColor = Color.DimGray
            });
            layout.Controls.Add(header, 0, 0);

            var grp = new GroupBox
            {
                Text = "THÔNG TIN BUỔI HỌC",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(23, 52, 83),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Padding = new Padding(10)
            };

            var nhap = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 3,
                Padding = new Padding(7, 4, 7, 4)
            };
            nhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
            nhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            nhap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
            nhap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            for (int i = 0; i < 3; i++)
                nhap.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 3));

            cboLop.DropDownStyle = ComboBoxStyle.DropDownList;
            dtpNgayHoc.Format = DateTimePickerFormat.Short;
            dtpBatDau.Format = DateTimePickerFormat.Custom;
            dtpBatDau.CustomFormat = "HH:mm";
            dtpKetThuc.Format = DateTimePickerFormat.Custom;
            dtpKetThuc.CustomFormat = "HH:mm";
            dtpBatDau.ShowUpDown = true; 
            dtpKetThuc.ShowUpDown = true;
            dtpBatDau.Value = DateTime.Today.AddHours(18);
            dtpKetThuc.Value = DateTime.Today.AddHours(20);
            txtNoiDung.MaxLength = 500;

            ThemTruong(nhap, "Lớp học (*)", cboLop, 0, 0);
            ThemTruong(nhap, "Ngày học (*)", dtpNgayHoc, 0, 1);
            ThemTruong(nhap, "Giờ bắt đầu (*)", dtpBatDau, 1, 0);
            ThemTruong(nhap, "Giờ kết thúc (*)", dtpKetThuc, 1, 1);
            ThemTruong(nhap, "Nội dung buổi học", txtNoiDung, 2, 0);
            grp.Controls.Add(nhap);
            layout.Controls.Add(grp, 0, 1);

            // Một lớp được dùng đồng thời để tạo buổi và lọc danh sách buổi.
            cboLop.SelectedIndexChanged += (_, _) =>
            {
                if (!dangNap)
                    XuLyAnToan(() => NapDanhSachBuoi());
            };

            var tools = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(0, 10, 0, 0)
            };
            var btnTao = TaoNut("Tạo buổi học", Color.FromArgb(30, 140, 95), 140);
            var btnHuy = TaoNut("Hủy buổi", Color.FromArgb(195, 65, 70), 115);
            var btnLamMoi = TaoNut("Làm mới", Color.FromArgb(100, 112, 125), 115);
            var btnTatCa = TaoNut("Tất cả lớp", Color.FromArgb(80, 105, 150), 115);
            var btnCoMat = TaoNut("Tất cả có mặt", Color.FromArgb(40, 110, 190), 158);
            var btnLuu = TaoNut("Lưu điểm danh", Color.FromArgb(23, 85, 130), 155);
            tools.Controls.AddRange(new Control[]
            {
                btnTao, btnHuy, btnLamMoi, btnTatCa, btnCoMat, btnLuu
            });
            layout.Controls.Add(tools, 0, 2);

            btnTao.Click += (_, _) => XuLyAnToan(TaoBuoiHoc);
            btnHuy.Click += (_, _) => XuLyAnToan(HuyBuoiHoc);
            btnLamMoi.Click += (_, _) => XuLyAnToan(LamMoi);
            btnTatCa.Click += (_, _) => XuLyAnToan(HienThiTatCaLop);
            btnCoMat.Click += (_, _) => XuLyAnToan(DanhDauTatCaCoMat);
            btnLuu.Click += (_, _) => XuLyAnToan(LuuDiemDanh);

            // Hai bảng cùng Dock=Fill: tránh lỗi bảng thu nhỏ như bước Học phí.
            var vungBang = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            vungBang.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            vungBang.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            vungBang.RowStyles.Add(new RowStyle(SizeType.Percent, 30));
            vungBang.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            vungBang.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            vungBang.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            vungBang.RowStyles.Add(new RowStyle(SizeType.Absolute, 2));
            layout.Controls.Add(vungBang, 0, 3);

            vungBang.Controls.Add(TaoTieuDe("DANH SÁCH BUỔI HỌC", lblSoBuoi), 0, 0);
            CauHinhBang(dgvBuoiHoc, false);
            TaoCotBuoiHoc();
            vungBang.Controls.Add(dgvBuoiHoc, 0, 1);

            lblThongKe.Dock = DockStyle.Fill;
            lblThongKe.Padding = new Padding(14, 8, 8, 8);
            lblThongKe.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblThongKe.BackColor = Color.FromArgb(234, 243, 252);
            lblThongKe.ForeColor = Color.FromArgb(23, 52, 83);
            lblThongKe.TextAlign = ContentAlignment.MiddleLeft;
            vungBang.Controls.Add(lblThongKe, 0, 2);

            vungBang.Controls.Add(TaoTieuDe("DANH SÁCH HỌC VIÊN – ĐIỂM DANH", lblSoHocVien), 0, 3);
            CauHinhBang(dgvDiemDanh, true);
            TaoCotDiemDanh();
            vungBang.Controls.Add(dgvDiemDanh, 0, 4);
            dgvBuoiHoc.SelectionChanged += DgvBuoiHoc_SelectionChanged;
            dgvDiemDanh.CurrentCellDirtyStateChanged += (_, _) =>
            {
                if (dgvDiemDanh.IsCurrentCellDirty)
                    dgvDiemDanh.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            dgvDiemDanh.CellValueChanged += (_, _) => CapNhatThongKe();
            dgvDiemDanh.DataError += (_, e) =>
            {
                Debug.WriteLine(e.Exception);
                e.ThrowException = false;
            };
            CapNhatThongKe();
        }

        private static Control TaoTieuDe(string text, Label dem)
        {
            var panel = new Panel { Dock = DockStyle.Fill };
            panel.Controls.Add(new Label
            {
                Text = text,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });
            dem.Dock = DockStyle.Right;
            dem.Width = 240;
            dem.TextAlign = ContentAlignment.MiddleRight;
            dem.ForeColor = Color.DimGray;
            panel.Controls.Add(dem);
            return panel;
        }

        private static void CauHinhBang(DataGridView grid, bool choPhepSua)
        {
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.AutoGenerateColumns = false;
            grid.ReadOnly = !choPhepSua;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AllowUserToResizeRows = false;
            grid.RowHeadersVisible = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersHeight = 38;
            grid.RowTemplate.Height = choPhepSua ? 42 : 38;
            grid.DefaultCellStyle.Font = new Font("Segoe UI", 10.5f);
            grid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 52, 83);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(215, 232, 248);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
        }

        private static void ThemCot(DataGridView dgv, string ten, string tieuDe,
            float tyLe, string? dinhDang = null, bool chiDoc = true)
        {
            var cot = new DataGridViewTextBoxColumn
            {
                Name = ten,
                HeaderText = tieuDe,
                DataPropertyName = ten,
                FillWeight = tyLe,
                ReadOnly = chiDoc
            };
            if (dinhDang != null)
                cot.DefaultCellStyle.Format = dinhDang;
            dgv.Columns.Add(cot);
        }

        private void TaoCotBuoiHoc()
        {
            // MaBuoi được giữ trong DongBuoiHoc để Sửa/Hủy/Lưu, chỉ ẩn khỏi bảng.
            ThemCot(dgvBuoiHoc, "TenLop", "Lớp học", 155);
            ThemCot(dgvBuoiHoc, "NgayHoc", "Ngày học", 104, "dd/MM/yyyy");
            ThemCot(dgvBuoiHoc, "KhungGio", "Giờ học", 110);
            ThemCot(dgvBuoiHoc, "NoiDung", "Nội dung", 210);
            ThemCot(dgvBuoiHoc, "HienThiTrangThai", "Trạng thái", 120);
            ThemCot(dgvBuoiHoc, "DaGhiNhan", "Đã ghi nhận", 85);
        }

        private void TaoCotDiemDanh()
        {
            var stt = new DataGridViewTextBoxColumn
            {
                Name = "STT",
                HeaderText = "STT",
                FillWeight = 52,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
            dgvDiemDanh.Columns.Add(stt);
            ThemCot(dgvDiemDanh, "MaHV", "Mã (VT)", 82);
            dgvDiemDanh.Columns["MaHV"].DefaultCellStyle.Format = "'VT'00";
            ThemCot(dgvDiemDanh, "HoTen", "Họ và tên", 220);
            ThemCot(dgvDiemDanh, "SoDienThoai", "Số điện thoại", 140);

            var trangThai = new DataGridViewComboBoxColumn
            {
                Name = "TrangThai",
                HeaderText = "Điểm danh (*)",
                DataPropertyName = "TrangThai",
                DisplayMember = "Ten",
                ValueMember = "Ma",
                DataSource = new[]
                {
                    new LuaChonTrangThai("ChuaDiemDanh", "Chưa điểm danh"),
                    new LuaChonTrangThai("CoMat", "Có mặt"),
                    new LuaChonTrangThai("Vang", "Vắng"),
                    new LuaChonTrangThai("CoPhep", "Có phép"),
                    new LuaChonTrangThai("DiTre", "Đi trễ")
                },
                FlatStyle = FlatStyle.Flat,
                FillWeight = 145
            };
            dgvDiemDanh.Columns.Add(trangThai);
            ThemCot(dgvDiemDanh, "GhiChu", "Ghi chú", 230, chiDoc: false);
            dgvDiemDanh.DataSource = nguonDiemDanh;
            dgvDiemDanh.CellFormatting += (_, e) =>
            {
                if (e.RowIndex < 0) return;
                if (dgvDiemDanh.Columns[e.ColumnIndex].Name == "STT")
                {
                    e.Value = (e.RowIndex + 1).ToString();
                    e.FormattingApplied = true;
                }
                else if (dgvDiemDanh.Columns[e.ColumnIndex].Name == "MaHV" && e.Value is int id)
                {
                    e.Value = $"VT{id:00}";
                    e.FormattingApplied = true;
                }
            };
        }

        public sealed class LuaChonTrangThai
        {
            public LuaChonTrangThai(string ma, string ten)
            {
                Ma = ma;
                Ten = ten;
            }
            public string Ma { get; }
            public string Ten { get; }
        }

        // Thống kê dựa trên trạng thái đang chọn trong bảng.
        // Chỉ lưu vào SQL Server khi giáo viên nhấn "Lưu điểm danh".
        private void CapNhatThongKe()
        {
            var ds = nguonDiemDanh.List.Cast<DongDiemDanh>().ToList();
            int coMat = ds.Count(d => d.TrangThai == "CoMat");
            int diTre = ds.Count(d => d.TrangThai == "DiTre");
            int vang = ds.Count(d => d.TrangThai == "Vang" || d.TrangThai == "CoPhep");
            int chua = ds.Count(d => d.TrangThai == "ChuaDiemDanh" || string.IsNullOrEmpty(d.TrangThai));
            string siSo = maBuoiDangChon.HasValue
                ? $"{ds.Count}/{(sucChuaPhongDangChon.HasValue ? sucChuaPhongDangChon.Value.ToString() : "Chưa phân phòng")}"
                : "— (chưa chọn buổi)";
            lblThongKe.Text = $"SĨ SỐ: {siSo}    |    CÓ MẶT: {coMat}    |    ĐI TRỄ: {diTre}    |    VẮNG: {vang}    |    CHƯA ĐIỂM DANH: {chua}";
        }

        private void TaiBanDau()
        {
            dangNap = true;
            try
            {
                cboLop.DisplayMember = nameof(LuaChonLopDiemDanh.HienThi);
                cboLop.ValueMember = nameof(LuaChonLopDiemDanh.MaLop);
                cboLop.DataSource = bll.LayLopHocs();
                cboLop.SelectedIndex = -1;
            }
            finally { dangNap = false; }
            NapDanhSachBuoi();
        }

        private int? LopDangChon => cboLop.SelectedValue is int id ? id : null;

        private void NapDanhSachBuoi(int? chonMaBuoi = null)
        {
            dangNap = true;
            try
            {
                var danhSach = bll.LayBuoiHocs(LopDangChon);
                dgvBuoiHoc.DataSource = null;
                dgvBuoiHoc.DataSource = danhSach;
                lblSoBuoi.Text = $"Tổng số: {danhSach.Count} buổi";
                dgvBuoiHoc.ClearSelection();

                maBuoiDangChon = null;
                sucChuaPhongDangChon = null;
                nguonDiemDanh.DataSource = new BindingList<DongDiemDanh>();
                lblSoHocVien.Text = "Chưa chọn buổi học";
                CapNhatThongKe();

                if (chonMaBuoi.HasValue)
                {
                    foreach (DataGridViewRow r in dgvBuoiHoc.Rows)
                    {
                        if (r.DataBoundItem is DongBuoiHoc b && b.MaBuoi == chonMaBuoi)
                        {
                            r.Selected = true;
                            dgvBuoiHoc.CurrentCell = r.Cells[0];
                            break;
                        }
                    }
                }
            }
            finally { dangNap = false; }
            // Tải điểm danh sau khi đã kết thúc quá trình binding.
            if (chonMaBuoi.HasValue)
                ChonBuoiHocDangChon();
        }

        private void DgvBuoiHoc_SelectionChanged(object? sender, EventArgs e)
        {
            if (!dangNap)
                XuLyAnToan(ChonBuoiHocDangChon);
        }

        private void ChonBuoiHocDangChon()
        {
            if (dgvBuoiHoc.SelectedRows.Count == 0 ||
                dgvBuoiHoc.SelectedRows[0].DataBoundItem is not DongBuoiHoc buoi)
                return;

            maBuoiDangChon = buoi.MaBuoi;
            sucChuaPhongDangChon = thongTinPhongDal.LaySucChuaPhongCuaLop(buoi.MaLop);
            dtpNgayHoc.Value = buoi.NgayHoc;
            dtpBatDau.Value = DateTime.Today.Add(buoi.GioBatDau);
            dtpKetThuc.Value = DateTime.Today.Add(buoi.GioKetThuc);
            txtNoiDung.Text = buoi.NoiDung;
            if (cboLop.SelectedValue is not int maLop || maLop != buoi.MaLop)
            {
                dangNap = true;
                try { cboLop.SelectedValue = buoi.MaLop; }
                finally { dangNap = false; }
            }
            var ds = bll.LayDiemDanh(buoi.MaBuoi);
            nguonDiemDanh.DataSource = new BindingList<DongDiemDanh>(ds);
            lblSoHocVien.Text = sucChuaPhongDangChon.HasValue
                ? $"Sĩ số: {ds.Count}/{sucChuaPhongDangChon.Value} học viên"
                : $"Sĩ số: {ds.Count} học viên (chưa phân phòng)";
            CapNhatThongKe();
            dgvDiemDanh.ReadOnly = buoi.TrangThai == "DaHuy";
            dgvDiemDanh.ClearSelection();
        }

        private void TaoBuoiHoc()
        {
            if (maBuoiDangChon.HasValue)
                throw new InvalidOperationException(
                    "Bạn đang xem một buổi học cũ. Nhấn Làm mới rồi chọn lớp để tạo buổi mới.");
            if (LopDangChon is not int maLop)
                throw new ArgumentException("Vui lòng chọn lớp học.");

            int ma = bll.TaoBuoiHoc(maLop, dtpNgayHoc.Value.Date,
                dtpBatDau.Value.TimeOfDay, dtpKetThuc.Value.TimeOfDay,
                txtNoiDung.Text);
            MessageBox.Show($"Đã tạo buổi học, mã: {ma}.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            NapDanhSachBuoi(ma);
        }

        private void LuuDiemDanh()
        {
            if (!maBuoiDangChon.HasValue)
                throw new ArgumentException("Vui lòng chọn buổi học trong bảng phía trên.");
            dgvDiemDanh.EndEdit();
            nguonDiemDanh.EndEdit();

            var ds = nguonDiemDanh.List.Cast<DongDiemDanh>()
                .Select(d => new CapNhatDiemDanh
                {
                    MaHV = d.MaHV,
                    TrangThai = d.TrangThai,
                    GhiChu = d.GhiChu
                }).ToList();
            int tong = bll.LuuDiemDanh(maBuoiDangChon.Value, ds);
            MessageBox.Show($"Đã lưu điểm danh cho {tong} học viên.",
                "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            NapDanhSachBuoi(maBuoiDangChon.Value);
        }

        private void DanhDauTatCaCoMat()
        {
            if (!maBuoiDangChon.HasValue)
                throw new ArgumentException("Vui lòng chọn buổi học trước.");
            if (dgvDiemDanh.ReadOnly)
                throw new InvalidOperationException("Buổi học đã hủy.");

            dgvDiemDanh.EndEdit();
            foreach (DongDiemDanh dong in nguonDiemDanh.List)
            {
                dong.TrangThai = "CoMat";
            }
            nguonDiemDanh.ResetBindings(false);
            CapNhatThongKe();
        }

        private void HuyBuoiHoc()
        {
            if (!maBuoiDangChon.HasValue)
                throw new ArgumentException("Vui lòng chọn buổi học muốn hủy.");
            if (MessageBox.Show("Xác nhận hủy buổi học? Dữ liệu sẽ được giữ lại.",
                "Xác nhận", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            bll.HuyBuoiHoc(maBuoiDangChon.Value);
            NapDanhSachBuoi();
        }

        private void LamMoi()
        {
            // Giữ nguyên lớp đang chọn để dễ tạo buổi tiếp theo.
            var id = LopDangChon;
            dangNap = true;
            try
            {
                cboLop.DataSource = bll.LayLopHocs();
                cboLop.SelectedValue = id ?? -1;
            }
            finally { dangNap = false; }
            dtpNgayHoc.Value = DateTime.Today;
            dtpBatDau.Value = DateTime.Today.AddHours(18);
            dtpKetThuc.Value = DateTime.Today.AddHours(20);
            txtNoiDung.Clear();
            NapDanhSachBuoi();
        }

        private void HienThiTatCaLop()
        {
            dangNap = true;
            try { cboLop.SelectedIndex = -1; }
            finally { dangNap = false; }
            NapDanhSachBuoi();
        }

        private static void XuLyAnToan(Action hanhDong)
        {
            try { hanhDong(); }
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
                Debug.WriteLine(ex);
                MessageBox.Show("Có lỗi khi thao tác với dữ liệu điểm danh. " +
                    "Vui lòng kiểm tra SQL Server và thử lại.",
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
