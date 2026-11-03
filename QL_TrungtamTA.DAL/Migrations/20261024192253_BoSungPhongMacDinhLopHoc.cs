using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_TrungtamTA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class BoSungPhongMacDinhLopHoc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GhiChu",
                table: "LopHoc",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HocPhiRieng",
                table: "LopHoc",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaPhongMacDinh",
                table: "LopHoc",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SiSoToiThieu",
                table: "LopHoc",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LopHoc_MaPhongMacDinh",
                table: "LopHoc",
                column: "MaPhongMacDinh");

            migrationBuilder.AddForeignKey(
                name: "FK_LopHoc_PhongHoc_MaPhongMacDinh",
                table: "LopHoc",
                column: "MaPhongMacDinh",
                principalTable: "PhongHoc",
                principalColumn: "MaPhong");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LopHoc_PhongHoc_MaPhongMacDinh",
                table: "LopHoc");

            migrationBuilder.DropIndex(
                name: "IX_LopHoc_MaPhongMacDinh",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "GhiChu",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "HocPhiRieng",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "MaPhongMacDinh",
                table: "LopHoc");

            migrationBuilder.DropColumn(
                name: "SiSoToiThieu",
                table: "LopHoc");
        }
    }
}
