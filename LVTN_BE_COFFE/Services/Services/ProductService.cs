
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LVTN_BE_COFFE.Services.Services
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;

        public ProductService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ProductResponse?> CreateProduct(ProductCreateVModel request)
        {
            var branch = await _context.Branches.FindAsync(request.BranchId);
            if (branch == null)
            {
                return null; // Hoặc ném ra ngoại lệ nếu cần
            }
            var findProduct = await _context.Products.FirstOrDefaultAsync(p => p.Name == request.Name);
            if (findProduct != null)
            {
                return null; // Hoặc ném ra ngoại lệ nếu cần
            }
            var category = await _context.Categories.FindAsync(request.CategoryId);
            if (category == null)
            {
                return null; // Hoặc ném ra ngoại lệ nếu cần
            }
            var newProduct = new Product
            {
                Sku = request.Sku,
                Name = request.Name,
                BasePrice = request.BasePrice,
                IsActive = request.IsActive,
                BranchId = request.BranchId,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();
            return new ProductResponse
            {
                ProductId = newProduct.ProductId,
                Sku = newProduct.Sku,
                Name = newProduct.Name,
                BasePrice = newProduct.BasePrice,
                IsActive = newProduct.IsActive,
                CreatedAt = newProduct.CreatedAt,
                Branch = new BranchResponse
                {
                    BranchId = branch.BranchId,
                    Name = branch.Name,
                    Address = branch.Address,
                    PhoneNumber = branch.PhoneNumber
                },
                Category = new CategoryResponse
                {
                    CategoryId = category.CategoryId,
                    Name = category.Name,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt
                }
            };
        }

        public async Task<bool> DeleteProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new Exception("Product not found!");

            _context.Products.Remove(product);
            _context.SaveChanges();
            return true;
        }

        public async Task<ProductResponse?> FindByName(string name)
        {
            var product = await _context.Products.FindAsync(name);
            if (product == null)
                throw new Exception("Product not found!");
            return new ProductResponse
            {
                ProductId = product.ProductId,
                Sku = product.Sku,
                Name = product.Name,
                BasePrice = product.BasePrice,
                IsActive = product.IsActive,
                BranchId = product.BranchId,
                CategoryId = product.CategoryId,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };

        }

        //public Task<List<ProductResponse>> GetAllProducts()
        //{
        //    return _context.Products.
        //        Select(p => new ProductResponse{
        //        ProductId = p.ProductId,
        //        Sku = p.Sku,
        //        Name = p.Name,
        //        BasePrice = p.BasePrice,
        //        IsActive = p.IsActive,
        //        CreatedAt = p.CreatedAt,
        //        UpdatedAt = p.UpdatedAt,
        //        Branch = new BranchResponse
        //        {
        //            BranchId = p.Branch.BranchId,
        //            Name = p.Branch.Name,
        //            Address = p.Branch.Address,
        //            PhoneNumber = p.Branch.PhoneNumber
        //        },
        //        Category = new CategoryResponse
        //        {
        //            CategoryId = p.Category.CategoryId,
        //            Name = p.Category.Name,
        //            CreatedAt = p.Category.CreatedAt,
        //            UpdatedAt = p.Category.UpdatedAt
        //        }
        //   })
        //.ToListAsync();
        //}

        public async Task<ProductResponse?> GetProduct(int productId)
        {
            var product = await _context.Products
                    .Include(p => p.Branch)
                   .Include(p => p.Category)
                   .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
                throw new Exception("Product not found!");

            return new ProductResponse
            {
                ProductId = product.ProductId,
                Sku = product.Sku,
                Name = product.Name,
                BasePrice = product.BasePrice,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Branch = new BranchResponse
                {
                    BranchId = product.Branch.BranchId,
                    Name = product.Branch.Name,
                    Address = product.Branch.Address,
                    PhoneNumber = product.Branch.PhoneNumber
                },
                Category = new CategoryResponse
                {
                    CategoryId = product.Category.CategoryId,
                    Name = product.Category.Name,
                    CreatedAt = product.Category.CreatedAt,
                    UpdatedAt = product.Category.UpdatedAt
                }
            };
        }

        public async Task<ActionResult<PaginationModel<ProductResponse>>> GetAllProducts(ProductFilterVModel filter)
        {
            var query = _context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(p => p.Name.Contains(filter.Name));
            }
            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }
            if (filter.BranchId.HasValue)
            {
                query = query.Where(p => p.BranchId == filter.BranchId.Value);
            }
            if (filter.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == filter.IsActive.Value);
            }
            var totalRecords = await query.CountAsync();
            var records = await query
                .OrderByDescending(p => p.ProductId)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(p => new ProductResponse
                {
                    ProductId = p.ProductId,
                    Sku = p.Sku,
                    Name = p.Name,
                    BasePrice = p.BasePrice,
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    Branch = new BranchResponse
                    {
                        BranchId = p.Branch.BranchId,
                        Name = p.Branch.Name,
                        Address = p.Branch.Address,
                        PhoneNumber = p.Branch.PhoneNumber
                    },
                    Category = new CategoryResponse
                    {
                        CategoryId = p.Category.CategoryId,
                        Name = p.Category.Name,
                        CreatedAt = p.Category.CreatedAt,
                        UpdatedAt = p.Category.UpdatedAt
                    }
                })
                .ToListAsync();
            return new PaginationModel<ProductResponse>
            {
                Records = records,
                TotalRecords = totalRecords
            };
        }

        public async Task<ProductResponse?> UpdateProduct(ProductUpdateVModel request, int Id)
        {
            var product =await _context.Products.FindAsync(Id);
            if (product == null)
                throw new Exception("Product not found!");
            product.Sku = request.Sku;
            product.Name = request.Name;
            product.BasePrice = request.BasePrice;
            product.IsActive = request.IsActive;
            product.BranchId = request.BranchId;
            product.CategoryId = request.CategoryId;
            product.UpdatedAt = DateTime.UtcNow;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return new ProductResponse
            {
                ProductId = product.ProductId,
                Sku = product.Sku,
                Name = product.Name,
                BasePrice = product.BasePrice,
                IsActive = product.IsActive,
                Branch = new BranchResponse
                {
                    BranchId = product.Branch.BranchId,
                    Name = product.Branch.Name,
                    Address = product.Branch.Address,
                    PhoneNumber = product.Branch.PhoneNumber
                },
                Category = new CategoryResponse
                {
                    CategoryId = product.Category.CategoryId,
                    Name = product.Category.Name,
                    CreatedAt = product.Category.CreatedAt,
                    UpdatedAt = product.Category.UpdatedAt
                },
                UpdatedAt = product.UpdatedAt
            };

        }
    }
}
