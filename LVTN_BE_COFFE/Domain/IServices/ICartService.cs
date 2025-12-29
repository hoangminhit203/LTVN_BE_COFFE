using LVTN_BE_COFFE.Domain.VModel;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface ICartService
    {
        Task<CartResponse?> GetCartAsync(string? userId, string? guestKey);
        Task<CartResponse> CreateCartIfNotExistsAsync(string? userId, string? guestKey);
        Task<bool> ClearCartAsync(int cartId);
    }
}
