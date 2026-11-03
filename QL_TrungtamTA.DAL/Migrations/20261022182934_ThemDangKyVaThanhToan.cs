using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QL_TrungtamTA.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ThemDangKyVaThanhToan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DangKyHoc",
                columns: table => new
                {
                    MaDK = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHV = table.Column<int>(type: "int", nullable: false),
                    MaLop = table.Column<int>(type: "int", nullable: false),
                    NgayDangKy = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HocPhiPhaiDong = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangKyHoc", x => x.MaDK);
                    table.ForeignKey(
                        name: "FK_DangKyHoc_HocVien_MaHV",
                        column: x => x.MaHV,
                        principalTable: "HocVien",
                        principalColumn: "MaHV",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DangKyHoc_LopHoc_MaLop",
                        column: x => x.MaLop,
                        principalTable: "LopHoc",
                        principalColumn: "MaLop",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ThanhToanHocPhi",
                columns: table => new
                {
                    MaTT = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDK = table.Column<int>(type: "int", nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NgayThanhToan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HinhThucTT = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThanhToanHocPhi", x => x.MaTT);
                    table.ForeignKey(
                        name: "FK_ThanhToanHocPhi_DangKyHoc_MaDK",
                        column: x => x.MaDK,
                        principalTable: "DangKyHoc",
                        principalColumn: "MaDK",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DangKyHoc_MaHV_MaLop",
                table: "DangKyHoc",
                columns: new[] { "MaHV", "MaLop" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DangKyHoc_MaLop",
                table: "DangKyHoc",
                column: "MaLop");

            migrationBuilder.CreateIndex(
                name: "IX_ThanhToanHocPhi_MaDK",
                table: "ThanhToanHocPhi",
                column: "MaDK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThanhToanHocPhi");

            migrationBuilder.DropTable(
                name: "DangKyHoc");
        }
    }
}
