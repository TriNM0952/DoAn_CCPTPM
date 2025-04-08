using Lab03.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lab03.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet khai báo các bảng
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<GioHang> GioHang { get; set; }
       
        public DbSet<HoaDon> HoaDon { get; set; }
        public DbSet<IdentityRole> Roles { get; set; }
        public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public DbSet<SalesRecord> SalesRecords { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<SupplierProduct> SupplierProducts { get; set; }
        public DbSet<SupplierProductConfig> SupplierProductConfigs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Quan hệ giữa Supplier và SupplierProduct
            modelBuilder.Entity<Supplier>()
                .HasMany(s => s.SupplierProducts)
                .WithOne(sp => sp.Supplier)
                .HasForeignKey(sp => sp.SupplierId);

            // Quan hệ 1-1 giữa SupplierProduct và SupplierProductConfig
            modelBuilder.Entity<SupplierProduct>()
                .HasOne(sp => sp.SupplierProductConfig)
                .WithOne(cfg => cfg.SupplierProduct)
                .HasForeignKey<SupplierProductConfig>(cfg => cfg.SupplierProductId)
                .OnDelete(DeleteBehavior.Cascade);

           
        }
        public DbSet<Discount> Discounts { get; set; }

    }


}
