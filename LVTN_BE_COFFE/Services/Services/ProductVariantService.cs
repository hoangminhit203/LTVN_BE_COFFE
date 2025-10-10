using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Domain.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly AppDbContext _context;

        public ProductVariantService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductVariantResponse>> GetAllVariants()
        {
            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .Include(v => v.Size)
                .Include(v => v.ProductVariantToppings)
                    .ThenInclude(pvt => pvt.Topping)
                .ToListAsync();

            return variants.Select(v => new ProductVariantResponse
            {
                ProductVariantId = v.ProductVariantId,
                ProductId = v.ProductId,
                SizeId = v.SizeId,
                Price = v.Price,
                ProductName = v.Product.Name,
                SizeName = v.Size.Name,
                CreatedAt = v.CreatedAt,
                ToppingNames = v.ProductVariantToppings.Select(t => t.Topping.Name).ToList()
            });
        }

        public async Task<ProductVariantResponse?> GetVariantById(int id)
        {
            var v = await _context.ProductVariants
                .Include(x => x.Product)
                .Include(x => x.Size)
                .Include(x => x.ProductVariantToppings)
                    .ThenInclude(t => t.Topping)
                .FirstOrDefaultAsync(x => x.ProductVariantId == id);

            if (v == null) return null;

            return new ProductVariantResponse
            {
                ProductVariantId = v.ProductVariantId,
                ProductId = v.ProductId,
                SizeId = v.SizeId,
                Price = v.Price,
                ProductName = v.Product.Name,
                SizeName = v.Size.Name,
                CreatedAt = v.CreatedAt,
                ToppingNames = v.ProductVariantToppings.Select(t => t.Topping.Name).ToList()
            };
        }

        public async Task<ProductVariantResponse> CreateVariant(ProductVariantCreateVModel model)
        {
            var variant = new ProductVariant
            {
                ProductId = model.ProductId,
                SizeId = model.SizeId,
                Price = model.Price
            };

            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();

            // Thêm topping nếu có
            if (model.ToppingIds != null && model.ToppingIds.Any())
            {
                foreach (var toppingId in model.ToppingIds)
                {
                    _context.ProductVariantToppings.Add(new ProductVariantTopping
                    {
                        ProductVariantId = variant.ProductVariantId,
                        ToppingId = toppingId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return await GetVariantById(variant.ProductVariantId) ?? throw new Exception("Lỗi tạo biến thể sản phẩm.");
        }

        public async Task<ProductVariantResponse?> UpdateVariant(ProductVariantUpdateVModel model)
        {
            var variant = await _context.ProductVariants
                .Include(v => v.ProductVariantToppings)
                .FirstOrDefaultAsync(v => v.ProductVariantId == model.ProductVariantId);

            if (variant == null) return null;

            variant.Price = model.Price;
            variant.SizeId = model.SizeId;

            // Cập nhật topping
            _context.ProductVariantToppings.RemoveRange(variant.ProductVariantToppings);
            if (model.ToppingIds != null && model.ToppingIds.Any())
            {
                foreach (var toppingId in model.ToppingIds)
                {
                    _context.ProductVariantToppings.Add(new ProductVariantTopping
                    {
                        ProductVariantId = model.ProductVariantId,
                        ToppingId = toppingId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return await GetVariantById(model.ProductVariantId);
        }

        public async Task<bool> DeleteVariant(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant == null) return false;

            _context.ProductVariants.Remove(variant);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
