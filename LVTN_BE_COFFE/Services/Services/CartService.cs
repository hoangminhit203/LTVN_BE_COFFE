using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
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

        public async Task<CartResponse?> GetCartAsync(string? userId, string? guestKey)
        {
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(guestKey)) return null;

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Images)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(p => p.Product)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Status == "Active" &&
                    ((userId != null && c.UserId == userId) ||
                     (guestKey != null && c.GuestKey == guestKey)));

            return cart == null ? null : MapToResponse(cart);
        }

        public async Task<CartResponse> CreateCartIfNotExistsAsync(string? userId, string? guestKey)
        {
            var existing = await GetCartAsync(userId, guestKey);
            if (existing != null) return existing;

            var cart = new Cart
            {
                UserId = userId,
                GuestKey = guestKey,
                Status = "Active",
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
            var items = cart.CartItems?.Select(static i => new CartItemResponse
            {
                Id = i.Id,
                ProductVariantId = i.ProductVariantId,
                ProductName = i.ProductVariant?.Product?.Name ?? "Unknown",
                ImageUrl = i.ProductVariant?.Images?.FirstOrDefault()?.ImageUrl // Thử lấy ảnh của Variant
                            ?? i.ProductVariant?.Product?.Images?.FirstOrDefault()?.ImageUrl // Nếu null, thử lấy ảnh của Product
                            ?? string.Empty,
                ProductPrice = i.UnitPrice,
                Quantity = i.Quantity,
                Subtotal = i.CalculatedSubtotal,
            }).ToList() ?? new List<CartItemResponse>();

            return new CartResponse
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                GuestKey = cart.GuestKey,
                UserName = cart.User?.UserName,
                Status = cart.Status,
                TotalPrice = items.Sum(i => i.Subtotal),
                CreatedAt = cart.CreatedAt,
                Items = items
            };
        }
    }
}
