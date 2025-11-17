using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Domain.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _context;

        public CartService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CartResponse?> GetCartByUserAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");

            return cart == null ? null : MapToResponse(cart);
        }

        /// <summary>
        /// Tạo cart nếu chưa tồn tại active cart cho user.
        /// Trả về CartResponse có CartId để service khác sử dụng.
        /// </summary>
        public async Task<CartResponse> CreateCartIfNotExistsAsync(string userId)
        {
            var existing = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");

            if (existing != null)
                return MapToResponse(existing);

            var cart = new Cart
            {
                UserId = userId,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            // reload with includes
            return MapToResponse(cart);
        }

        public async Task<bool> ClearCartAsync(int cartId, string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);

            if (cart == null) return false;

            _context.CartItems.RemoveRange(cart.CartItems);
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CartResponse> MapCartToResponseAsync(int cartId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == cartId);

            if (cart == null) throw new InvalidOperationException("Cart not found");

            return MapToResponse(cart);
        }

        private static CartResponse MapToResponse(Cart cart)
        {
            var response = new CartResponse
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                UserName = cart.User?.UserName,
                Status = cart.Status,
                TotalPrice = cart.CartItems?.Sum(i => i.Subtotal) ?? 0m,
                CreatedAt = cart.CreatedAt,
                Items = cart.CartItems?.Select(i => new CartItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? string.Empty,
                    ProductPrice = i.Product?.Price ?? 0m,
                    Quantity = i.Quantity,
                    Subtotal = i.Subtotal,
                    AddedAt = i.AddedAt
                }).ToList() ?? new List<CartItemResponse>()
            };

            return response;
        }
    }
}
