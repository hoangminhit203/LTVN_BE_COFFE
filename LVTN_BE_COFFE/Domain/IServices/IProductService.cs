using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IProductService
    {
        Task<ActionResult<ProductResponse>?> CreateProduct(ProductCreateVModel request);
        Task<ActionResult<ProductResponse>?> UpdateProduct(ProductUpdateVModel request, int id);
        Task<ActionResult<bool>> DeleteProduct(int id);
        Task<ActionResult<ProductResponse>?> GetProduct(int id);
        //Task<ActionResult<ProductResponse>?> FindByName(string name);
        Task<ActionResult<PaginationModel<ProductResponse>>> GetAllProducts(ProductFilterVModel filter);
    }
}