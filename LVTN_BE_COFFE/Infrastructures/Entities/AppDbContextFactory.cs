using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    // Factory này chỉ dùng lúc migration, không ảnh hưởng lúc chạy app
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // 👉 Thay chuỗi kết nối này bằng chuỗi trong appsettings.json
            optionsBuilder.UseSqlServer("Server=.;Database=CoffeeDb;Trusted_Connection=True;TrustServerCertificate=True;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
