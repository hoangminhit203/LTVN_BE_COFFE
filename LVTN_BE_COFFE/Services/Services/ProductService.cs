using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.Ultilities;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LVTN_BE_COFFE.Services.Services
{
    public class ProductService : Globals, IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context, IHttpContextAccessor accessor) : base(accessor)
        {
            _context = context;
        }

        // -------------------------
        // CREATE PRODUCT
        // -------------------------
        public async Task<ActionResult<ProductResponse>?> CreateProduct(ProductCreateVModel request)
        {
            if (await _context.Products.AnyAsync(x => x.Name == request.Name))
                throw new Exception("Tên sản phẩm đã tồn tại");

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                ImageUrl = request.ImageUrl,
                IsFeatured = request.IsFeatured,
                IsOnSale = request.IsOnSale,
                CreatedAt = DateTime.Now,
                //UpdatedAt = DateTime.Now
            };

            // GÁN CATEGORY CHO PRODUCT TRÁNH NULL VÀ TRÙNG LẶP
            if (request.CategoryId != null)
            {
                // Lấy category từ database
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category != null && !product.Categories.Any(c => c.Id == category.Id))
                {
                    product.Categories.Add(category);
                }
            }


            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // LOAD CATEGORY để trả về response
            await _context.Entry(product)
                .Collection(x => x.Categories)
                .LoadAsync();

            return MapToResponse(product);
        }


        // -------------------------
        // UPDATE PRODUCT
        // -------------------------
        public async Task<ActionResult<ProductResponse>?> UpdateProduct(ProductUpdateVModel request, int id)
        {
            var product = await _context.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
                return null;

            if (await _context.Products.AnyAsync(x => x.Name == request.Name && x.Id != id))
                throw new Exception("Tên sản phẩm đã tồn tại");

            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.Stock = request.Stock;
            product.ImageUrl = request.ImageUrl;
            product.IsFeatured = request.IsFeatured;
            product.IsOnSale = request.IsOnSale;
            product.UpdatedAt = DateTime.Now;

            // UPDATE CATEGORY
            if (request.CategoryId != null)
            {
                // Xóa category không còn trong request
                var categoriesToRemove = product.Categories
                    .Where(c => c.Id != request.CategoryId) // giữ category đang chọn
                    .ToList();

                foreach (var cat in categoriesToRemove)
                    product.Categories.Remove(cat);

                // Lấy category từ database
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category != null && !product.Categories.Any(c => c.Id == category.Id))
                {
                    product.Categories.Add(category);
                }
            }
            else
            {
                product.Categories.Clear();
            }

            await _context.SaveChangesAsync();
            return MapToResponse(product);
        }


        // -------------------------
        // DELETE PRODUCT
        // -------------------------
        public async Task<ActionResult<bool>> DeleteProduct(int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == productId);
            if (product == null)
                return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }


        // -------------------------
        // GET PRODUCT BY ID
        // -------------------------
        public async Task<ActionResult<ProductResponse>?> GetProduct(int productId)
        {
            var product = await _context.Products
                .Include(x => x.Categories)
                .FirstOrDefaultAsync(x => x.Id == productId);

            return product == null ? null : MapToResponse(product);
        }


        // -------------------------
        // FIND PRODUCT BY NAME
        // -------------------------
        public async Task<ActionResult<ProductResponse>?> FindByName(string name)
        {
            var product = await _context.Products
                .Include(x => x.Categories)
                .FirstOrDefaultAsync(x => x.Name == name);

            return product == null ? null : MapToResponse(product);
        }


        // -------------------------
        // GET ALL PRODUCTS (WITH CATEGORY LIST)
        // -------------------------
        public async Task<ActionResult<PaginationModel<ProductResponse>>> GetAllProducts(ProductFilterVModel filter)
        {
            var query = _context.Products
                .Include(x => x.Categories)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(x => x.Name.Contains(filter.Name));

            if (filter.IsFeatured.HasValue)
                query = query.Where(x => x.IsFeatured == filter.IsFeatured);

            if (filter.IsOnSale.HasValue)
                query = query.Where(x => x.IsOnSale == filter.IsOnSale);

            var totalRecords = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginationModel<ProductResponse>
            {
                Records = data.Select(MapToResponse).ToList(),
                TotalRecords = totalRecords
            };
        }


        // -------------------------
        // MAPPER
        // -------------------------
        private static ProductResponse MapToResponse(Product x)
        {
            return new ProductResponse
            {
                ProductId = x.Id,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                Stock = x.Stock,
                ImageUrl = x.ImageUrl,
                IsFeatured = x.IsFeatured,
                IsOnSale = x.IsOnSale,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,

                Category = x.Categories
                    .Select(c => new CategoryResponse
                    {
                        
                        Name = c.Name
                    })
                    .ToList()
            };
        }
    }
}
