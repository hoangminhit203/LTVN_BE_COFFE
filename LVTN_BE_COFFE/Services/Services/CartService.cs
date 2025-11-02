using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Domain.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CartResponse?> GetCartByUserAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");

            return cart == null ? null : MapToResponse(cart);
        }

        public async Task<CartResponse> CreateCartAsync(int userId)
        {
            var existing = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");

            if (existing != null)
                return MapToResponse(existing);

            var cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return MapToResponse(cart);
        }

        public async Task<bool> ClearCartAsync(int cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null) return false;

            _context.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        private static CartResponse MapToResponse(Cart cart)
        {
            return new CartResponse
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                UserName = cart.User?.UserName,
                Status = cart.Status,
                TotalPrice = cart.TotalPrice,
                CreatedAt = cart.CreatedAt,
                Items = cart.CartItems.Select(i => new CartItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "",
                    ProductPrice = i.Product?.Price ?? 0,
                    Quantity = i.Quantity,
                    Subtotal = i.Subtotal,
                    AddedAt = i.AddedAt
                }).ToList()
            };
        }
    }
}
