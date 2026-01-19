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

        public async Task<ActionResult<ResponseResult>?> CreateProduct(ProductCreateVModel request)
        {
            try
            {
                if (await _context.Products.AnyAsync(x => x.Name == request.Name))
                    return new ErrorResponseResult("Tên sản phẩm đã tồn tại");

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
                    return new ErrorResponseResult("Không tồn tại Category");
                product.Categories.Add(category);


                //Add Flavor Notes
                if (request.FlavorNoteIds != null && request.FlavorNoteIds.Any())
                {
                    // Kiểm tra các FlavorNote có tồn tại không
                    var existingFlavorNoteIds = await _context.FlavorNotes
                        .Where(fn => request.FlavorNoteIds.Contains(fn.Id))
                        .Select(fn => fn.Id)
                        .ToListAsync();

                    if (existingFlavorNoteIds.Count != request.FlavorNoteIds.Count)
                        return new ErrorResponseResult("Một số FlavorNote không tồn tại");

                    // Tạo Entity trung gian ProductFlavorNote
                    product.ProductFlavorNotes = existingFlavorNoteIds
                        .Select(id => new ProductFlavorNote { FlavorNoteId = id })
                        .ToList();
                }

                //Add Brewing Methods
                if (request.BrewingMethodIds != null && request.BrewingMethodIds.Any())
                {
                    // Kiểm tra các BrewingMethod có tồn tại không
                    var existingBrewingMethodIds = await _context.BrewingMethods
                        .Where(bm => request.BrewingMethodIds.Contains(bm.Id))
                        .Select(bm => bm.Id)
                        .ToListAsync();

                    if (existingBrewingMethodIds.Count != request.BrewingMethodIds.Count)
                        return new ErrorResponseResult("Một số BrewingMethod không tồn tại");

                    // Tạo Entity trung gian ProductBrewingMethod
                    product.ProductBrewingMethods = existingBrewingMethodIds
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

                // Reload product với đầy đủ thông tin để map
                product = await _context.Products
                    .Include(x => x.Categories)
                    .Include(x => x.Variants).ThenInclude(v => v.Images)
                    .Include(x => x.ProductFlavorNotes).ThenInclude(fn => fn.FlavorNote)
                    .Include(x => x.ProductBrewingMethods).ThenInclude(bm => bm.BrewingMethod)
                    .FirstOrDefaultAsync(x => x.Id == product.Id);

                return new SuccessResponseResult(MapToResponse(product), "Tạo sản phẩm thành công");
            }
            catch (Exception ex)
            {
                return new ErrorResponseResult($"Lỗi khi tạo sản phẩm: {ex.Message}");
            }
        }

        public async Task<ActionResult<ResponseResult>?> UpdateProduct(ProductUpdateVModel request, int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(x => x.Categories)
                    .Include(x => x.Variants)
                        .ThenInclude(v => v.Images)
                    .Include(x => x.ProductFlavorNotes)
                    .Include(x => x.ProductBrewingMethods)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (product == null)
                    return new ErrorResponseResult("Không tìm thấy sản phẩm");

                // Check duplicate name
                if (await _context.Products.AnyAsync(x => x.Name == request.Name && x.Id != id))
                    return new ErrorResponseResult("Tên sản phẩm đã tồn tại");

                // ===== UPDATE BASIC INFO =====
                product.Name = request.Name;
                product.Description = request.Description;
                product.UpdatedAt = DateTime.UtcNow;

                // ===== UPDATE CATEGORY (1 category) =====
                product.Categories.Clear();
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category != null)
                    product.Categories.Add(category);

                // ===== UPDATE FLAVOR NOTES =====
                product.ProductFlavorNotes.Clear();
                if (request.FlavorNoteIds?.Any() == true)
                {
                    var flavorNoteIds = await _context.FlavorNotes
                        .Where(x => request.FlavorNoteIds.Contains(x.Id))
                        .Select(x => x.Id)
                        .ToListAsync();

                    if (flavorNoteIds.Count != request.FlavorNoteIds.Count)
                        return new ErrorResponseResult("Một số FlavorNote không tồn tại");

                    product.ProductFlavorNotes = flavorNoteIds
                        .Select(id => new ProductFlavorNote { FlavorNoteId = id })
                        .ToList();
                }

                // ===== UPDATE BREWING METHODS =====
                product.ProductBrewingMethods.Clear();
                if (request.BrewingMethodIds?.Any() == true)
                {
                    var brewingMethodIds = await _context.BrewingMethods
                        .Where(x => request.BrewingMethodIds.Contains(x.Id))
                        .Select(x => x.Id)
                        .ToListAsync();

                    if (brewingMethodIds.Count != request.BrewingMethodIds.Count)
                        return new ErrorResponseResult("Một số BrewingMethod không tồn tại");

                    product.ProductBrewingMethods = brewingMethodIds
                        .Select(id => new ProductBrewingMethod { BrewingMethodId = id })
                        .ToList();
                }

                // UPDATE VARIANTS (CORE)
                var existingVariants = product.Variants.ToList();

                var requestVariantIds = request.Variants?
                    .Where(v => v.Id.HasValue)
                    .Select(v => v.Id.Value)
                    .ToList() ?? new List<int>();

                // REMOVE VARIANTS NOT IN REQUEST
                var variantsToRemove = existingVariants
                    .Where(v => !requestVariantIds.Contains(v.Id))
                    .ToList();

                _context.ProductVariant.RemoveRange(variantsToRemove);

                // UPDATE / ADD VARIANTS
                foreach (var v in request.Variants ?? Enumerable.Empty<ProductVariantUpdateVModel>())
                {
                    if (v.Id.HasValue)
                    {
                        // UPDATE EXISTING
                        var variant = existingVariants.FirstOrDefault(x => x.Id == v.Id.Value);
                        if (variant == null)
                            return new ErrorResponseResult($"Variant id {v.Id} không tồn tại");

                        variant.Sku = v.Sku;
                        variant.Price = v.Price;
                        variant.Stock = v.Stock;
                        variant.BeanType = v.BeanType;
                        variant.RoastLevel = v.RoastLevel;
                        variant.Origin = v.Origin;
                        variant.Acidity = v.Acidity;
                        variant.Weight = v.Weight;
                        variant.Certifications = v.Certifications;
                    }
                    else
                    {
                        // ADD NEW VARIANT
                        product.Variants.Add(new ProductVariant
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
                        });
                    }
                }

                await _context.SaveChangesAsync();

                // ===== RELOAD FOR RESPONSE =====
                product = await _context.Products
                    .Include(x => x.Categories)
                    .Include(x => x.Variants)
                        .ThenInclude(v => v.Images)
                    .Include(x => x.ProductFlavorNotes)
                        .ThenInclude(x => x.FlavorNote)
                    .Include(x => x.ProductBrewingMethods)
                        .ThenInclude(x => x.BrewingMethod)
                    .FirstOrDefaultAsync(x => x.Id == id);

                return new SuccessResponseResult(
                    MapToResponse(product),
                    "Cập nhật sản phẩm thành công"
                );
            }
            catch (Exception ex)
            {
                return new ErrorResponseResult($"Lỗi khi cập nhật sản phẩm: {ex.Message}");
            }
        }


        public async Task<ActionResult<ResponseResult>> DeleteProduct(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                    return new ErrorResponseResult("Không tìm thấy sản phẩm");

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return new SuccessResponseResult(true, "Xóa sản phẩm thành công");
            }
            catch (Exception ex)
            {
                return new ErrorResponseResult($"Lỗi khi xóa sản phẩm: {ex.Message}");
            }
        }

        public async Task<ActionResult<ResponseResult>?> GetProduct(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(x => x.Categories)
                    .Include(x => x.Variants).ThenInclude(v => v.Images)
                    .Include(x => x.ProductFlavorNotes).ThenInclude(fn => fn.FlavorNote)
                    .Include(x => x.ProductBrewingMethods).ThenInclude(bm => bm.BrewingMethod)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (product == null)
                    return new ErrorResponseResult("Không tìm thấy sản phẩm");

                return new SuccessResponseResult(MapToResponse(product), "Lấy thông tin sản phẩm thành công");
            }
            catch (Exception ex)
            {
                return new ErrorResponseResult($"Lỗi khi lấy thông tin sản phẩm: {ex.Message}");
            }
        }

        public async Task<ActionResult<ResponseResult>> GetAllProducts(ProductFilterVModel filter)
        {
            try
            {
                var query = _context.Products
                    .Include(x => x.Categories)
                    .Include(x => x.Variants).ThenInclude(v => v.Images)
                    .Include(x => x.ProductFlavorNotes).ThenInclude(fn => fn.FlavorNote)
                    .Include(x => x.ProductBrewingMethods).ThenInclude(bm => bm.BrewingMethod)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(filter.Name))
                    query = query.Where(x => x.Name.Contains(filter.Name));

                if (!string.IsNullOrEmpty(filter.Origin))
                    query = query.Where(x => x.Variants.Any(v => v.Origin.Contains(filter.Origin)));
                if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue)
                {
                    query = query.Where(x => x.Variants.Any(v => v.Price >= filter.MinPrice.Value && v.Price <= filter.MaxPrice.Value));
                }
                else if (filter.MinPrice.HasValue)
                {
                    query = query.Where(x => x.Variants.Any(v => v.Price >= filter.MinPrice.Value));
                }
                else if (filter.MaxPrice.HasValue)
                {
                    query = query.Where(x => x.Variants.Any(v => v.Price <= filter.MaxPrice.Value));
                }

                    var total = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)total / filter.PageSize);

                var data = await query
                    .OrderByDescending(x => x.Id)
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var paginationResponse = new
                {
                    TotalRecords = total,
                    TotalPages = totalPages,
                    CurrentPage = filter.PageNumber,
                    PageSize = filter.PageSize,
                    Records = data.Select(MapToResponse).ToList()
                };

                return new SuccessResponseResult(paginationResponse, "Lấy danh sách sản phẩm thành công");
            }
            catch (Exception ex)
            {
                return new ErrorResponseResult($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}");
            }
        }

        public async Task<ActionResult<ResponseResult>> GetProductByCategory(int categoryId)
        {
            try
            {
                var products = await _context.Products
                    .Include(x => x.Categories)
                    .Include(x => x.Variants).ThenInclude(v => v.Images)
                    .Include(x => x.ProductFlavorNotes).ThenInclude(fn => fn.FlavorNote)
                    .Include(x => x.ProductBrewingMethods).ThenInclude(bm => bm.BrewingMethod)
                    .Where(p => p.Categories.Any(c => c.Id == categoryId))
                    .ToListAsync();
                var productResponses = products.Select(MapToResponse).ToList();
                return new SuccessResponseResult(productResponses, "Lấy sản phẩm theo danh mục thành công");
            }
            catch (Exception ex)
            {
                return new ErrorResponseResult($"Lỗi khi lấy sản phẩm theo danh mục: {ex.Message}");
            }
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
                    .Select(x => new FlavorNoteResponse
                    {
                        FlavorNoteId = x.FlavorNoteId,
                        Name = x.FlavorNote.Name,
                        IsActive = x.FlavorNote.IsActive,
                        CreatedDate = x.FlavorNote.CreatedDate,
                        UpdatedDate = x.FlavorNote.UpdatedDate
                    })
                    .ToList(),

                BrewingMethods = p.ProductBrewingMethods
                    .Select(x => new BrewingMethodsResponse
                    {
                        BrewingMethodId = x.BrewingMethodId,
                        Name = x.BrewingMethod.Name,
                        Description = x.BrewingMethod.Description,
                        IsActive = x.BrewingMethod.IsActive,
                        CreatedDate = x.BrewingMethod.CreatedDate,
                        UpdatedDate = x.BrewingMethod.UpdatedDate
                    })
                    .ToList(),

                Category = p.Categories
                    .Select(c => new CategoryResponse
                    {
                        CategoryId = c.Id,
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
                            ProductVariantId = img.ProductVariantId,
                            IsMain = img.IsMain,
                            SortOrder = img.SortOrder
                        }).ToList()

                }).ToList()
            };
        }

    }
}