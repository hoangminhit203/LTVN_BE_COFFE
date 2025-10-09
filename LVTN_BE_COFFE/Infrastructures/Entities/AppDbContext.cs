using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class AppDbContext : IdentityDbContext<AspNetUsers>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Entities
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Topping> Toppings { get; set; }
        public DbSet<ProductVariantTopping> ProductVariantToppings { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Category 1 - N Product
            builder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Branch 1 - N Product
            builder.Entity<Branch>()
                .HasMany(b => b.Products)
                .WithOne(p => p.Branch)
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product 1 - N ProductVariant
            builder.Entity<Product>()
                .HasMany(p => p.Variants)
                .WithOne(v => v.Product)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Size 1 - N ProductVariant
            builder.Entity<Size>()
                .HasMany(s => s.Variants)
                .WithOne(v => v.Size)
                .HasForeignKey(v => v.SizeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-Many ProductVariant ↔ Topping
            builder.Entity<ProductVariantTopping>()
                .HasKey(pvt => new { pvt.ProductVariantId, pvt.ToppingId });

            builder.Entity<ProductVariantTopping>()
                .HasOne(pvt => pvt.ProductVariant)
                .WithMany(pv => pv.ProductVariantToppings)
                .HasForeignKey(pvt => pvt.ProductVariantId);

            builder.Entity<ProductVariantTopping>()
                .HasOne(pvt => pvt.Topping)
                .WithMany(t => t.ProductVariantToppings)
                .HasForeignKey(pvt => pvt.ToppingId);
        }
    }
}
