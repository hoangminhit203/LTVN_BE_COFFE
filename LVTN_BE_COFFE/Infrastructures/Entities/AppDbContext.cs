using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        // Bảng sản phẩm
        public DbSet<Products> Products { get; set; }
        // Bảng loại sản phẩm (trà sữa, topping, đồ ăn kèm…)
        public DbSet<ProductType> ProductTypes { get; set; }

        // Bảng chi nhánh (mỗi quán/chi nhánh)
        public DbSet<Branch> Branches { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Quan hệ: ProductType 1 - nhiều Products
            builder.Entity<ProductType>()
                .HasMany(pt => pt.Products)
                .WithOne(p => p.ProductType)
                .HasForeignKey(p => p.ProductTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ: Branch 1 - nhiều Products
            builder.Entity<Branch>()
                .HasMany(b => b.Products)
                .WithOne(p => p.Branch)
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
