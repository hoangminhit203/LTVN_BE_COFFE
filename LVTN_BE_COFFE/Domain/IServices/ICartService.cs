using LVTN_BE_COFFE.Domain.VModel;

public interface ICartService
{
    Task<CartResponse?> GetCartByUserAsync(string userId);
    Task<CartResponse> CreateCartAsync(string userId);
    Task<bool> ClearCartAsync(int cartId);
}
