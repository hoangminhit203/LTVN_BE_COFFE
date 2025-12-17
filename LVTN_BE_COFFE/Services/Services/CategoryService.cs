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
        public async Task<ActionResult<ResponseResult>?> CreateCategory(CategoryCreateVModel request)
        {
            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = true, // Set IsActive when creating
                CreatedDate = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new SuccessResponseResult(MapToResponse(category), "Category created successfully");
        }

        // ✅ Update
        public async Task<ActionResult<ResponseResult>?> UpdateCategory(CategoryUpdateVModel request, int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null || category.IsActive != true)
                return new ErrorResponseResult("Category not found");

            category.Name = request.Name;
            category.Description = request.Description;
            category.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new SuccessResponseResult(MapToResponse(category), "Category updated successfully");
        }

        // ✅ Soft Delete (Set IsActive to false)
        public async Task<ActionResult<ResponseResult>> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return new ErrorResponseResult("Category not found");

            // Soft delete by setting IsActive to false
            category.IsActive = false;
            category.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new SuccessResponseResult(true, "Category deleted successfully");
        }

        // ✅ Get by ID (only active)
        public async Task<ActionResult<ResponseResult>?> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products.Where(p => p.IsActive == true)) // Only active products
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive == true); // Only active category

            if (category == null)
                return new ErrorResponseResult("Category not found");

            return new SuccessResponseResult(MapToResponse(category), "Category retrieved successfully");
        }

        // ✅ Get all with pagination + filter (only active)
        public async Task<ActionResult<ResponseResult>> GetAllCategories(CategoryFilterVModel filter)
        {
            var query = _context.Categories.Where(c => c.IsActive == true); // Only active categories

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(x => x.Name.Contains(filter.Name));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            var items = await query
                .Include(c => c.Products.Where(p => p.IsActive == true)) // Only active products
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var paginationResponse = new
            {
                TotalRecords = totalCount,
                TotalPages = totalPages,
                CurrentPage = filter.PageNumber,
                PageSize = filter.PageSize,
                Records = items.Select(MapToResponse).ToList()
            };

            return new SuccessResponseResult(paginationResponse, "Categories retrieved successfully");
        }

        // ✅ Map entity → response
        private static CategoryResponse MapToResponse(Category x)
        {
            return new CategoryResponse
            {
                CategoryId = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive, // Include IsActive in response
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate
                //Products = x.Products?.Select(p => new ProductResponse
                //{
                //    ProductId = p.Id,
                //    Name = p.Name,
                //    Description = p.Description,
                //    Price = p.Price,
                //    ImageUrl = p.ImageUrl,
                //    CategoryId = x.Id // gán CategoryId trực tiếp từ Category
                //}).ToList()
            };
        }
    }
}