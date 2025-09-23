using AutoMapper;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using LVTN_BE_COFFE.Mapper;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        public ProductService(IMapper mapper)
        {
            _mapper = mapper;
        }
        public ProductService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<ProductResponse?> CreateProduct(ProductRequest request)
        {
            var exists= FindByName(request.Name);
            if (exists==null)
            {
                throw new Exception("Khong tìm thấy sản phẩm");
            }
            var product = _mapper.Map<Products>(request);
            //product.ProductId = Guid.NewGuid().ToString();
            //product.CreatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            return _mapper.Map<ProductResponse>(request);
        }

        public async Task<bool?> DeleteProduct(int productId)
        {
            // Tìm sản phẩm trong DB
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return false; // Không tìm thấy
            }

            // Xóa sản phẩm
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> FindByName(string name)
        {
            // tìm sản phẩm đảm bảo không phân biệt hoa thường
            return await _context.Products
                .AnyAsync(p => p.Name.ToLower() == name.ToLower());
        }

        public async Task<List<ProductResponse?>> GetAllProducts()
        {
            // lấy all sản phẩm
            var products = await _context.Products.ToListAsync();
            return _mapper.Map<List<ProductResponse?>>(products);
        }

        public async Task<ProductResponse?> GetProduct(int productId)
        {
            //Lấy sản phẩm theo id
            var product = await _context.Products.FindAsync(productId);
            return _mapper.Map<ProductResponse?>(product);
        }

        public async Task<ProductResponse?> UpdateProduct(ProductRequest request, int productId)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
            {
                return null; // Không tìm thấy sản phẩm
            }

            // 2. Cập nhật dữ liệu từ request vào entity
            _mapper.Map(request, product);
            // 3. Cập nhật thời gian sửa đổi (nếu có field UpdatedAt)
            product.UpdateAt = DateTime.UtcNow;

            // 4. Lưu thay đổi vào DB
            await _context.SaveChangesAsync();

            // 5. Trả về response
            return _mapper.Map<ProductResponse?>(product);
        }
    }
}
