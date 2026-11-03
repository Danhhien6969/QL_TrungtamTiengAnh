
using System;
using System.Drawing;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmMain : Form
    {
        // ============================================
        // 1. BỘ MÀU NAVY - TRẮNG
        // ============================================

        private static readonly Color Navy =
            Color.FromArgb(21, 46, 75);

        private static readonly Color NavyDark =
            Color.FromArgb(14, 33, 56);

        private static readonly Color NavyHover =
            Color.FromArgb(40, 76, 112);

        private static readonly Color Blue =
            Color.FromArgb(45, 111, 190);

        private static readonly Color White =
            Color.White;

        private static readonly Color Background =
            Color.FromArgb(245, 248, 252);

        private static readonly Color TextDark =
            Color.FromArgb(26, 48, 74);

        private static readonly Color TextMuted =
            Color.FromArgb(108, 127, 148);

        private static readonly Color MenuText =
            Color.FromArgb(230, 238, 247);

        // ============================================
        // 2. KHAI BÁO
        // ============================================

        private readonly PhienDangNhap phien;

        private readonly TableLayoutPanel layoutChinh = new();
        private readonly TableLayoutPanel layoutBenPhai = new();

        private readonly Panel pnlMenu = new();
        private readonly Panel pnlHeader = new();
        private readonly Panel pnlNoiDung = new();

        private readonly FlowLayoutPanel pnlDanhMuc = new();

        private readonly Label lblTieuDeTrang = new();
        private readonly Label lblMoTaTrang = new();

        private Form? formDangMo;
        private Button? nutDangChon;

        private bool LaAdmin =>
            string.Equals(
                phien.TenVaiTro,
                "Admin",
                StringComparison.OrdinalIgnoreCase);

        private bool LaGiaoVien =>
            string.Equals(
                phien.TenVaiTro,
                "GiaoVien",
                StringComparison.OrdinalIgnoreCase);

        // ============================================
        // 3. CONSTRUCTOR
        // ============================================

        public frmMain(PhienDangNhap phienDangNhap)
        {
            phien = phienDangNhap ??
                throw new ArgumentNullException(
                    nameof(phienDangNhap));

            InitializeComponent();

            TaoGiaoDien();
        }

        // ============================================
        // 4. GIAO DIỆN CHÍNH
        // ============================================

        private void TaoGiaoDien()
        {
            SuspendLayout();

            Text = "English Center - Hệ thống quản lý";

            StartPosition =
                FormStartPosition.CenterScreen;

            WindowState =
                FormWindowState.Maximized;

            ClientSize = new Size(1360, 800);

            MinimumSize = new Size(1050, 650);

            BackColor = Background;

            Font = new Font("Segoe UI", 10);

            AutoScaleMode = AutoScaleMode.Dpi;

            DoubleBuffered = true;

            Controls.Clear();

            // ========================================
            // BỐ CỤC TỔNG THỂ
            //
            // CỘT 1: MENU TRÁI
            // CỘT 2: HEADER + FORM CON
            //
            // DÙNG TABLELAYOUTPANEL ĐỂ KHÔNG
            // BỊ CHỒNG LẤN DO THỨ TỰ DOCK
            // ========================================

            layoutChinh.Dock = DockStyle.Fill;

            layoutChinh.ColumnCount = 2;
            layoutChinh.RowCount = 1;

            layoutChinh.Margin = Padding.Empty;
            layoutChinh.Padding = Padding.Empty;

            layoutChinh.BackColor = Background;

            layoutChinh.ColumnStyles.Clear();
            layoutChinh.RowStyles.Clear();

            layoutChinh.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute, 242));

            layoutChinh.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 100));

            layoutChinh.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100));

            Controls.Add(layoutChinh);

            // ========================================
            // MENU TRÁI
            // ========================================

            pnlMenu.Dock = DockStyle.Fill;

            pnlMenu.Margin = Padding.Empty;
            pnlMenu.Padding = Padding.Empty;

            pnlMenu.BackColor = Navy;

            layoutChinh.Controls.Add(
                pnlMenu, 0, 0);

            TaoMenuTrai();

            // ========================================
            // BÊN PHẢI
            // ========================================

            layoutBenPhai.Dock = DockStyle.Fill;

            layoutBenPhai.Margin = Padding.Empty;
            layoutBenPhai.Padding = Padding.Empty;

            layoutBenPhai.ColumnCount = 1;
            layoutBenPhai.RowCount = 2;

            layoutBenPhai.ColumnStyles.Clear();
            layoutBenPhai.RowStyles.Clear();

            layoutBenPhai.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 100));

            // Header cao 78px
            layoutBenPhai.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 78));

            // Phần còn lại là nội dung
            layoutBenPhai.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100));

            layoutChinh.Controls.Add(
                layoutBenPhai, 1, 0);

            // ========================================
            // HEADER
            // ========================================

            pnlHeader.Dock = DockStyle.Fill;

            pnlHeader.Margin = Padding.Empty;
            pnlHeader.Padding = Padding.Empty;

            pnlHeader.BackColor = White;

            layoutBenPhai.Controls.Add(
                pnlHeader, 0, 0);

            TaoThanhTieuDe();

            // ========================================
            // VÙNG NỘI DUNG FORM CON
            // ========================================

            pnlNoiDung.Dock = DockStyle.Fill;

            pnlNoiDung.Margin = Padding.Empty;

            pnlNoiDung.Padding = Padding.Empty;

            pnlNoiDung.BackColor = Background;

            pnlNoiDung.AutoScroll = false;

            layoutBenPhai.Controls.Add(
                pnlNoiDung, 0, 1);

            ResumeLayout(true);
            PerformLayout();

            // ========================================
            // TỰ MỞ TRANG TỔNG QUAN
            // ========================================

            if (LaAdmin || LaGiaoVien)
            {
                foreach (Control control in
                    pnlDanhMuc.Controls)
                {
                    if (control is Button btn &&
                        btn.Tag?.ToString() == "Tổng quan")
                    {
                        MoChucNang("Tổng quan", btn);
                        break;
                    }
                }
            }
        }

        // ============================================
        // 5. THIẾT KẾ HEADER
        // ============================================

        private void TaoThanhTieuDe()
        {
            pnlHeader.Controls.Clear();

            var layoutHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 2,
                RowCount = 1,

                Margin = Padding.Empty,
                Padding = Padding.Empty,

                BackColor = White
            };

            layoutHeader.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 100));

            layoutHeader.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Absolute, 240));

            layoutHeader.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100));

            pnlHeader.Controls.Add(layoutHeader);

            // ========================================
            // BÊN TRÁI: TIÊU ĐỀ TRANG
            // ========================================

            var layoutTieuDe = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,
                RowCount = 2,

                Margin = Padding.Empty,

                Padding = new Padding(
                    24, 11, 0, 8)
            };

            layoutTieuDe.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 100));

            layoutTieuDe.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 35));

            layoutTieuDe.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100));

            lblTieuDeTrang.Text = "Tổng quan";

            lblTieuDeTrang.Dock = DockStyle.Fill;

            lblTieuDeTrang.AutoSize = false;

            lblTieuDeTrang.TextAlign =
                ContentAlignment.MiddleLeft;

            lblTieuDeTrang.Font = new Font(
                "Segoe UI", 17, FontStyle.Bold);

            lblTieuDeTrang.ForeColor = TextDark;

            lblMoTaTrang.Text =
                "Theo dõi hoạt động của trung tâm";

            lblMoTaTrang.Dock = DockStyle.Fill;

            lblMoTaTrang.AutoSize = false;

            lblMoTaTrang.TextAlign =
                ContentAlignment.MiddleLeft;

            lblMoTaTrang.Font = new Font(
                "Segoe UI", 9);

            lblMoTaTrang.ForeColor = TextMuted;

            layoutTieuDe.Controls.Add(
                lblTieuDeTrang, 0, 0);

            layoutTieuDe.Controls.Add(
                lblMoTaTrang, 0, 1);

            layoutHeader.Controls.Add(
                layoutTieuDe, 0, 0);

            // ========================================
            // BÊN PHẢI: THÔNG TIN TÀI KHOẢN
            // ========================================

            var pnlTaiKhoan = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = White
            };

            var pnlAvatar = new Panel
            {
                Location = new Point(12, 18),

                Size = new Size(40, 40),

                BackColor =
                    Color.FromArgb(229, 240, 253)
            };

            var lblAvatar = new Label
            {
                Text = LaAdmin ? "A" : "GV",

                Dock = DockStyle.Fill,

                ForeColor = Blue,

                Font = new Font(
                    "Segoe UI", 11, FontStyle.Bold),

                TextAlign =
                    ContentAlignment.MiddleCenter
            };

            pnlAvatar.Controls.Add(lblAvatar);

            var lblVaiTro = new Label
            {
                Text = LaAdmin
                    ? "QUẢN TRỊ VIÊN"
                    : "GIÁO VIÊN",

                Location = new Point(65, 15),

                Size = new Size(165, 22),

                ForeColor = TextMuted,

                Font = new Font(
                    "Segoe UI", 8, FontStyle.Bold)
            };

            var lblTaiKhoan = new Label
            {
                Text = LaAdmin
                    ? "Administrator"
                    : "Tài khoản giáo viên",

                Location = new Point(65, 38),

                Size = new Size(170, 27),

                ForeColor = TextDark,

                Font = new Font(
                    "Segoe UI", 10, FontStyle.Bold)
            };

            pnlTaiKhoan.Controls.Add(pnlAvatar);
            pnlTaiKhoan.Controls.Add(lblVaiTro);
            pnlTaiKhoan.Controls.Add(lblTaiKhoan);

            layoutHeader.Controls.Add(
                pnlTaiKhoan, 1, 0);

            // Đường phân cách dưới Header
            var duongKe = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,

                BackColor =
                    Color.FromArgb(225, 232, 240)
            };

            pnlHeader.Controls.Add(duongKe);
        }

        // ============================================
        // 6. THIẾT KẾ MENU TRÁI
        // ============================================

        private void TaoMenuTrai()
        {
            pnlMenu.Controls.Clear();
            pnlDanhMuc.Controls.Clear();

            // Dùng TableLayoutPanel cố định
            // LOGO - MENU - ĐĂNG XUẤT
            var layoutMenu = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,

                ColumnCount = 1,
                RowCount = 3,

                Margin = Padding.Empty,
                Padding = Padding.Empty,

                BackColor = Navy
            };

            layoutMenu.ColumnStyles.Add(
                new ColumnStyle(
                    SizeType.Percent, 100));

            layoutMenu.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 108));

            layoutMenu.RowStyles.Add(
                new RowStyle(
                    SizeType.Percent, 100));

            layoutMenu.RowStyles.Add(
                new RowStyle(
                    SizeType.Absolute, 72));

            pnlMenu.Controls.Add(layoutMenu);

            // ========================================
            // LOGO
            // ========================================

            var pnlLogo = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                BackColor = NavyDark
            };

            var lblLogo = new Label
            {
                Text = "ENGLISH CENTER",

                Dock = DockStyle.Fill,

                TextAlign =
                    ContentAlignment.MiddleCenter,

                ForeColor = White,

                Font = new Font(
                    "Segoe UI", 16, FontStyle.Bold)
            };

            pnlLogo.Controls.Add(lblLogo);

            layoutMenu.Controls.Add(
                pnlLogo, 0, 0);

            // ========================================
            // DANH SÁCH MENU CÓ CUỘN DỌC
            // ========================================

            pnlDanhMuc.Dock = DockStyle.Fill;

            pnlDanhMuc.Margin = Padding.Empty;

            pnlDanhMuc.Padding =
                new Padding(10, 12, 0, 12);

            pnlDanhMuc.FlowDirection =
                FlowDirection.TopDown;

            pnlDanhMuc.WrapContents = false;

            pnlDanhMuc.AutoScroll = true;

            pnlDanhMuc.BackColor = Navy;

            layoutMenu.Controls.Add(
                pnlDanhMuc, 0, 1);

            // ========================================
            // ĐĂNG XUẤT
            // ========================================

            var pnlDangXuat = new Panel
            {
                Dock = DockStyle.Fill,

                Margin = Padding.Empty,

                Padding = new Padding(12),

                BackColor = NavyDark
            };

            var btnDangXuat = new Button
            {
                Text = "↪   Đăng xuất",

                Dock = DockStyle.Fill,

                BackColor = NavyHover,

                ForeColor = White,

                Font = new Font(
                    "Segoe UI", 10, FontStyle.Bold),

                FlatStyle = FlatStyle.Flat,

                Cursor = Cursors.Hand
            };

            btnDangXuat.FlatAppearance.BorderSize = 0;

            btnDangXuat.FlatAppearance.MouseOverBackColor =
                Color.FromArgb(164, 58, 68);

            btnDangXuat.Click += (_, _) =>
            {
                var ketQua = MessageBox.Show(
                    "Bạn có chắc chắn muốn đăng xuất?",
                    "Xác nhận đăng xuất",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (ketQua != DialogResult.Yes)
                    return;

                DialogResult = DialogResult.Retry;
                Close();
            };

            pnlDangXuat.Controls.Add(btnDangXuat);

            layoutMenu.Controls.Add(
                pnlDangXuat, 0, 2);

            // ========================================
            // THIẾT LẬP DANH MỤC
            // ========================================

            ThemNhanNhom("ĐIỀU HƯỚNG");

            ThemNutMenu(
                "Tổng quan", "▦");

            if (LaAdmin)
            {
                ThemNhanNhom("QUẢN LÝ TRUNG TÂM");

                ThemNutMenu(
                    "Quản lý học viên", "◉");

                ThemNutMenu(
                    "Quản lý giáo viên", "♙");

                ThemNutMenu(
                    "Quản lý khóa học", "▤");

                ThemNutMenu(
                    "Quản lý phòng học", "▣");

                ThemNutMenu(
                    "Quản lý lớp học", "▥");

                ThemNhanNhom("HỌC TẬP & GIẢNG DẠY");

                ThemNutMenu(
                    "Lịch học", "▦");

                ThemNutMenu(
                    "Quản lý lịch giáo viên", "◷");

                ThemNutMenu(
                    "Đăng ký học", "☑");

                ThemNutMenu(
                    "Học phí", "$");

                ThemNutMenu(
                    "Điểm danh", "✓");

                ThemNhanNhom("HỆ THỐNG");

                ThemNutMenu(
                    "Thống kê", "▥");

                ThemNutMenu(
                    "Quản lý tài khoản", "♙");
            }
            else if (LaGiaoVien)
            {
                ThemNhanNhom("GIẢNG DẠY");

                ThemNutMenu(
                    "Lịch dạy của tôi", "◷");

                ThemNutMenu(
                    "Điểm danh", "✓");
            }

            if (LaAdmin || LaGiaoVien)
            {
                ThemNutMenu(
                    "Đổi mật khẩu", "⚿");
            }
        }

        // ============================================
        // 7. NHÃN NHÓM MENU
        // ============================================

        private void ThemNhanNhom(string tieuDe)
        {
            var lbl = new Label
            {
                Text = tieuDe,

                Width = 214,
                Height = 31,

                AutoSize = false,

                TextAlign =
                    ContentAlignment.BottomLeft,

                Padding = new Padding(
                    12, 0, 0, 5),

                ForeColor =
                    Color.FromArgb(156, 182, 209),

                Font = new Font(
                    "Segoe UI", 8, FontStyle.Bold),

                Margin = new Padding(
                    0, 10, 0, 7)
            };

            pnlDanhMuc.Controls.Add(lbl);
        }

        // ============================================
        // 8. TẠO NÚT MENU
        // ============================================

        private Button ThemNutMenu(
            string noiDung,
            string kyHieu)
        {
            var btn = new Button
            {
                Text = $"  {kyHieu}    {noiDung}",

                Tag = noiDung,

                Width = 214,
                Height = 43,

                FlatStyle = FlatStyle.Flat,

                BackColor = Navy,

                ForeColor = MenuText,

                TextAlign =
                    ContentAlignment.MiddleLeft,

                Font = new Font(
                    "Segoe UI", 9.5f),

                Cursor = Cursors.Hand,

                Margin = new Padding(
                    0, 0, 0, 5),

                Padding = new Padding(
                    6, 0, 0, 0),

                UseVisualStyleBackColor = false
            };

            btn.FlatAppearance.BorderSize = 0;

            btn.FlatAppearance.MouseOverBackColor =
                NavyHover;

            btn.FlatAppearance.MouseDownBackColor =
                Blue;

            btn.MouseEnter += (_, _) =>
            {
                if (nutDangChon != btn)
                    btn.BackColor = NavyHover;
            };

            btn.MouseLeave += (_, _) =>
            {
                if (nutDangChon != btn)
                    btn.BackColor = Navy;
            };

            btn.Click += (_, _) =>
                MoChucNang(noiDung, btn);

            pnlDanhMuc.Controls.Add(btn);

            return btn;
        }

        // ============================================
        // 9. ĐÁNH DẤU MENU ĐANG CHỌN
        // ============================================

        private void ChonNutMenu(Button btn)
        {
            if (nutDangChon != null)
            {
                nutDangChon.BackColor = Navy;
                nutDangChon.ForeColor = MenuText;

                nutDangChon.Font = new Font(
                    "Segoe UI", 9.5f);
            }

            nutDangChon = btn;

            btn.BackColor = Blue;
            btn.ForeColor = White;

            btn.Font = new Font(
                "Segoe UI", 9.5f, FontStyle.Bold);
        }

        // ============================================
        // 10. ĐỔI TIÊU ĐỀ KHI CHỌN MENU
        // ============================================

        private void CapNhatTieuDe(string tenChucNang)
        {
            lblTieuDeTrang.Text = tenChucNang;

            lblMoTaTrang.Text = tenChucNang switch
            {
                "Tổng quan" =>
                    "Theo dõi hoạt động của trung tâm",

                "Quản lý học viên" =>
                    "Quản lý thông tin học viên và phụ huynh",

                "Quản lý giáo viên" =>
                    "Quản lý hồ sơ và phân công giảng dạy",

                "Quản lý khóa học" =>
                    "Quản lý chứng chỉ và chương trình đào tạo",

                "Quản lý phòng học" =>
                    "Quản lý phòng học và sức chứa",

                "Quản lý lớp học" =>
                    "Phân công giáo viên, phòng học và lớp",

                "Lịch học" =>
                    "Thiết lập lịch học hằng tuần",

                "Quản lý lịch giáo viên" =>
                    "Theo dõi lịch giảng dạy của từng giáo viên",

                "Lịch dạy của tôi" =>
                    "Theo dõi lịch giảng dạy cá nhân",

                "Đăng ký học" =>
                    "Đăng ký học viên vào lớp",

                "Học phí" =>
                    "Thu học phí và lịch sử thanh toán",

                "Điểm danh" =>
                    "Theo dõi việc tham gia các buổi học",

                "Thống kê" =>
                    "Thống kê và báo cáo trung tâm",

                "Quản lý tài khoản" =>
                    "Quản lý người dùng và phân quyền",

                "Đổi mật khẩu" =>
                    "Cập nhật mật khẩu tài khoản",

                _ =>
                    "Hệ thống quản lý trung tâm tiếng Anh"
            };
        }

        // ============================================
        // 11. ĐÓNG TRANG HIỆN TẠI
        // ============================================

        private void DongTrangHienTai()
        {
            if (formDangMo == null)
                return;

            pnlNoiDung.Controls.Remove(formDangMo);

            formDangMo.Dispose();

            formDangMo = null;
        }

        // ============================================
        // 12. HIỂN THỊ FORM CON
        // ============================================

        private void HienThiFormCon(Form form)
        {
            // Không cho Form con thành cửa sổ riêng.
            form.TopLevel = false;

            form.FormBorderStyle =
                FormBorderStyle.None;

            form.ShowInTaskbar = false;

            // Quan trọng: Form con luôn nằm
            // đúng bên trong vùng nội dung.
            form.Dock = DockStyle.Fill;

            // Xóa giới hạn kích thước cũ
            // để tránh bị tràn ra ngoài.
            form.MinimumSize = Size.Empty;
            form.MaximumSize = Size.Empty;

            pnlNoiDung.SuspendLayout();

            try
            {
                DongTrangHienTai();

                pnlNoiDung.Controls.Add(form);

                formDangMo = form;

                form.Show();

                form.BringToFront();
            }
            finally
            {
                pnlNoiDung.ResumeLayout(true);
            }
        }

        // ============================================
        // 13. KIỂM TRA PHÂN QUYỀN
        // ============================================

        private bool DuocPhepMo(string tenChucNang)
        {
            if (LaAdmin)
            {
                return tenChucNang !=
                    "Lịch dạy của tôi";
            }

            if (LaGiaoVien)
            {
                return
                    tenChucNang == "Tổng quan" ||
                    tenChucNang == "Lịch dạy của tôi" ||
                    tenChucNang == "Điểm danh" ||
                    tenChucNang == "Đổi mật khẩu";
            }

            return false;
        }

        // ============================================
        // 14. MỞ CHỨC NĂNG THEO MENU
        // ============================================

        private void MoChucNang(
            string tenChucNang,
            Button btn)
        {
            if (!DuocPhepMo(tenChucNang))
            {
                MessageBox.Show(
                    "Tài khoản không có quyền truy cập.",
                    "Không có quyền",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                switch (tenChucNang)
                {
                    // =================================
                    // TỔNG QUAN
                    // =================================

                    case "Tổng quan":
                        {
                            if (formDangMo is not frmTongQuan)
                            {
                                HienThiFormCon(
                                    new frmTongQuan(phien));
                            }

                            break;
                        }

                    // =================================
                    // HỌC VIÊN
                    // =================================

                    case "Quản lý học viên":
                        {
                            if (formDangMo is not frmHocVien)
                            {
                                HienThiFormCon(
                                    new frmHocVien());
                            }

                            break;
                        }

                    // =================================
                    // GIÁO VIÊN
                    // =================================

                    case "Quản lý giáo viên":
                        {
                            if (formDangMo is not frmGiaoVien)
                            {
                                HienThiFormCon(
                                    new frmGiaoVien());
                            }

                            break;
                        }

                    // =================================
                    // KHÓA HỌC
                    // =================================

                    case "Quản lý khóa học":
                        {
                            if (formDangMo is not frmKhoaHoc)
                            {
                                HienThiFormCon(
                                    new frmKhoaHoc());
                            }

                            break;
                        }

                    // =================================
                    // PHÒNG HỌC
                    // =================================

                    case "Quản lý phòng học":
                        {
                            if (formDangMo is not frmPhongHoc)
                            {
                                HienThiFormCon(
                                    new frmPhongHoc());
                            }

                            break;
                        }

                    // =================================
                    // LỚP HỌC
                    // =================================

                    case "Quản lý lớp học":
                        {
                            if (formDangMo is not frmLopHoc)
                            {
                                HienThiFormCon(
                                    new frmLopHoc());
                            }

                            break;
                        }

                    // =================================
                    // LỊCH HỌC
                    // =================================

                    case "Lịch học":
                        {
                            if (formDangMo is not frmLichHoc)
                            {
                                HienThiFormCon(
                                    new frmLichHoc());
                            }

                            break;
                        }

                    // =================================
                    // QUẢN LÝ LỊCH GIÁO VIÊN - ADMIN
                    // =================================

                    case "Quản lý lịch giáo viên":
                        {
                            if (!LaAdmin)
                            {
                                throw new InvalidOperationException(
                                    "Chỉ Admin có quyền xem " +
                                    "lịch của tất cả giáo viên.");
                            }

                            if (formDangMo
                                is not frmQuanLyLichGiaoVien)
                            {
                                HienThiFormCon(
                                    new frmQuanLyLichGiaoVien());
                            }

                            break;
                        }

                    // =================================
                    // LỊCH DẠY CỦA TÔI - GIÁO VIÊN
                    // =================================

                    case "Lịch dạy của tôi":
                        {
                            if (!LaGiaoVien)
                            {
                                throw new InvalidOperationException(
                                    "Chức năng chỉ dành cho giáo viên.");
                            }

                            if (formDangMo is not frmLichDayCuaToi)
                            {
                                HienThiFormCon(
                                    new frmLichDayCuaToi(phien));
                            }

                            break;
                        }

                    // =================================
                    // ĐĂNG KÝ HỌC
                    // =================================

                    case "Đăng ký học":
                        {
                            if (formDangMo is not frmDangKyHoc)
                            {
                                HienThiFormCon(
                                    new frmDangKyHoc());
                            }

                            break;
                        }

                    // =================================
                    // HỌC PHÍ
                    // =================================

                    case "Học phí":
                        {
                            if (formDangMo is not frmHocPhi)
                            {
                                HienThiFormCon(
                                    new frmHocPhi());
                            }

                            break;
                        }

                    // =================================
                    // ĐIỂM DANH
                    // =================================

                    case "Điểm danh":
                        {
                            if (LaAdmin)
                            {
                                if (formDangMo
                                    is not frmTheoDoiDiemDanh)
                                {
                                    HienThiFormCon(
                                        new frmTheoDoiDiemDanh(
                                            phien));
                                }
                            }
                            else if (LaGiaoVien)
                            {
                                if (formDangMo is not frmDiemDanh)
                                {
                                    HienThiFormCon(
                                        new frmDiemDanh(phien));
                                }
                            }

                            break;
                        }

                    // =================================
                    // THỐNG KÊ
                    // =================================

                    case "Thống kê":
                        {
                            if (formDangMo is not frmThongKe)
                            {
                                HienThiFormCon(
                                    new frmThongKe());
                            }

                            break;
                        }

                    // =================================
                    // QUẢN LÝ TÀI KHOẢN
                    // =================================

                    case "Quản lý tài khoản":
                        {
                            if (!LaAdmin)
                            {
                                throw new InvalidOperationException(
                                    "Chỉ Admin được quản lý tài khoản.");
                            }

                            if (formDangMo is not frmTaiKhoan)
                            {
                                HienThiFormCon(
                                    new frmTaiKhoan(phien));
                            }

                            break;
                        }

                    // =================================
                    // ĐỔI MẬT KHẨU
                    // =================================

                    case "Đổi mật khẩu":
                        {
                            if (formDangMo is not frmDoiMatKhau)
                            {
                                HienThiFormCon(
                                    new frmDoiMatKhau(phien));
                            }

                            break;
                        }

                    // =================================
                    // KHÔNG TÌM THẤY
                    // =================================

                    default:
                        {
                            MessageBox.Show(
                                $"Chưa tìm thấy chức năng '{tenChucNang}'.",
                                "Thông báo",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);

                            return;
                        }
                }

                // Chỉ tô sáng menu khi đã mở thành công.
                ChonNutMenu(btn);

                // Đồng bộ tiêu đề trang trên Header.
                CapNhatTieuDe(tenChucNang);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Không thể mở chức năng.\n\n" +
                    $"Chi tiết: {ex.Message}",
                    "Lỗi hệ thống",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
