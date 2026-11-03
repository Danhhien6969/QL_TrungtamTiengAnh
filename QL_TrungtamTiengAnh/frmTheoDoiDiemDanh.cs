using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTiengAnh.GUI
{
    // Chỉ xem. Không cung cấp thao tác tạo, sửa, hủy hoặc lưu điểm danh.
    public sealed partial class frmTheoDoiDiemDanh : Form
    {
        private readonly PhienDangNhap phien;
        private readonly ThongKeDiemDanhDAL dal = new();
        private readonly DateTimePicker dtpNgay = new();
        private readonly DataGridView dgv = new();
        private readonly Label lblTong = new();

        public frmTheoDoiDiemDanh(PhienDangNhap phienDangNhap)
        {
            phien = phienDangNhap ?? throw new ArgumentNullException(nameof(phienDangNhap));
            if (phien.TenVaiTro != "Admin")
                throw new UnauthorizedAccessException("Chỉ Admin được xem tổng hợp điểm danh.");
            Text = "Theo dõi điểm danh – Admin";
            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);
            var vung = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 4
            };
            vung.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            vung.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            vung.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
            vung.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            vung.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(vung);
            vung.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "THEO DÕI ĐIỂM DANH CÁC LỚP",
                Font = new Font("Segoe UI", 21, FontStyle.Bold),
                ForeColor = Color.FromArgb(23, 52, 83),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            var thanhLoc = new FlowLayoutPanel { Dock = DockStyle.Fill };
            thanhLoc.Controls.Add(new Label
            {
                Text = "Ngày học:",
                AutoSize = true,
                Margin = new Padding(0, 10, 10, 0)
            });
            dtpNgay.Format = DateTimePickerFormat.Custom;
            dtpNgay.CustomFormat = "dd/MM/yyyy";
            dtpNgay.Width = 155;
            thanhLoc.Controls.Add(dtpNgay);
            var btn = new Button
            {
                Text = "Làm mới",
                Width = 110,
                Height = 32,
                BackColor = Color.FromArgb(32, 92, 151),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btn.Click += (_, _) => TaiDuLieu();
            thanhLoc.Controls.Add(btn);
            dtpNgay.ValueChanged += (_, _) => TaiDuLieu();
            vung.Controls.Add(thanhLoc, 0, 1);
            lblTong.Dock = DockStyle.Fill;
            lblTong.ForeColor = Color.FromArgb(23, 52, 83);
            vung.Controls.Add(lblTong, 0, 2);

            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoGenerateColumns = false;
            dgv.BackgroundColor = Color.White;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 42;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(23, 52, 83);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(243, 248, 254);
            ThemCot("TenLop", "Lớp", 100);
            ThemCot("GiaoVien", "Giáo viên phụ trách", 180);
            ThemCot("Phong", "Phòng", 65);
            ThemCot("SiSo", "Sĩ số", 70);
            ThemCot("GioHoc", "Giờ học", 95);
            ThemCot("CoMat", "Có mặt", 70);
            ThemCot("Vang", "Vắng", 60);
            ThemCot("CoPhep", "Có phép", 70);
            ThemCot("DiTre", "Đi trễ", 60);
            ThemCot("ChuaDiemDanh", "Chưa ĐD", 75);
            ThemCot("TrangThai", "Trạng thái", 125);
            dgv.CellDoubleClick += (_, e) =>
            {
                if (e.RowIndex >= 0) XemChiTiet();
            };
            vung.Controls.Add(dgv, 0, 3);
            Shown += (_, _) => TaiDuLieu();
        }

        private void ThemCot(string field, string title, float weight)
        {
            dgv.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = field,
                HeaderText = title,
                DataPropertyName = field,
                FillWeight = weight,
                ReadOnly = true
            });
        }

        private void KiemTraQuyen()
        {
            // Không chỉ tin thông tin vai trò lưu trong phiên đăng nhập.
            using var db = new QL_TrungtamTA.DAL.Data.TrungTamDbContext();
            bool hopLe = db.TaiKhoans.Any(t => t.MaTK == phien.MaTK &&
                t.TrangThai && t.VaiTro.TenVaiTro == "Admin");
            if (!hopLe) throw new UnauthorizedAccessException("Tài khoản không còn quyền Admin.");
        }

        private void TaiDuLieu()
        {
            try
            {
                KiemTraQuyen();
                var ds = dal.LayTheoNgay(dtpNgay.Value.Date);
                dgv.DataSource = ds;
                int ghiNhan = ds.Count(x => x.TrangThai == "Đã ghi nhận");
                lblTong.Text = $"{ds.Select(x => x.MaLop).Distinct().Count()} lớp  |  " +
                    $"{ds.Count(x => x.MaBuoi.HasValue)} buổi trong ngày  |  " +
                    $"{ghiNhan} buổi đã điểm danh  |  Nhấp đúp dòng để xem học viên vắng";
                dgv.ClearSelection();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, "Không thể tải điểm danh",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void XemChiTiet()
        {
            if (dgv.CurrentRow?.DataBoundItem is not DongTheoDoiDiemDanh dong ||
                !dong.MaBuoi.HasValue) return;
            try
            {
                KiemTraQuyen();
                var ds = dal.LayHocVienVang(dong.MaBuoi.Value);
                string noiDung = ds.Count == 0
                    ? "Chưa có học viên được ghi nhận vắng/có phép."
                    : string.Join(Environment.NewLine,
                        ds.Select(x => $"• {x.HocVien} – {x.TrangThai}" +
                            (string.IsNullOrWhiteSpace(x.GhiChu) ? "" : $" ({x.GhiChu})")));
                MessageBox.Show(noiDung, $"Học viên vắng – {dong.TenLop}",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.Message, "Không thể xem chi tiết",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
