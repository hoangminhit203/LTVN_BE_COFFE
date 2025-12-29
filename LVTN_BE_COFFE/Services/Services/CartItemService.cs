using LVTN_BE_COFFE.Infrastructures.Entities;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Domain.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Domain.Services
{
    public class CartItemService : ICartItemService
    {
        private readonly AppDbContext _context;
        private readonly ICartService _cartService;

        public CartItemService(AppDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<ActionResult<CartItemResponse>> AddItem(string? userId, string? guestKey, CartItemCreateVModel request)
        {
            // 1) Validate biến thể sản phẩm
            var variant = await _context.ProductVariant
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId);

            if (variant == null) return new NotFoundObjectResult("Product variant not found.");

            // 2) Lấy hoặc tạo giỏ hàng dựa trên định danh
            var cartResp = await _cartService.CreateCartIfNotExistsAsync(userId, guestKey);

            // Lấy Entity Cart thực tế từ DB
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == cartResp.CartId);

            if (cart == null) return new BadRequestObjectResult("Could not initialize cart.");

            // 3) Xử lý thêm/cập nhật item
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductVariantId == request.ProductVariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
                existingItem.UnitPrice = variant.Price; // Cập nhật giá mới nhất
                _context.CartItems.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = variant.Id,
                    Quantity = request.Quantity,
                    UnitPrice = variant.Price,
                };
                await _context.CartItems.AddAsync(newItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Lấy lại item kèm theo thông tin Product để map response
            var resultItem = await _context.CartItems
                .Include(ci => ci.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductVariantId == variant.Id);

            return MapToResponse(resultItem!);
        }

        public async Task<ActionResult<IEnumerable<CartItemResponse>>> GetItems(string? userId, string? guestKey)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                    .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.Status == "Active" &&
                    ((userId != null && c.UserId == userId) || (guestKey != null && c.GuestKey == guestKey)));

            if (cart == null) return new List<CartItemResponse>();

            return cart.CartItems.Select(MapToResponse).ToList();
        }

        public async Task<ActionResult<bool>> RemoveItem(string? userId, string? guestKey, int cartItemId)
        {
            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId &&
                    ((userId != null && ci.Cart.UserId == userId) || (guestKey != null && ci.Cart.GuestKey == guestKey)));

            if (item == null) return new NotFoundObjectResult("Item not found in your cart.");

            _context.CartItems.Remove(item);
            item.Cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ActionResult<CartItemResponse>> UpdateItem(string? userId, string? guestKey, CartItemUpdateVModel request)
        {
            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId &&
                    ((userId != null && ci.Cart.UserId == userId) || (guestKey != null && ci.Cart.GuestKey == guestKey)));

            if (item == null) return new NotFoundObjectResult("Item not found in your cart.");

            item.Quantity = request.Quantity;
            item.Cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(item);
        }

        private CartItemResponse MapToResponse(CartItem item)
        {
            return new CartItemResponse
            {
                Id = item.Id,
                ProductVariantId = item.ProductVariantId,
                ProductName = item.ProductVariant?.Product?.Name ?? "Unknown",
                ProductPrice = item.UnitPrice,
                Quantity = item.Quantity,
                Subtotal = item.CalculatedSubtotal,
            };
        }
    }
}
