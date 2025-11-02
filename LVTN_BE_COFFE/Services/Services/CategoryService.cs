using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Domain.Services
{
    public class CategoryService : ControllerBase, ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Create
        public async Task<ActionResult<CategoryResponse>?> CreateCategory(CategoryCreateVModel request)
        {
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return MapToResponse(category);
        }

        // ✅ Update
        public async Task<ActionResult<CategoryResponse>?> UpdateCategory(CategoryUpdateVModel request, int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Category not found");

            category.Name = request.Name;
            category.Description = request.Description;

            await _context.SaveChangesAsync();

            return MapToResponse(category);
        }

        // ✅ Delete
        public async Task<ActionResult<bool>> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound("Category not found");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        // ✅ Get by ID
        public async Task<ActionResult<CategoryResponse>?> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound("Category not found");

            return MapToResponse(category);
        }

        // ✅ Get all with pagination + filter
        public async Task<ActionResult<PaginationModel<CategoryResponse>>> GetAllCategories(CategoryFilterVModel filter)
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(x => x.Name.Contains(filter.Name));

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                .ToListAsync();

           return new PaginationModel<CategoryResponse>
            {
                TotalRecords = totalCount,
                Records = items.Select(MapToResponse).ToList()
            };
        }

        // ✅ Map entity → response
        private static CategoryResponse MapToResponse(Category x)
        {
            return new CategoryResponse
            {
                CategoryId = x.Id,
                Name = x.Name,
                Description = x.Description,
                Products = x.ProductCategories?.Select(pc => new ProductResponse
                {
                    ProductId = pc.Product.Id,
                    Name = pc.Product.Name,
                    Description = pc.Product.Description,
                    Price = pc.Product.Price,
                    ImageUrl = pc.Product.ImageUrl,
                    CategoryId = pc.CategoryId
                }).ToList()
            };
        }
    }
}
