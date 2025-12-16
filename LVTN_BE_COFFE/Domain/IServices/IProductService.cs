using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IProductService
    {
        Task<ActionResult<ResponseResult>?> CreateProduct(ProductCreateVModel request);
        Task<ActionResult<ResponseResult>?> UpdateProduct(ProductUpdateVModel request, int id);
        Task<ActionResult<ResponseResult>> DeleteProduct(int id);
        Task<ActionResult<ResponseResult>?> GetProduct(int id);
        Task<ActionResult<ResponseResult>> GetAllProducts(ProductFilterVModel filter);
    }
}