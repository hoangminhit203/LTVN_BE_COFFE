using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    // public interface IProductImageService
    public interface IProductImageService
    {
        Task<ProductImageResponse> AddAsync(ProductImageCreateVModel model);
        Task<ProductImageResponse> UpdateAsync(ProductImageUpdateVModel model); // Sửa từ ImageId -> Id
        Task<bool> DeleteAsync(int id); // Sửa từ productImageId -> id
        Task<ProductImageResponse?> GetByIdAsync(int id); // Sửa từ productImageId -> id
        Task<List<ProductImageResponse>> GetByProductIdAsync(int productId);
        Task<List<ProductImageResponse>> GetByProductVariantIdAsync(int variantId);
        // Loại bỏ ActionResult<> trong Service Layer, chỉ trả về Model
        Task<PaginationModel<ProductImageResponse>> GetAllAsync(ProductImageFilterVModel filterVModel);
    }
}
