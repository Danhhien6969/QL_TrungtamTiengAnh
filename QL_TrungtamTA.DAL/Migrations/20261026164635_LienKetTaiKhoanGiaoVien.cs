using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_TrungtamTA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class LienKetTaiKhoanGiaoVien : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaGV",
                table: "TaiKhoan",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_MaGV",
                table: "TaiKhoan",
                column: "MaGV",
                unique: true,
                filter: "[MaGV] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TaiKhoan_GiaoVien_MaGV",
                table: "TaiKhoan",
                column: "MaGV",
                principalTable: "GiaoVien",
                principalColumn: "MaGV",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaiKhoan_GiaoVien_MaGV",
                table: "TaiKhoan");

            migrationBuilder.DropIndex(
                name: "IX_TaiKhoan_MaGV",
                table: "TaiKhoan");

            migrationBuilder.DropColumn(
                name: "MaGV",
                table: "TaiKhoan");
        }
    }
}
