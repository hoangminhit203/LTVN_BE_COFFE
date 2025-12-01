using LVTN_BE_COFFE.Domain.VModel;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface ICartService
    {
        Task<CartResponse?> GetCartByUserAsync(string userId);
        Task<CartResponse> CreateCartIfNotExistsAsync(string userId);
        Task<bool> ClearCartAsync(int cartId, string userId);
        //Task<CartResponse> MapCartToResponseAsync(int cartId);
    }
}
