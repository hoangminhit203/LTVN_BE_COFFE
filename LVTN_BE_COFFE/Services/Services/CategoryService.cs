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

        // Create
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

            return new SuccessResponseResult(MapToResponse(category), "Tạo thành công");
        }

        // Update
        public async Task<ActionResult<ResponseResult>?> UpdateCategory(CategoryUpdateVModel request, int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null || category.IsActive != true)
                return new ErrorResponseResult("Không tìm thấy");

            category.Name = request.Name;
            category.Description = request.Description;
            category.IsActive = true;
            category.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new SuccessResponseResult(MapToResponse(category), "Cập nhật thành công");
        }

        // Soft Delete (Set IsActive to false)
        public async Task<ActionResult<ResponseResult>> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return new ErrorResponseResult("Không tìm thấy");

            // Soft delete by setting IsActive to false
            category.IsActive = false;
            category.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new SuccessResponseResult(true, "Xóa thành công");
        }

        // Get by ID (only active)
        public async Task<ActionResult<ResponseResult>?> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Products.Where(p => p.IsActive == true)) // Only active products
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive == true); // Only active category

            if (category == null)
                return new ErrorResponseResult("Không tìm thấy");

            return new SuccessResponseResult(MapToResponse(category), "Lấy dữ liệu thành công");
        }

        // Get all with pagination + filter (only active)
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

            return new SuccessResponseResult(paginationResponse, "Lấy dữ liệu thành công");
        }
        public async Task<ActionResult<ResponseResult>> GetProductsByCategoryId(int categoryId, int pageNumber = 1, int pageSize = 10)
        {
            var category = await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                return new ErrorResponseResult("Không tìm thấy");

            var productIds = category.Products
                .Select(p => p.Id)
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
                }, "Không tìm thấy sản phẩm");
            }

            var query = _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Images)
                .Where(p => productIds.Contains(p.Id));

            // 4. Phân trang
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 5. Lấy dữ liệu và Map
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new {
                     Product = p,
                     PrimaryVariant = p.Variants
                         .OrderByDescending(v => v.Stock > 0)
                         .ThenBy(v => v.Price)
                         .FirstOrDefault()
                 })
                .Select(x => new // Map ra Object cuối cùng
                {
                    ProductId = x.Product.Id,
                    Name = x.Product.Name,
                    Description = x.Product.Description,
                    Price = x.PrimaryVariant != null ? x.PrimaryVariant.Price : 0,
                    Sku = x.PrimaryVariant != null ? x.PrimaryVariant.Sku : null,

                    // Lấy ảnh từ Variant chính, nếu không có thì lấy ảnh của Product
                    ImageUrl = x.PrimaryVariant != null && x.PrimaryVariant.Images.Any()
                        ? x.PrimaryVariant.Images.FirstOrDefault().ImageUrl
                        : (x.Product.Images.Any() ? x.Product.Images.FirstOrDefault().ImageUrl : null),

                    IsFeatured = x.Product.IsFeatured,
                    IsOnSale = x.Product.IsOnSale
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

            return new SuccessResponseResult(paginationResponse, "Lấy dữ liệu thành công");
        }

        // Map entity → response
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