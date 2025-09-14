using LVTN_BE_COFFE.Domain.VModel;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IAuthService
    {
        Task<string?> RegisterAsync(LoginRequest request);
        Task<string?> LoginAsync(LoginRequest request);
    }
}
