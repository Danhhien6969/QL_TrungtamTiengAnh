using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_TrungtamTA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class BoSungChungChiLoTrinh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoaiKhoa",
                table: "KhoaHoc",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SiSoToiDa",
                table: "KhoaHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoBuoi",
                table: "KhoaHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SoGioMoiBuoi",
                table: "KhoaHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ThoiLuongThang",
                table: "KhoaHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrinhDoDauRa",
                table: "KhoaHoc",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TrinhDoDauVao",
                table: "KhoaHoc",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoaiKhoa",
                table: "KhoaHoc");

            migrationBuilder.DropColumn(
                name: "SiSoToiDa",
                table: "KhoaHoc");

            migrationBuilder.DropColumn(
                name: "SoBuoi",
                table: "KhoaHoc");

            migrationBuilder.DropColumn(
                name: "SoGioMoiBuoi",
                table: "KhoaHoc");

            migrationBuilder.DropColumn(
                name: "ThoiLuongThang",
                table: "KhoaHoc");

            migrationBuilder.DropColumn(
                name: "TrinhDoDauRa",
                table: "KhoaHoc");

            migrationBuilder.DropColumn(
                name: "TrinhDoDauVao",
                table: "KhoaHoc");
        }
    }
}
