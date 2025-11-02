using LVTN_BE_COFFE.Domain.VModel;

public interface ICartService
{
    Task<CartResponse?> GetCartByUserAsync(int userId);
    Task<CartResponse> CreateCartAsync(int userId);
    Task<bool> ClearCartAsync(int cartId);
}
