using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : IdentityDbContext<AspNetUsers>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
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