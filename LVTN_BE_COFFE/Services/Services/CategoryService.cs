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
                Description = request.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new SuccessResponseResult(MapToResponse(category), "Category created successfully");
        }

        // ✅ Update
        public async Task<ActionResult<ResponseResult>?> UpdateCategory(CategoryUpdateVModel request, int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return new ErrorResponseResult("Category not found");

            category.Name = request.Name;
            category.Description = request.Description;

            await _context.SaveChangesAsync();

            return new SuccessResponseResult(MapToResponse(category), "Category updated successfully");
        }

        // ✅ Delete
        public async Task<ActionResult<ResponseResult>> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return new ErrorResponseResult("Category not found");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return new SuccessResponseResult(true, "Category deleted successfully");
        }

        // ✅ Get by ID
        public async Task<ActionResult<ResponseResult>?> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products) // thay ProductCategories -> Products
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return new ErrorResponseResult("Category not found");

            return new SuccessResponseResult(MapToResponse(category), "Category retrieved successfully");
        }


        // ✅ Get all with pagination + filter
        public async Task<ActionResult<ResponseResult>> GetAllCategories(CategoryFilterVModel filter)
        {
            var query = _context.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(x => x.Name.Contains(filter.Name));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            var items = await query
                .Include(c => c.Products) // thay ProductCategories -> Products
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
