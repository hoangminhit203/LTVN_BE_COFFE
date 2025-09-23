using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IProductService
    {
        Task<ProductResponse?> CreateProduct(ProductRequest request);
        Task<ProductResponse?> UpdateProduct(ProductRequest request, int productId);
        Task<bool?> DeleteProduct(int productId); // Delete nên trả bool thay vì Products
        Task<List<ProductResponse?>> GetAllProducts();
        Task<ProductResponse?> GetProduct(int productId);
        Task<bool> FindByName(string name);

    }
}
