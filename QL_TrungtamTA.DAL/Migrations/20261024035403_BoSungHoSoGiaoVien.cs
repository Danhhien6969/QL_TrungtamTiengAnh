using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_TrungtamTA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class BoSungHoSoGiaoVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "AnhChungChi",
                table: "GiaoVien",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiaChi",
                table: "GiaoVien",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "GiaoVien",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrinhDoHocVan",
                table: "GiaoVien",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "XepLoaiTotNghiep",
                table: "GiaoVien",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnhChungChi",
                table: "GiaoVien");

            migrationBuilder.DropColumn(
                name: "DiaChi",
                table: "GiaoVien");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "GiaoVien");

            migrationBuilder.DropColumn(
                name: "TrinhDoHocVan",
                table: "GiaoVien");

            migrationBuilder.DropColumn(
                name: "XepLoaiTotNghiep",
                table: "GiaoVien");
        }
    }
}
