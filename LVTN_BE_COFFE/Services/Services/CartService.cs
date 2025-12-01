using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Product)
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
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Product)
                .Include(c => c.User)
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

        private static CartResponse MapToResponse(Cart cart)
        {
            var items = cart.CartItems?.Select(i => new CartItemResponse
            {
                Id = i.Id,
                ProductVariantId = i.ProductVariantId,
                ProductName = i.ProductVariant?.Product?.Name ?? "Unknown",
                ProductPrice = i.UnitPrice,
                Quantity = i.Quantity,
                Subtotal = i.CalculatedSubtotal,
                AddedAt = i.AddedAt
            }).ToList() ?? new List<CartItemResponse>();

            return new CartResponse
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                UserName = cart.User?.UserName,
                Status = cart.Status,
                TotalPrice = items.Sum(i => i.Subtotal),
                CreatedAt = cart.CreatedAt,
                Items = items
            };
        }
    }
}
