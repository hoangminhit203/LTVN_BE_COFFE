using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

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

    public DbSet<Banner> Banners { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariant { get; set; }
    public DbSet<FlavorNote> FlavorNotes { get; set; }
    public DbSet<ProductFlavorNote> ProductFlavorNotes { get; set; }
    public DbSet<BrewingMethod> BrewingMethods { get; set; }
    public DbSet<ProductImage> ProductImage { get; set; }
    public DbSet<ProductBrewingMethod> ProductBrewingMethods { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Wishlist> Wishlists { get; set; }
    public DbSet<Promotion> Promotions { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<RefreshToken> RefreshToken { get; set; }
    public DbSet<ShippingAddress> ShippingAddresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AspNetUsers>()
            .HasIndex(u => u.UserName)
            .IsUnique();
        modelBuilder.Entity<AspNetUsers>()
            .HasIndex(u => u.Email)
            .IsUnique();
        modelBuilder.Entity<AspNetUsers>()
            .HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        modelBuilder.Entity<Banner>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.ImageUrl).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasIndex(p => p.Name).IsUnique(false);

            entity.Property(p => p.Name)
                  .HasMaxLength(255).IsRequired();

            entity.HasMany(p => p.Variants)
                  .WithOne(v => v.Product)
                  .HasForeignKey(v => v.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Images)
                  .WithOne(i => i.Product)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.ProductFlavorNotes)
                  .WithOne(fn => fn.Product)
                  .HasForeignKey(fn => fn.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.ProductBrewingMethods)
                  .WithOne(bm => bm.Product)
                  .HasForeignKey(bm => bm.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Categories)
                  .WithMany(c => c.Products);

        });
        modelBuilder.Entity<ProductImage>(entity =>
        {

            entity.HasOne(i => i.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.ProductVariant)
                  .WithMany(v => v.Images)
                  .HasForeignKey(i => i.ProductVariantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.HasIndex(v => v.Sku).IsUnique(true);

            entity.Property(v => v.Sku)
                  .HasMaxLength(100).IsRequired();

            entity.Property(v => v.Price)
                  .HasColumnType("decimal(18,2)").IsRequired();

            entity.Property(v => v.Stock).IsRequired();

            entity.HasIndex(v => new { v.ProductId, v.RoastLevel, v.BeanType });

            entity.HasMany(v => v.Images)
                  .WithOne(i => i.ProductVariant)
                  .HasForeignKey(i => i.ProductVariantId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(p => p.Reviews)
                  .WithOne(r => r.Variant)
                  .HasForeignKey(r => r.VariantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });


        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(i => i.PublicId).HasMaxLength(255);

            entity.HasOne(i => i.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.ProductVariant)
                  .WithMany(v => v.Images)
                  .HasForeignKey(i => i.ProductVariantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FlavorNote>(entity =>
        {
            entity.HasKey(fn => fn.Id);
        });

        modelBuilder.Entity<ProductFlavorNote>(entity =>
        {
            entity.HasKey(pfn => new { pfn.ProductId, pfn.FlavorNoteId });

            entity.HasOne(pfn => pfn.Product)
                  .WithMany(p => p.ProductFlavorNotes)
                  .HasForeignKey(pfn => pfn.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(pfn => pfn.FlavorNote)
                  .WithMany(fn => fn.ProductFlavorNotes)
                  .HasForeignKey(pfn => pfn.FlavorNoteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BrewingMethod>(entity =>
        {
            entity.HasKey(bm => bm.Id);
        });

        modelBuilder.Entity<ProductBrewingMethod>(entity =>
        {
            entity.HasKey(pbm => new { pbm.ProductId, pbm.BrewingMethodId });

            entity.HasOne(pbm => pbm.Product)
                  .WithMany(p => p.ProductBrewingMethods)
                  .HasForeignKey(pbm => pbm.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pbm => pbm.BrewingMethod)
                  .WithMany(bm => bm.ProductBrewingMethods)
                  .HasForeignKey(pbm => pbm.BrewingMethodId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasOne(c => c.User)
                  .WithMany()
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasOne(ci => ci.Cart)
                  .WithMany(c => c.CartItems)
                  .HasForeignKey(ci => ci.CartId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(ci => ci.ProductVariant)
                  .WithMany(v => v.CartItems)
                  .HasForeignKey(ci => ci.ProductVariantId)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasOne(o => o.User)
                  .WithMany()
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(o => o.Promotion)
                  .WithMany(p => p.Orders)
                  .HasForeignKey(o => o.PromotionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId);

            entity.HasOne(oi => oi.ProductVariant)
                  .WithMany(v => v.OrderItems)
                  .HasForeignKey(oi => oi.ProductVariantId);
        });

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.TransactionId).IsUnique();

        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId);

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(r => r.Comment)
                  .HasMaxLength(2000);

            entity.HasOne(r => r.Variant)
                  .WithMany(p => p.Reviews)
                  .HasForeignKey(r => r.VariantId);

            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId);
        });

        modelBuilder.Entity<Wishlist>(entity =>
        {
            entity.HasOne(w => w.User)
                  .WithMany()
                  .HasForeignKey(w => w.UserId);

            entity.HasOne(w => w.ProductVariant)
                  .WithMany(p => p.Wishlist)
                  .HasForeignKey(w => w.ProductVariantId);
        });

        modelBuilder.Entity<Promotion>()
            .HasIndex(p => p.Code).IsUnique();

        modelBuilder.Entity<Contact>()
            .HasIndex(c => c.Email);

    modelBuilder.Entity<RefreshToken>()
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
