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
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductAttribute> ProductAttributes { get; set; }
    public DbSet<FlavorNote> FlavorNotes { get; set; }
    public DbSet<ProductFlavorNote> ProductFlavorNotes { get; set; }
    public DbSet<BrewingMethod> BrewingMethods { get; set; }
    public DbSet<ProductBrewingMethod> ProductBrewingMethods { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Required for ASP.NET Identity

        // Configure AspNetUsers
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

        // Configure RefreshTokens
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        // Configure Products (from your provided Product class)
        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name)
            .IsUnique(false); // FULLTEXT index for search
        modelBuilder.Entity<Product>()
            .HasOne(p => p.ProductAttribute)
            .WithOne(pa => pa.Product)
            .HasForeignKey<ProductAttribute>(pa => pa.ProductId);

        // Configure ProductAttributes
        modelBuilder.Entity<ProductAttribute>()
            .HasIndex(pa => new { pa.ProductId, pa.RoastLevel, pa.BeanType });

        // Configure FlavorNotes and ProductFlavorNotes (many-to-many)
        modelBuilder.Entity<ProductFlavorNote>()
            .HasKey(pfn => new { pfn.ProductId, pfn.FlavorNoteId });
        modelBuilder.Entity<ProductFlavorNote>()
            .HasOne(pfn => pfn.Product)
            .WithMany(p => p.ProductFlavorNotes)
            .HasForeignKey(pfn => pfn.ProductId);
        modelBuilder.Entity<ProductFlavorNote>()
            .HasOne(pfn => pfn.FlavorNote)
            .WithMany(fn => fn.ProductFlavorNotes)
            .HasForeignKey(pfn => pfn.FlavorNoteId);

        // Configure BrewingMethods and ProductBrewingMethods (many-to-many)
        modelBuilder.Entity<ProductBrewingMethod>()
            .HasKey(pbm => new { pbm.ProductId, pbm.BrewingMethodId });
        modelBuilder.Entity<ProductBrewingMethod>()
            .HasOne(pbm => pbm.Product)
            .WithMany(p => p.ProductBrewingMethods)
            .HasForeignKey(pbm => pbm.ProductId);
        modelBuilder.Entity<ProductBrewingMethod>()
            .HasOne(pbm => pbm.BrewingMethod)
            .WithMany(bm => bm.ProductBrewingMethods)
            .HasForeignKey(pbm => pbm.BrewingMethodId);

        // Configure Categories and ProductCategories (many-to-many)
        modelBuilder.Entity<ProductCategory>()
            .HasKey(pc => new { pc.ProductId, pc.CategoryId });
        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId);
        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId);
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure Carts
        modelBuilder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure CartItems
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId);
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.Product)
            .WithMany(p => p.CartItems)
            .HasForeignKey(ci => ci.ProductId);

        // Configure Orders
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.SetNull);
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Promotion)
            .WithMany(p => p.Orders)
            .HasForeignKey(o => o.VoucherCode);

        // Configure OrderItems
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId);

        // Configure Payments
        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.TransactionId)
            .IsUnique();
        modelBuilder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithMany(o => o.Payments)
            .HasForeignKey(p => p.OrderId);

        // Configure Reviews
        modelBuilder.Entity<Review>()
            .HasOne(r => r.Product)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProductId);
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId);

        // Configure Wishlists
        modelBuilder.Entity<Wishlist>()
            .HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId);
        modelBuilder.Entity<Wishlist>()
            .HasOne(w => w.Product)
            .WithMany(p => p.Wishlists)
            .HasForeignKey(w => w.ProductId);

        // Configure Promotions
        modelBuilder.Entity<Promotion>()
            .HasIndex(p => p.Code)
            .IsUnique();

        // Configure Contacts
        modelBuilder.Entity<Contact>()
            .HasIndex(c => c.Email);

        // RefreshToken - User
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
