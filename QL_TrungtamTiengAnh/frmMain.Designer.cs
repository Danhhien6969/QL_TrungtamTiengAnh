
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace QL_TrungtamTiengAnh.GUI
{
    partial class frmMain
    {
        private IContainer? components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            SuspendLayout();

            // =====================================
            // FRMMAIN - CẤU HÌNH GIAO DIỆN
            // =====================================

            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;

            // Kích thước thiết kế ban đầu.
            ClientSize = new Size(1360, 800);

            // Giới hạn kích thước tối thiểu.
            MinimumSize = new Size(1050, 650);

            // Cho phép thay đổi kích thước cửa sổ.
            FormBorderStyle = FormBorderStyle.Sizable;

            // Hiển thị ở giữa màn hình.
            StartPosition = FormStartPosition.CenterScreen;

            // Có thể phóng to hoặc thu nhỏ.
            MaximizeBox = true;
            MinimizeBox = true;

            // Mở ứng dụng ở trạng thái toàn màn hình.
            WindowState = FormWindowState.Maximized;

            // Font mặc định đồng bộ.
            Font = new Font(
                "Segoe UI",
                10F,
                FontStyle.Regular,
                GraphicsUnit.Point);

            // Màu nền xanh xám rất nhạt.
            BackColor = Color.FromArgb(245, 248, 252);

            // Màu chữ mặc định.
            ForeColor = Color.FromArgb(30, 48, 70);

            // Tránh nhấp nháy khi vẽ lại giao diện.
            DoubleBuffered = true;

            // Các thuộc tính hỗ trợ hiển thị.
            AutoScroll = false;
            KeyPreview = true;

            // Tên Form.
            Name = "frmMain";

            // Tiêu đề cửa sổ.
            Text = "ENGLISH CENTER | Hệ thống quản lý trung tâm tiếng Anh";

            ResumeLayout(false);
        }

        #endregion
    }
}
