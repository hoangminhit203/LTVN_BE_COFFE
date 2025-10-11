using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

public class AppDbContext : IdentityDbContext<AspNetUsers, AspNetRoles, string>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Identity
    public DbSet<AspNetRoles> AspNetRoles { get; set; } = null!;
    public DbSet<AspNetUsers> AspNetUsers { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    // System
    public DbSet<SysApi> SysApis { get; set; } = null!;

    // Coffee Shop Entities
    public DbSet<Branch> Branches { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<ProductVariant> ProductVariants { get; set; } = null!;
    public DbSet<Topping> Toppings { get; set; } = null!;
    public DbSet<ProductVariantTopping> ProductVariantToppings { get; set; } = null!;
    public DbSet<Size> Sizes { get; set; } = null!;

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

        // RefreshToken ↔ User
        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId);
    }

    public override int SaveChanges()
    {
        SetAuditFields();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditFields()
    {
        var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";

        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is BaseEntity &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            if (entry.State == EntityState.Added)
            {
                entity.CreatedDate = DateTime.Now;
                entity.CreatedBy = currentUser;
            }
            if (entry.State == EntityState.Modified)
            {
                entity.UpdatedDate = DateTime.Now;
                entity.UpdatedBy = currentUser;
            }
        }
    }
}
