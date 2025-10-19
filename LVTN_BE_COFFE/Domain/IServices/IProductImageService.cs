using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IProductImageService
    {
        Task<ProductImageResponse> AddAsync(ProductImageCreateVModel model);
        Task<ProductImageResponse> UpdateAsync(ProductImageUpdateVModel model);
        Task<bool> DeleteAsync(int productImageId);
        Task<ProductImageResponse?> GetByIdAsync(int productImageId);
        Task<List<ProductImageResponse>> GetByProductIdAsync(int productId);
        Task<List<ProductImageResponse>> GetByProductVariantIdAsync(int variantId);
        Task<ActionResult<PaginationModel<ProductImageResponse>>> GetAllAsync(ProductImageFilterVModel filterVModel);
    }
}
