using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<AspNetUsers>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
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

    public DbSet<AspNetRoles> AspNetRoles { get; set; } = null!;
    public DbSet<AspNetUsers> AspNetUsers { get; set; } = null!;
    public DbSet<Products> Products { get; set; }

    //SYSTEM
    public DbSet<SysApi> SysApis { get; set; } = null!;
    //public DbSet<SysConfiguration> SysConfigurations { get; set; } = null!;
    //public DbSet<SysFile> SysFile { get; set; } = null!;
    //public DbSet<SysFunction> SysFunctions { get; set; } = null!;
    //public DbSet<SysLanguage> SysLanguage { get; set; } = null!;
    //public DbSet<SysStatisticalAccess> SysStatisticalAccesses { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Cấu hình mối quan hệ nếu cần
        builder.Entity<Products>()
            .HasOne(p => p.ProductType)
            .WithMany(pt => pt.Products)
            .HasForeignKey(p => p.ProductTypeId);

        builder.Entity<Products>()
            .HasOne(p => p.Branch)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BranchId);

        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId);
    }
}