using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTiengAnh.GUI
{
    public partial class frmThongKe : Form
    {
        private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");
        private readonly ThongKeBLL bll = new();
        private readonly NumericUpDown nudNam = new();
        private readonly Panel pnlBieuDo = new();
        private readonly DataGridView dgvKhoaHoc = new();
        private readonly Label[] lblChiSo = new Label[8];
        private DuLieuThongKe? baoCao;

        public frmThongKe()
        {
            InitializeComponent();
            TaoGiaoDien();
            Shown += (_, _) => XuLyAnToan(NapBaoCao);
        }

        private void TaoGiaoDien()
        {
            Text = "Thống kê - Báo cáo";
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(900, 620);
            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);
            Controls.Clear();

            var khungCuon = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(245, 248, 252)
            };
            Controls.Add(khungCuon);

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 855,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(18, 16, 18, 10),
                BackColor = Color.FromArgb(245, 248, 252)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 83));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 240));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 360));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            khungCuon.Controls.Add(layout);

            // Tiêu đề
            var header = new Panel { Dock = DockStyle.Fill };
            header.Controls.Add(new Label
            {
                Text = "THỐNG KÊ - BÁO CÁO",
                Location = new Point(5, 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            });
            header.Controls.Add(new Label
            {
                Text = "Tổng hợp dữ liệu trung tâm và doanh thu học phí",
                Location = new Point(7, 48),
                AutoSize = true,
                ForeColor = Color.DimGray
            });
            layout.Controls.Add(header, 0, 0);

            // Bộ lọc năm và công cụ
            var congCu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(5, 8, 0, 3)
            };
            congCu.Controls.Add(new Label
            {
                Text = "Năm báo cáo:",
                AutoSize = true,
                Margin = new Padding(0, 10, 9, 0),
                ForeColor = Color.FromArgb(40, 52, 70)
            });
            nudNam.Minimum = 2000;
            nudNam.Maximum = 2100;
            nudNam.Value = Math.Clamp(DateTime.Today.Year, 2000, 2100);
            nudNam.Width = 105;
            nudNam.Height = 35;
            nudNam.Font = new Font("Segoe UI", 11);
            nudNam.Margin = new Padding(0, 3, 10, 3);
            congCu.Controls.Add(nudNam);

            Button btnTaiLai = TaoNut("Xem thống kê", Color.FromArgb(30, 105, 172), 155);
            btnTaiLai.Click += (_, _) => XuLyAnToan(NapBaoCao);
            congCu.Controls.Add(btnTaiLai);

            Button btnXuatCsv = TaoNut("Xuất báo cáo CSV", Color.FromArgb(28, 130, 90), 175);
            btnXuatCsv.Click += (_, _) => XuLyAnToan(XuatCsv);
            congCu.Controls.Add(btnXuatCsv);
            layout.Controls.Add(congCu, 0, 1);

            // Các thẻ chỉ số
            var luoiThe = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 2
            };
            for (int i = 0; i < 4; i++)
                luoiThe.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            luoiThe.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            luoiThe.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            string[] tenChiSo =
            {
                "HỌC VIÊN", "GIÁO VIÊN", "KHÓA HỌC", "LỚP HỌC",
                "ĐĂNG KÝ HỢP LỆ", "DOANH THU TRONG NĂM", "CÔNG NỢ HIỆN TẠI", "GIAO DỊCH TRONG NĂM"
            };
            for (int i = 0; i < 8; i++)
            {
                var (the, giaTri) = TaoTheChiSo(tenChiSo[i]);
                lblChiSo[i] = giaTri;
                luoiThe.Controls.Add(the, i % 4, i / 4);
            }
            layout.Controls.Add(luoiThe, 0, 2);

            // Biểu đồ doanh thu 12 tháng và bảng khóa học nổi bật
            var hangBaoCao = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            hangBaoCao.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 63));
            hangBaoCao.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 37));
            hangBaoCao.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var pnlBieuDoNgoai = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12),
                Margin = new Padding(5),
                ColumnCount = 1,
                RowCount = 2
            };
            pnlBieuDoNgoai.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlBieuDoNgoai.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
            pnlBieuDoNgoai.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var lblTieuDeBieuDo = new Label
            {
                Text = "DOANH THU 12 THÁNG (VNĐ)",
                Dock = DockStyle.Top,
                Height = 37,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            };
            pnlBieuDo.Dock = DockStyle.Fill;
            pnlBieuDo.BackColor = Color.White;
            pnlBieuDo.Paint += VeBieuDo;
            pnlBieuDo.Resize += (_, _) => pnlBieuDo.Invalidate();
            pnlBieuDoNgoai.Controls.Add(lblTieuDeBieuDo, 0, 0);
            pnlBieuDoNgoai.Controls.Add(pnlBieuDo, 0, 1);
            hangBaoCao.Controls.Add(pnlBieuDoNgoai, 0, 0);

            var pnlKhoaHoc = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(12),
                Margin = new Padding(5),
                ColumnCount = 1,
                RowCount = 3
            };
            pnlKhoaHoc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pnlKhoaHoc.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
            pnlKhoaHoc.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            pnlKhoaHoc.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            var lblTieuDeTop = new Label
            {
                Text = "TOP KHÓA HỌC THEO ĐĂNG KÝ",
                Dock = DockStyle.Top,
                Height = 37,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83)
            };
            var lblGhiChuTop = new Label
            {
                Text = "Tính trên các đăng ký chưa hủy, tất cả các năm.",
                Dock = DockStyle.Bottom,
                Height = 36,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.DimGray
            };
            CauHinhBangTop();
            pnlKhoaHoc.Controls.Add(lblTieuDeTop, 0, 0);
            pnlKhoaHoc.Controls.Add(dgvKhoaHoc, 0, 1);
            pnlKhoaHoc.Controls.Add(lblGhiChuTop, 0, 2);
            hangBaoCao.Controls.Add(pnlKhoaHoc, 1, 0);
            layout.Controls.Add(hangBaoCao, 0, 3);

            layout.Controls.Add(new Label
            {
                Text = "Ghi chú: Tổng học viên, giáo viên, khóa học, lớp học và công nợ là số liệu hiện tại. " +
                       "Doanh thu và số giao dịch được lọc theo năm đã chọn. " +
                       "Doanh thu là tiền thực nhận, không phải học phí phải thu.",
                Dock = DockStyle.Fill,
                Padding = new Padding(6, 8, 4, 0),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray
            }, 0, 4);
        }

        private static Button TaoNut(string noiDung, Color mau, int rong)
        {
            var btn = new Button
            {
                Text = noiDung,
                Width = rong,
                Height = 38,
                BackColor = mau,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 2, 10, 3)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static (Control, Label) TaoTheChiSo(string nhan)
        {
            var the = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.White,
                Margin = new Padding(5, 4, 5, 5),
                Padding = new Padding(14, 10, 10, 10)
            };
            the.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            the.RowStyles.Add(new RowStyle(SizeType.Percent, 46));
            the.RowStyles.Add(new RowStyle(SizeType.Percent, 54));
            the.Controls.Add(new Label
            {
                Text = nhan,
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(95, 110, 128),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            }, 0, 0);
            var lbl = new Label
            {
                Text = "—",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83),
                AutoEllipsis = true
            };
            the.Controls.Add(lbl, 0, 1);
            return (the, lbl);
        }

        private void CauHinhBangTop()
        {
            dgvKhoaHoc.Dock = DockStyle.Fill;
            dgvKhoaHoc.BackgroundColor = Color.White;
            dgvKhoaHoc.BorderStyle = BorderStyle.None;
            dgvKhoaHoc.AutoGenerateColumns = false;
            dgvKhoaHoc.ReadOnly = true;
            dgvKhoaHoc.AllowUserToAddRows = false;
            dgvKhoaHoc.AllowUserToDeleteRows = false;
            dgvKhoaHoc.AllowUserToResizeRows = false;
            dgvKhoaHoc.RowHeadersVisible = false;
            dgvKhoaHoc.MultiSelect = false;
            dgvKhoaHoc.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvKhoaHoc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvKhoaHoc.EnableHeadersVisualStyles = false;
            dgvKhoaHoc.ColumnHeadersHeight = 40;
            dgvKhoaHoc.RowTemplate.Height = 34;
            dgvKhoaHoc.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 52, 83);
            dgvKhoaHoc.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvKhoaHoc.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvKhoaHoc.DefaultCellStyle.SelectionBackColor = Color.FromArgb(212, 233, 250);
            dgvKhoaHoc.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvKhoaHoc.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);
            dgvKhoaHoc.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Tên khóa học",
                DataPropertyName = nameof(KhoaHocNhieuDangKy.TenKhoaHoc),
                FillWeight = 78
            });
            dgvKhoaHoc.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Đăng ký",
                DataPropertyName = nameof(KhoaHocNhieuDangKy.SoDangKy),
                FillWeight = 22,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });
        }

        private static string Tien(decimal giaTri) => giaTri.ToString("N0", Vi) + " ₫";

        private void NapBaoCao()
        {
            var moi = bll.LayThongKe((int)nudNam.Value);
            baoCao = moi;
            lblChiSo[0].Text = moi.TongHocVien.ToString("N0", Vi);
            lblChiSo[1].Text = moi.TongGiaoVien.ToString("N0", Vi);
            lblChiSo[2].Text = moi.TongKhoaHoc.ToString("N0", Vi);
            lblChiSo[3].Text = moi.TongLopHoc.ToString("N0", Vi);
            lblChiSo[4].Text = moi.TongDangKyHopLe.ToString("N0", Vi);
            lblChiSo[5].Text = Tien(moi.DoanhThuTrongNam);
            lblChiSo[6].Text = Tien(moi.CongNoHienTai);
            lblChiSo[7].Text = moi.SoGiaoDichTrongNam.ToString("N0", Vi);
            dgvKhoaHoc.DataSource = null;
            dgvKhoaHoc.DataSource = moi.TopKhoaHoc;
            dgvKhoaHoc.ClearSelection();
            pnlBieuDo.Invalidate();
        }

        // Biểu đồ vẽ bằng GDI+ thuần WinForms: không cần cài Chart package.
        private void VeBieuDo(object? sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);

            if (baoCao == null || pnlBieuDo.ClientSize.Width < 150 ||
                pnlBieuDo.ClientSize.Height < 100)
                return;

            var duLieu = baoCao.DoanhThu12Thang;
            decimal giaTriLonNhat = duLieu.Count == 0 ? 0m :
                Math.Max(0m, duLieu.Max(x => x.DoanhThu));

            int trai = 34;
            int phai = 15;
            int tren = 30;
            int duoi = 42;
            int rong = Math.Max(1, pnlBieuDo.ClientSize.Width - trai - phai);
            int cao = Math.Max(1, pnlBieuDo.ClientSize.Height - tren - duoi);
            int gocY = tren + cao;

            using var butDuong = new Pen(Color.FromArgb(230, 235, 240));
            using var nenCot = new SolidBrush(Color.FromArgb(46, 121, 192));
            using var chu = new SolidBrush(Color.FromArgb(80, 95, 113));
            using var fontTruc = new Font("Segoe UI", 8);
            using var fontSo = new Font("Segoe UI", 7.5f, FontStyle.Bold);

            // Lưới ngang hỗ trợ so sánh tương đối.
            for (int i = 0; i <= 4; i++)
            {
                float y = tren + (cao * i / 4f);
                g.DrawLine(butDuong, trai, y, trai + rong, y);
            }

            float buoc = rong / 12f;
            float rongCot = Math.Max(5f, Math.Min(40f, buoc * 0.55f));
            for (int i = 0; i < 12; i++)
            {
                decimal soTien = duLieu[i].DoanhThu;
                float x = trai + buoc * i + (buoc - rongCot) / 2f;
                float tyLe = giaTriLonNhat > 0 ? (float)(soTien / giaTriLonNhat) : 0f;
                float chieuCao = Math.Max(0f, tyLe * (cao - 17));
                if (chieuCao > 0)
                    g.FillRectangle(nenCot, x, gocY - chieuCao, rongCot, chieuCao);

                string nhanThang = (i + 1).ToString();
                SizeF sizeThang = g.MeasureString(nhanThang, fontTruc);
                g.DrawString(nhanThang, fontTruc, chu,
                    x + (rongCot - sizeThang.Width) / 2f, gocY + 9);

                // Chỉ vẽ nhãn giá trị khi biểu đồ đủ rộng để tránh chồng chữ.
                if (soTien > 0 && buoc >= 42)
                {
                    string nhanSo = VietTat(soTien);
                    SizeF kichThuoc = g.MeasureString(nhanSo, fontSo);
                    g.DrawString(nhanSo, fontSo, chu,
                        x + (rongCot - kichThuoc.Width) / 2f,
                        gocY - chieuCao - kichThuoc.Height - 3);
                }
            }
            if (giaTriLonNhat <= 0)
            {
                const string chuaCo = "Chưa có giao dịch thanh toán trong năm này";
                SizeF kt = g.MeasureString(chuaCo, Font);
                g.DrawString(chuaCo, Font, chu,
                    Math.Max(0, (pnlBieuDo.Width - kt.Width) / 2f),
                    Math.Max(0, tren + (cao - kt.Height) / 2f));
            }
        }

        private static string VietTat(decimal soTien)
        {
            if (soTien >= 1_000_000_000m)
                return (soTien / 1_000_000_000m).ToString("0.#", Vi) + " tỷ";
            if (soTien >= 1_000_000m)
                return (soTien / 1_000_000m).ToString("0.#", Vi) + "tr";
            if (soTien >= 1_000m)
                return (soTien / 1_000m).ToString("0.#", Vi) + "k";
            return soTien.ToString("N0", Vi);
        }

        private void XuatCsv()
        {
            if (baoCao == null)
                throw new InvalidOperationException("Chưa có dữ liệu để xuất. Hãy nhấn Xem thống kê.");

            using var save = new SaveFileDialog
            {
                Title = "Lưu báo cáo thống kê",
                Filter = "CSV (*.csv)|*.csv",
                DefaultExt = "csv",
                AddExtension = true,
                FileName = $"ThongKe_TrungTam_{baoCao.Nam}.csv"
            };
            if (save.ShowDialog(this) != DialogResult.OK)
                return;

            static string Esc(string giaTri)
            {
                // Không để nội dung người dùng nhập bị Excel hiểu thành công thức.
                string kiemTra = giaTri.TrimStart();
                if (kiemTra.StartsWith("=") || kiemTra.StartsWith("+") ||
                    kiemTra.StartsWith("-") || kiemTra.StartsWith("@"))
                    giaTri = "'" + giaTri;
                return "\"" + giaTri.Replace("\"", "\"\"") + "\"";
            }
            using (var writer = new StreamWriter(save.FileName, false, new UTF8Encoding(true)))
            {
                writer.WriteLine(Esc($"BÁO CÁO TRUNG TÂM - NĂM {baoCao.Nam}"));
                writer.WriteLine("Chỉ tiêu;Giá trị");
                writer.WriteLine($"Học viên;{baoCao.TongHocVien}");
                writer.WriteLine($"Giáo viên;{baoCao.TongGiaoVien}");
                writer.WriteLine($"Khóa học;{baoCao.TongKhoaHoc}");
                writer.WriteLine($"Lớp học;{baoCao.TongLopHoc}");
                writer.WriteLine($"Đăng ký hợp lệ;{baoCao.TongDangKyHopLe}");
                writer.WriteLine($"Doanh thu trong năm (VND);{baoCao.DoanhThuTrongNam.ToString("0.##", CultureInfo.InvariantCulture)}");
                writer.WriteLine($"Công nợ hiện tại (VND);{baoCao.CongNoHienTai.ToString("0.##", CultureInfo.InvariantCulture)}");
                writer.WriteLine($"Giao dịch trong năm;{baoCao.SoGiaoDichTrongNam}");
                writer.WriteLine();
                writer.WriteLine("Tháng;Doanh thu (VND);Số giao dịch");
                foreach (var t in baoCao.DoanhThu12Thang)
                    writer.WriteLine($"{t.Thang};{t.DoanhThu.ToString("0.##", CultureInfo.InvariantCulture)};{t.SoGiaoDich}");
                writer.WriteLine();
                writer.WriteLine("Khóa học;Số đăng ký chưa hủy (tất cả các năm)");
                foreach (var k in baoCao.TopKhoaHoc)
                    writer.WriteLine($"{Esc(k.TenKhoaHoc)};{k.SoDangKy}");
            }
            MessageBox.Show("Xuất báo cáo CSV thành công!", "Thành công",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void XuLyAnToan(Action thaoTac)
        {
            try { thaoTac(); }
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
                MessageBox.Show("Không thể đọc hoặc xuất báo cáo. Vui lòng kiểm tra SQL Server và thử lại.",
                    "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
