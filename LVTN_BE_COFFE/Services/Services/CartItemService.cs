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

        /// <summary>
        /// Thêm sản phẩm vào cartItem của user
        /// </summary>
        public async Task<ActionResult<CartItemResponse>> AddItem(string userId, CartItemCreateVModel request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Quantity <= 0) throw new ArgumentException("Quantity must be greater than 0.");

            // 1) Validate variant
            var variant = await _context.ProductVariant
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == request.ProductVariantId);

            if (variant == null)
                throw new InvalidOperationException("Product variant not found.");

            // 2) Ensure user has a cart
            var cartResponse = await _cartService.CreateCartIfNotExistsAsync(userId);
            var cartId = cartResponse.CartId;

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId);

            if (cart == null)
                throw new InvalidOperationException("Cart not found or not owned by user.");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var existing = cart.CartItems.FirstOrDefault(ci => ci.ProductVariantId == request.ProductVariantId);

                if (existing != null)
                {
                    // Nếu đã có trong cart, cộng dồn quantity
                    existing.Quantity += request.Quantity;
                    _context.CartItems.Update(existing);
                }
                else
                {
                    // Thêm mới
                    var newItem = new CartItem
                    {
                        CartId = cart.Id,
                        UserId = userId,
                        ProductVariantId = variant.Id,
                        ProductVariant = variant,
                        Quantity = request.Quantity,
                        UnitPrice = variant.Price,
                        AddedAt = DateTime.UtcNow
                    };
                    await _context.CartItems.AddAsync(newItem);
                }

                // Update cart tổng
                cart.UpdatedAt = DateTime.UtcNow;
                _context.Carts.Update(cart);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                var item = await _context.CartItems
                    .Include(ci => ci.ProductVariant)
                    .ThenInclude(v => v.Product)
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductVariantId == request.ProductVariantId);

                return MapToResponse(item);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<ActionResult<IEnumerable<CartItemResponse>>> GetItemsByUserId(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == "Active");

            if (cart == null)
                return new List<CartItemResponse>();

            return cart.CartItems.Select(MapToResponse).ToList();
        }

        public async Task<ActionResult<bool>> RemoveItem(string userId, int cartItemId)
        {
            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);

            if (item == null)
                throw new InvalidOperationException("Cart item not found or does not belong to user.");

            _context.CartItems.Remove(item);
            item.Cart.UpdatedAt = DateTime.UtcNow;
            _context.Carts.Update(item.Cart);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ActionResult<CartItemResponse>> UpdateItem(string userId, CartItemUpdateVModel request)
        {
            var item = await _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.ProductVariant)
                .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId && ci.Cart.UserId == userId);

            if (item == null)
                throw new InvalidOperationException("Cart item not found or does not belong to user.");
            if (request.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0.");

            item.Quantity = request.Quantity;
            item.Cart.UpdatedAt = DateTime.UtcNow;

            _context.CartItems.Update(item);
            _context.Carts.Update(item.Cart);
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
                AddedAt = item.AddedAt
            };
        }
    }
}
