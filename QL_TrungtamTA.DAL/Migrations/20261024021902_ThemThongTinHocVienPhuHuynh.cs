using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_TrungtamTA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ThemThongTinHocVienPhuHuynh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HoTenPhuHuynh",
                table: "HocVien",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LopTruong",
                table: "HocVien",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SoDienThoaiPhuHuynh",
                table: "HocVien",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tuoi",
                table: "HocVien",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HoTenPhuHuynh",
                table: "HocVien");

            migrationBuilder.DropColumn(
                name: "LopTruong",
                table: "HocVien");

            migrationBuilder.DropColumn(
                name: "SoDienThoaiPhuHuynh",
                table: "HocVien");

            migrationBuilder.DropColumn(
                name: "Tuoi",
                table: "HocVien");
        }
    }
}
