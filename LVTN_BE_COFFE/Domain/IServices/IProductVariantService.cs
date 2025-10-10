using LVTN_BE_COFFE.Domain.VModel;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IProductVariantService
    {
        Task<IEnumerable<ProductVariantResponse>> GetAllVariants();
        Task<ProductVariantResponse?> GetVariantById(int id);
        Task<ProductVariantResponse> CreateVariant(ProductVariantCreateVModel model);
        Task<ProductVariantResponse?> UpdateVariant(ProductVariantUpdateVModel model);
        Task<bool> DeleteVariant(int id);
    }
}
