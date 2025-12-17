using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;

public interface IProductImageService
{
    Task<ProductImageResponse> AddAsync(ProductImageCreateVModel model);

    Task<List<ProductImageResponse>> AddMultipleAsync(ProductImageMultiCreateVModel model);

    Task<ProductImageResponse> UpdateAsync(ProductImageUpdateVModel model);

    Task<bool> DeleteAsync(int id);

    Task<ProductImageResponse?> GetByIdAsync(int id);

    Task<List<ProductImageResponse>> GetByProductIdAsync(int productId);

    Task<List<ProductImageResponse>> GetByProductVariantIdAsync(int variantId);

    Task<PaginationModel<ProductImageResponse>> GetAllAsync(ProductImageFilterVModel filterVModel);
}
