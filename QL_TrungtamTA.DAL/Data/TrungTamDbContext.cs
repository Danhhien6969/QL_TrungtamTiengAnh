
using Microsoft.EntityFrameworkCore;
using QL_TrungtamTA.DAL.Models;

namespace QL_TrungtamTA.DAL.Data
{
    public class TrungTamDbContext : DbContext
    {
        public TrungTamDbContext()
        {
        }

        public TrungTamDbContext(
            DbContextOptions<TrungTamDbContext> options)
            : base(options)
        {
        }

        // KẾT NỐI SQL SERVER
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(
                    @"Server=localhost\SQLEXPRESS;" +
                    "Database=QL_TrungtamTA_DB;" +
                    "Trusted_Connection=True;" +
                    "TrustServerCertificate=True;");
            }
        }

        // KHAI BÁO 12 BẢNG DỮ LIỆU
        public DbSet<HocVien> HocViens
            => Set<HocVien>();

        public DbSet<GiaoVien> GiaoViens
            => Set<GiaoVien>();

        public DbSet<KhoaHoc> KhoaHocs
            => Set<KhoaHoc>();

        public DbSet<LopHoc> LopHocs
            => Set<LopHoc>();

        public DbSet<PhongHoc> PhongHocs
            => Set<PhongHoc>();

        public DbSet<LichHoc> LichHocs
            => Set<LichHoc>();

        public DbSet<DangKyHoc> DangKyHocs
            => Set<DangKyHoc>();

        public DbSet<ThanhToanHocPhi> ThanhToanHocPhis
            => Set<ThanhToanHocPhi>();

        public DbSet<BuoiHoc> BuoiHocs
            => Set<BuoiHoc>();

        public DbSet<DiemDanh> DiemDanhs
            => Set<DiemDanh>();

        public DbSet<VaiTro> VaiTros
            => Set<VaiTro>();

        public DbSet<TaiKhoan> TaiKhoans
            => Set<TaiKhoan>();

        // CẤU HÌNH BẢNG VÀ CÁC MỐI QUAN HỆ
        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. HỌC VIÊN
            modelBuilder.Entity<HocVien>(entity =>
            {
                entity.ToTable("HocVien");

                entity.HasKey(h => h.MaHV);

                entity.Property(h => h.MaHV)
                    .ValueGeneratedOnAdd();

                entity.Property(h => h.HoTen)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(h => h.NgayDangKy)
                    .HasColumnType("date");
            });

            // 2. GIÁO VIÊN
            modelBuilder.Entity<GiaoVien>(entity =>
            {
                entity.ToTable("GiaoVien");

                entity.HasKey(g => g.MaGV);

                entity.Property(g => g.MaGV)
                    .ValueGeneratedOnAdd();

                entity.Property(g => g.HoTen)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(g => g.NgayVaoLam)
                    .HasColumnType("date");
            });

            // 3. KHÓA HỌC
            modelBuilder.Entity<KhoaHoc>(entity =>
            {
                entity.ToTable("KhoaHoc");

                entity.HasKey(k => k.MaKH);

                entity.Property(k => k.MaKH)
                    .ValueGeneratedOnAdd();

                entity.Property(k => k.TenKH)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(k => k.HocPhi)
                    .HasColumnType("decimal(18,2)");
            });

            // 4. LỚP HỌC
            modelBuilder.Entity<LopHoc>(entity =>
            {
                entity.ToTable("LopHoc");

                entity.HasKey(l => l.MaLop);

                entity.Property(l => l.MaLop)
                    .ValueGeneratedOnAdd();

                entity.Property(l => l.TenLop)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(l => l.TrangThai)
                    .HasMaxLength(30);

                entity.HasOne(l => l.KhoaHoc)
                    .WithMany()
                    .HasForeignKey(l => l.MaKH)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.GiaoVien)
                    .WithMany()
                    .HasForeignKey(l => l.MaGV)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 5. PHÒNG HỌC
            modelBuilder.Entity<PhongHoc>(entity =>
            {
                entity.ToTable("PhongHoc");

                entity.HasKey(p => p.MaPhong);

                entity.Property(p => p.MaPhong)
                    .ValueGeneratedOnAdd();

                entity.Property(p => p.TenPhong)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            // 6. LỊCH HỌC
            modelBuilder.Entity<LichHoc>(entity =>
            {
                entity.ToTable("LichHoc");

                entity.HasKey(l => l.MaLH);

                entity.Property(l => l.MaLH)
                    .ValueGeneratedOnAdd();

                entity.Property(l => l.GioBatDau)
                    .HasColumnType("time");

                entity.Property(l => l.GioKetThuc)
                    .HasColumnType("time");

                entity.HasOne(l => l.LopHoc)
                    .WithMany(l => l.LichHocs)
                    .HasForeignKey(l => l.MaLop)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.PhongHoc)
                    .WithMany(p => p.LichHocs)
                    .HasForeignKey(l => l.MaPhong)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 7. ĐĂNG KÝ HỌC
            modelBuilder.Entity<DangKyHoc>(entity =>
            {
                entity.ToTable("DangKyHoc");

                entity.HasKey(d => d.MaDK);

                entity.Property(d => d.MaDK)
                    .ValueGeneratedOnAdd();

                entity.Property(d => d.HocPhiPhaiDong)
                    .HasColumnType("decimal(18,2)");

                entity.Property(d => d.TrangThai)
                    .IsRequired()
                    .HasMaxLength(30);

                // Không đăng ký trùng lớp
                entity.HasIndex(d => new
                {
                    d.MaHV,
                    d.MaLop
                }).IsUnique();

                entity.HasOne(d => d.HocVien)
                    .WithMany()
                    .HasForeignKey(d => d.MaHV)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.LopHoc)
                    .WithMany()
                    .HasForeignKey(d => d.MaLop)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 8. THANH TOÁN HỌC PHÍ
            modelBuilder.Entity<ThanhToanHocPhi>(entity =>
            {
                entity.ToTable("ThanhToanHocPhi");

                entity.HasKey(t => t.MaTT);

                entity.Property(t => t.MaTT)
                    .ValueGeneratedOnAdd();

                entity.Property(t => t.SoTien)
                    .HasColumnType("decimal(18,2)");

                entity.Property(t => t.HinhThucTT)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(t => t.GhiChu)
                    .HasMaxLength(255);

                entity.HasOne(t => t.DangKyHoc)
                    .WithMany(d => d.ThanhToanHocPhis)
                    .HasForeignKey(t => t.MaDK)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 9. BUỔI HỌC
            modelBuilder.Entity<BuoiHoc>(entity =>
            {
                entity.ToTable("BuoiHoc");

                entity.HasKey(b => b.MaBuoi);

                entity.Property(b => b.MaBuoi)
                    .ValueGeneratedOnAdd();

                entity.Property(b => b.NgayHoc)
                    .HasColumnType("date");

                entity.Property(b => b.GioBatDau)
                    .HasColumnType("time");

                entity.Property(b => b.GioKetThuc)
                    .HasColumnType("time");

                entity.Property(b => b.TrangThai)
                    .IsRequired()
                    .HasMaxLength(30);

                // Ngăn trùng buổi học cùng ngày, cùng giờ
                entity.HasIndex(b => new
                {
                    b.MaLop,
                    b.NgayHoc,
                    b.GioBatDau
                }).IsUnique();

                entity.HasOne(b => b.LopHoc)
                    .WithMany()
                    .HasForeignKey(b => b.MaLop)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 10. ĐIỂM DANH
            modelBuilder.Entity<DiemDanh>(entity =>
            {
                entity.ToTable("DiemDanh");

                entity.HasKey(d => d.MaDD);

                entity.Property(d => d.MaDD)
                    .ValueGeneratedOnAdd();

                entity.Property(d => d.TrangThai)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(d => d.GhiChu)
                    .HasMaxLength(255);

                // Một học viên chỉ điểm danh 1 lần/buổi
                entity.HasIndex(d => new
                {
                    d.MaBuoi,
                    d.MaHV
                }).IsUnique();

                entity.HasOne(d => d.BuoiHoc)
                    .WithMany(b => b.DiemDanhs)
                    .HasForeignKey(d => d.MaBuoi)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.HocVien)
                    .WithMany()
                    .HasForeignKey(d => d.MaHV)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // 11. VAI TRÒ
            modelBuilder.Entity<VaiTro>(entity =>
            {
                entity.ToTable("VaiTro");

                entity.HasKey(v => v.MaVT);

                entity.Property(v => v.MaVT)
                    .ValueGeneratedOnAdd();

                entity.Property(v => v.TenVaiTro)
                    .IsRequired()
                    .HasMaxLength(50);

                // Không được trùng tên vai trò
                entity.HasIndex(v => v.TenVaiTro)
                    .IsUnique();
            });

            // 12. TÀI KHOẢN
            modelBuilder.Entity<TaiKhoan>(entity =>
            {
                entity.ToTable("TaiKhoan");

                entity.HasKey(t => t.MaTK);

                entity.Property(t => t.MaTK)
                    .ValueGeneratedOnAdd();

                entity.Property(t => t.TenDangNhap)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(t => t.MatKhauHash)
                    .IsRequired()
                    .HasMaxLength(255);

                // Không được trùng tên đăng nhập
                entity.HasIndex(t => t.TenDangNhap)
                    .IsUnique();

                entity.HasOne(t => t.VaiTro)
                    .WithMany(v => v.TaiKhoans)
                    .HasForeignKey(t => t.MaVT)
                    .OnDelete(DeleteBehavior.Restrict);

                // Một giáo viên chỉ được liên kết với tối đa một tài khoản.
                entity.HasOne(t => t.GiaoVien)
                    .WithOne()
                    .HasForeignKey<TaiKhoan>(t => t.MaGV)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(t => t.MaGV)
                    .IsUnique()
                    .HasFilter("[MaGV] IS NOT NULL");
            });
        }
    }
}
