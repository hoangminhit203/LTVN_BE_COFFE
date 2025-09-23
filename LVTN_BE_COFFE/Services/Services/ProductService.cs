using AutoMapper;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;

        // chỉ giữ 1 constructor duy nhất
        public ProductService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductResponse?> CreateProduct(ProductRequest request)
        {
            // kiểm tra trùng tên
            var exists = await FindByName(request.Name);
            if (exists)
            {
                throw new Exception("Sản phẩm đã tồn tại");
            }

            var product = _mapper.Map<Products>(request);
            //product.ProductId = Guid.NewGuid().ToString();
            //product.CreatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync(); // ✅ nhớ lưu DB

            return _mapper.Map<ProductResponse>(product);
        }

        public async Task<bool?> DeleteProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> FindByName(string name)
        {
            return await _context.Products
                .AnyAsync(p => p.Name.ToLower() == name.ToLower());
        }

        public async Task<List<ProductResponse?>> GetAllProducts()
        {
            var products = await _context.Products.ToListAsync();
            return _mapper.Map<List<ProductResponse?>>(products);
        }

        public async Task<ProductResponse?> GetProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            return _mapper.Map<ProductResponse?>(product);
        }

        public async Task<ProductResponse?> UpdateProduct(ProductRequest request, int productId)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return null;
            }

            _mapper.Map(request, product);
            product.UpdateAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return _mapper.Map<ProductResponse?>(product);
        }
    }
}
