using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IProductService
    {
        Task<ProductResponse?> CreateProduct(ProductCreateVModel request);
        Task<ProductResponse?> UpdateProduct(ProductUpdateVModel request, int Id);
        Task<bool> DeleteProduct(int productId); // Delete nên trả bool thay vì Products
        //Task<List<ProductResponse>> GetAllProducts();
        Task<ProductResponse?> GetProduct(int productId);
        Task<ProductResponse?> FindByName(string name);
        Task<ActionResult<PaginationModel<ProductResponse>>> GetAllProducts(ProductFilterVModel filter);

    }
}
