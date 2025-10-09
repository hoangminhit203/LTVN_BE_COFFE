using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryResponse?> CreateCategory(CategoryCreateVModel request)
        {
            var existing = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == request.Name);

            if (existing != null)
                throw new Exception("Category already exists.");

            var category = new Category
            {
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<CategoryResponse?> UpdateCategory(CategoryUpdateVModel request)
        {
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                throw new Exception("Category not found.");

            category.Name = request.Name;
            category.UpdatedAt = DateTime.UtcNow;

            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            return new CategoryResponse
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }

        public async Task<bool> DeleteCategory(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category == null)
                throw new Exception("Category not found.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CategoryResponse>> GetAllCategories()
        {
            return await _context.Categories
                .AsNoTracking()
                .Select(c => new CategoryResponse
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<CategoryResponse?> GetCategoryById(int categoryId)
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryId == categoryId)
                .Select(c => new CategoryResponse
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<CategoryResponse?> FindByName(string name)
        {
            var c = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Name == name);

            return c == null ? null : new CategoryResponse
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            };
        }
    }
}
