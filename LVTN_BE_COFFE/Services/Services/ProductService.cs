using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IProductImageService _productImageService;

        public ProductService(AppDbContext context, IProductImageService productImageService)
        {
            _context = context;
            _productImageService = productImageService;
        }

        public async Task<ActionResult<ProductResponse>?> CreateProduct(ProductCreateVModel request)
        {
            if (await _context.Products.AnyAsync(x => x.Name == request.Name))
                throw new Exception("Tên sản phẩm đã tồn tại");

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            //Add Category
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
                throw new Exception("Không tồn tại Category");
            product.Categories.Add(category);
            

            //Add Flavor Notes
            if (request.FlavorNotes != null && request.FlavorNotes.Any())
            {
                // 1. Lấy ID của các FlavorNote dựa trên tên
                var flavorNoteIds = await _context.FlavorNotes
                    .Where(fn => request.FlavorNotes.Contains(fn.Name)) // Giả sử FlavorNote Entity có Name
                    .Select(fn => fn.Id)
                    .ToListAsync();

                // 2. Tạo Entity trung gian ProductFlavorNote bằng cách gán FlavorNoteId
                product.ProductFlavorNotes = flavorNoteIds
                    .Select(id => new ProductFlavorNote { FlavorNoteId = id })
                    .ToList();
            }

            //Add Brewing Methods
            if (request.BrewingMethods != null && request.BrewingMethods.Any())
            {
                // 1. Lấy ID của các BrewingMethod dựa trên tên
                var brewingMethodIds = await _context.BrewingMethods
                    .Where(bm => request.BrewingMethods.Contains(bm.Name)) // Giả sử BrewingMethod Entity có Name
                    .Select(bm => bm.Id)
                    .ToListAsync();

                // 2. Tạo Entity trung gian ProductBrewingMethod bằng cách gán BrewingMethodId
                product.ProductBrewingMethods = brewingMethodIds
                    .Select(id => new ProductBrewingMethod { BrewingMethodId = id })
                    .ToList();
            }

            //Add Variants
            if (request.Variants != null)
            {
                foreach (var v in request.Variants)
                {
                    var variant = new ProductVariant
                    {
                        Sku = v.Sku,
                        Price = v.Price,
                        Stock = v.Stock,
                        BeanType = v.BeanType,
                        RoastLevel = v.RoastLevel,
                        Origin = v.Origin,
                        Acidity = v.Acidity,
                        Weight = v.Weight,
                        Certifications = v.Certifications
                    };

                    product.Variants.Add(variant);
                }
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return MapToResponse(product);
        }

        public async Task<ActionResult<ProductResponse>?> UpdateProduct(ProductUpdateVModel request, int id)
        {
            var product = await _context.Products
                .Include(x => x.Categories)
                .Include(x => x.Variants).ThenInclude(v => v.Images)
                .Include(x => x.ProductFlavorNotes)
                .Include(x => x.ProductBrewingMethods)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
                return null;

            if (await _context.Products.AnyAsync(x => x.Name == request.Name && x.Id != id))
                throw new Exception("Tên sản phẩm đã tồn tại");

            product.Name = request.Name;
            product.Description = request.Description;
            product.UpdatedAt = DateTime.UtcNow;

            // 🔹 Update Categories
            product.Categories.Clear();
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category != null)
                product.Categories.Add(category);

            // 🔹 Update Flavor Notes
            product.ProductFlavorNotes.Clear(); // Luôn xóa các mối quan hệ cũ trước
            if (request.FlavorNotes != null && request.FlavorNotes.Any())
            {
                // 1. Tra cứu các ID của FlavorNote dựa trên tên (Name)
                var flavorNoteIds = await _context.FlavorNotes
                    .Where(fn => request.FlavorNotes.Contains(fn.Name)) // Giả sử FlavorNote có thuộc tính Name
                    .Select(fn => fn.Id)
                    .ToListAsync();

                // 2. Tạo Entity trung gian ProductFlavorNote bằng cách gán FlavorNoteId
                product.ProductFlavorNotes = flavorNoteIds
                    .Select(id => new ProductFlavorNote { FlavorNoteId = id })
                    .ToList();
            }

            // 🔹 Update Brewing Methods
            product.ProductBrewingMethods.Clear(); // Luôn xóa các mối quan hệ cũ trước
            if (request.BrewingMethods != null && request.BrewingMethods.Any())
            {
                // 1. Tra cứu các ID của BrewingMethod dựa trên tên (Name)
                var brewingMethodIds = await _context.BrewingMethods
                    .Where(bm => request.BrewingMethods.Contains(bm.Name)) // Giả sử BrewingMethod có thuộc tính Name
                    .Select(bm => bm.Id)
                    .ToListAsync();

                // 2. Tạo Entity trung gian ProductBrewingMethod bằng cách gán BrewingMethodId
                product.ProductBrewingMethods = brewingMethodIds
                    .Select(id => new ProductBrewingMethod { BrewingMethodId = id })
                    .ToList();
            }

            // 🔹 Update Variants
            product.Variants.Clear();

            if (request.Variants != null)
            {
                foreach (var v in request.Variants)
                {
                    var variant = new ProductVariant
                    {
                        Sku = v.Sku,
                        Price = v.Price,
                        Stock = v.Stock,
                        BeanType = v.BeanType,
                        RoastLevel = v.RoastLevel,
                        Origin = v.Origin,
                        Acidity = v.Acidity,
                        Weight = v.Weight,
                        Certifications = v.Certifications
                    };

                    product.Variants.Add(variant);
                }
            }

            await _context.SaveChangesAsync();

            return MapToResponse(product);
        }

        public async Task<ActionResult<bool>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ActionResult<ProductResponse>?> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(x => x.Categories)
                .Include(x => x.Variants).ThenInclude(v => v.Images)
                .Include(x => x.ProductFlavorNotes).ThenInclude(fn => fn.FlavorNote)
                .Include(x => x.ProductBrewingMethods).ThenInclude(bm => bm.BrewingMethod)
                .FirstOrDefaultAsync(x => x.Id == id);

            return product == null ? null : MapToResponse(product);
        }

        public async Task<ActionResult<PaginationModel<ProductResponse>>> GetAllProducts(ProductFilterVModel filter)
        {
            var query = _context.Products
                .Include(x => x.Categories)
                .Include(x => x.Variants).ThenInclude(v => v.Images)
                .Include(x => x.ProductFlavorNotes)
                .Include(x => x.ProductBrewingMethods)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(x => x.Name.Contains(filter.Name));

            if (filter.CategoryId.HasValue)
                query = query.Where(x => x.Categories.Any(c => c.Id == filter.CategoryId));

            if (!string.IsNullOrEmpty(filter.RoastLevel))
                query = query.Where(x => x.Variants.Any(v => v.RoastLevel == filter.RoastLevel));

            if (!string.IsNullOrEmpty(filter.BeanType))
                query = query.Where(x => x.Variants.Any(v => v.BeanType == filter.BeanType));

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Id)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginationModel<ProductResponse>
            {
                TotalRecords = total,
                Records = data.Select(MapToResponse).ToList()
            };
        }

        private static ProductResponse MapToResponse(Product p)
        {
            return new ProductResponse
            {
                ProductId = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,

                FlavorNotes = p.ProductFlavorNotes
                    .Select(x => x.FlavorNote.Name)
                    .ToList(),

                BrewingMethods = p.ProductBrewingMethods
                    .Select(x => x.BrewingMethod.Name)
                    .ToList(),

                Category = p.Categories
                    .Select(c => new CategoryResponse
                    {
                        CategoryId=c.Id,
                        Name = c.Name
                    })
                    .ToList(),

                Variants = p.Variants.Select(v => new ProductVariantResponse
                {
                    VariantId = v.Id,
                    Sku = v.Sku,
                    Price = v.Price,
                    Stock = v.Stock,
                    BeanType = v.BeanType,
                    RoastLevel = v.RoastLevel,
                    Origin = v.Origin,
                    Acidity = v.Acidity,
                    Weight = v.Weight,
                    Certifications = v.Certifications,

                    Images = v.Images
                        .OrderBy(img => img.SortOrder)
                        .Select(img => new ProductImageResponse
                        {
                            //Id = img.Id,
                            ImageUrl = img.ImageUrl,

                            ProductId = img.ProductId,
                            ProductVariantId =img.ProductVariantId,
                            IsMain = img.IsMain,
                            SortOrder = img.SortOrder
                        }).ToList()

                }).ToList()
            };
        }

    }
}
