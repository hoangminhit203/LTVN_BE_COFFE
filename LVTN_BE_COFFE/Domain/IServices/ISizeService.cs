using LVTN_BE_COFFE.Domain.VModel;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface ISizeService
    {
        Task<List<SizeResponse>> GetAllSizes();
        Task<SizeResponse?> GetSize(int sizeId);
        Task<SizeResponse> CreateSize(SizeCreateVModel request);
        Task<SizeResponse?> UpdateSize(SizeUpdateVModel request);
        Task<bool> DeleteSize(int sizeId);
    }
}
