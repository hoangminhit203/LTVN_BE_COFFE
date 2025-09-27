using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class AppDbContext : IdentityDbContext<AspNetUsers>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    }
}
