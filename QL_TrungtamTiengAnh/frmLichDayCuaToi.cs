using System;
using System.Drawing;
using System.Windows.Forms;
using QL_TrungtamTA.BLL;
using QL_TrungtamTA.DAL;

namespace QL_TrungtamTiengAnh.GUI
{
    // Màn hình chỉ xem: không có thao tác tạo/sửa/xóa lịch.
    public sealed class frmLichDayCuaToi : Form
    {
        private readonly PhienDangNhap phien;
        private readonly LichDayCuaToiDAL dal = new();
        private readonly DataGridView dgv = new();
        private readonly Label lblSoLuong = new();

        public frmLichDayCuaToi(PhienDangNhap phienDangNhap)
        {
            phien = phienDangNhap ?? throw new ArgumentNullException(nameof(phienDangNhap));
            if (!string.Equals(phien.TenVaiTro, "GiaoVien", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Chỉ giáo viên được xem Lịch dạy của tôi.");
            Text = "Lịch dạy của tôi";
            BackColor = Color.FromArgb(245, 248, 252);
            Font = new Font("Segoe UI", 10);
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22), RowCount = 3, ColumnCount = 1 };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.Controls.Add(new Label { Text = "LỊCH DẠY CỦA TÔI", Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(23, 52, 83), Font = new Font("Segoe UI", 22, FontStyle.Bold) }, 0, 0);
            var tools = new FlowLayoutPanel { Dock = DockStyle.Fill };
            var btn = new Button { Text = "Làm mới", Width = 125, Height = 34 };
            btn.Click += (_, _) => NapDuLieu();
            lblSoLuong.AutoSize = true;
            lblSoLuong.Margin = new Padding(20, 8, 0, 0);
            tools.Controls.Add(btn); tools.Controls.Add(lblSoLuong);
            root.Controls.Add(tools, 0, 1);
            dgv.Dock = DockStyle.Fill;
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.RowHeadersVisible = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.BackgroundColor = Color.White;
            dgv.AutoGenerateColumns = true;
            root.Controls.Add(dgv, 0, 2);
            Controls.Add(root);
            Shown += (_, _) => NapDuLieu();
        }

        private void NapDuLieu()
        {
            try
            {
                var rows = dal.LayDanhSach(phien.MaTK);
                dgv.DataSource = rows;
                if (dgv.Columns.Contains("TenLop")) dgv.Columns["TenLop"].HeaderText = "Lớp";
                if (dgv.Columns.Contains("Phong")) dgv.Columns["Phong"].HeaderText = "Phòng";
                if (dgv.Columns.Contains("Thu")) dgv.Columns["Thu"].HeaderText = "Ngày học";
                if (dgv.Columns.Contains("GioHoc")) dgv.Columns["GioHoc"].HeaderText = "Giờ học";
                if (dgv.Columns.Contains("TuNgay")) dgv.Columns["TuNgay"].HeaderText = "Áp dụng từ";
                if (dgv.Columns.Contains("DenNgay")) dgv.Columns["DenNgay"].HeaderText = "Áp dụng đến";
                lblSoLuong.Text = $"{rows.Count} lịch dạy cố định";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thể tải lịch dạy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
