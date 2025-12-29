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
                IsActive = request.IsActive, // Set IsActive when creating
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
            category.IsActive = true;
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
            // Filter only active categories
            var query = _context.Categories.Where(c => c.IsActive == true);

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
        public async Task<ActionResult<ResponseResult>> GetProductsByCategoryId(int categoryId, int pageNumber = 1, int pageSize = 10)
        {
            // 1. Kiểm tra Category có tồn tại không (BỎ CHECK IsActive nếu cần)
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == categoryId); // ⭐ BỎ && c.IsActive == true

            if (category == null)
                return new ErrorResponseResult("Category not found");

            // 2. Lấy danh sách productIds từ category (BỎ CHECK IsActive)
            var productIds = category.Products
                .Select(p => p.Id) // ⭐ BỎ .Where(p => p.IsActive == true)
                .ToList();

            if (!productIds.Any())
            {
                return new SuccessResponseResult(new
                {
                    TotalRecords = 0,
                    TotalPages = 0,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    Records = new List<object>()
                }, "No products found in this category");
            }

            // 3. Query sản phẩm dựa trên productIds (BỎ CHECK IsActive)
            var query = _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .Where(p => productIds.Contains(p.Id)); // ⭐ BỎ && p.IsActive == true

            // 4. Phân trang
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 5. Lấy dữ liệu và Map
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new
                {
                    ProductId = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    // Lấy giá thấp nhất từ các variant có stock > 0
                    Price = p.Variants.Any(v => v.Stock > 0)
                        ? p.Variants.Where(v => v.Stock > 0).Min(v => v.Price)
                        : (p.Variants.Any() ? p.Variants.Min(v => v.Price) : 0),
                    // ⭐ Lấy SKU của variant có giá thấp nhất (cùng logic với Price)
                    Sku = p.Variants.Any(v => v.Stock > 0)
                        ? p.Variants.Where(v => v.Stock > 0).OrderBy(v => v.Price).First().Sku
                        : (p.Variants.Any() ? p.Variants.OrderBy(v => v.Price).First().Sku : null),
                    ImageUrl = p.Images.Any()
                        ? p.Images.OrderBy(i => i.SortOrder).First().ImageUrl
                        : null,
                    IsFeatured = p.IsFeatured,
                    IsOnSale = p.IsOnSale
                })
                .ToListAsync();

            // 6. Trả về kết quả
            var paginationResponse = new
            {
                TotalRecords = totalCount,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Records = items
            };

            return new SuccessResponseResult(paginationResponse, "Products retrieved successfully");
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