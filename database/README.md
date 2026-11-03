# Cơ sở dữ liệu mẫu

File `QL_TrungtamTA_DB.bak` chứa đầy đủ cấu trúc và dữ liệu của 14 bảng (102 học viên, 10 giáo viên, 5 tài khoản). Chủ dự án xác nhận toàn bộ là dữ liệu mẫu.

Bản sao lưu tạo từ SQL Server 2025 (17.0.1000.7), đã kiểm tra bằng RESTORE VERIFYONLY WITH CHECKSUM. Khôi phục bằng SQL Server 2025 hoặc phiên bản mới hơn; không thể khôi phục trực tiếp trên SQL Server 2022 trở xuống.

## Cách khôi phục bằng SQL Server Management Studio (SSMS)

1. Tải file .bak và chép vào thư mục Backup của SQL Server để dịch vụ có quyền đọc.
2. Kết nối SQL Server, nhấp phải Databases → Restore Database.
3. Chọn Source: Device → chọn file QL_TrungtamTA_DB.bak.
4. Đặt tên database là QL_TrungtamTA_DB.
5. Trong Files, chọn Relocate all files to folder nếu đường dẫn máy khác máy gốc.
6. Kiểm tra đường dẫn Data/Log và bấm OK. Nếu đã có database cùng tên, hãy sao lưu database đó hoặc khôi phục dưới tên khác; không ghi đè ngoài ý muốn.
7. Mã nguồn mặc định kết nối localhost\SQLEXPRESS bằng Windows Authentication. Nếu máy dùng instance khác, sửa cấu hình trong QL_TrungtamTA.DAL/Data/TrungTamDbContext.cs.

Đây là bản sao lưu tại thời điểm xuất; các thay đổi dữ liệu sau đó cần xuất lại.
