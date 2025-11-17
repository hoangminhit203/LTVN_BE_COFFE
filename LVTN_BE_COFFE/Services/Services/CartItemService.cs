using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures;
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
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0.");

            // 1) Validate product
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId);

            if (product == null)
                throw new InvalidOperationException("Product not found.");

            // 2) Ensure the user has a cart (AUTO CREATE)
            var cartResponse = await _cartService.CreateCartIfNotExistsAsync(userId);
            var cartId = cartResponse.CartId;

            // 3) Load the cart with items
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.cartId == cartId && c.UserId == userId);

            if (cart == null)
                throw new InvalidOperationException("Cart not found or not owned by user.");

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // 4) Check if item already exists
                var existing = cart.CartItems.FirstOrDefault(ci => ci.ProductId == request.ProductId);

                if (existing != null)
                {
                    existing.Quantity += request.Quantity;
                    existing.AddedAt = DateTime.UtcNow;
                    _context.CartItems.Update(existing);
                }
                else
                {
                    var newItem = new CartItem
                    {
                        CartId = cart.cartId,
                        UserId = userId,
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        AddedAt = DateTime.UtcNow
                    };

                    await _context.CartItems.AddAsync(newItem);
                }

                // 5) Update cart timestamp
                cart.UpdatedAt = DateTime.UtcNow;

                _context.Carts.Update(cart);

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                // 6) Load fresh item to return
                var item = await _context.CartItems
                    .Include(ci => ci.Product)
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.cartId && ci.ProductId == request.ProductId);

                if (item == null)
                    throw new InvalidOperationException("Failed to retrieve updated cart item.");

                return MapToResponse(item);
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }


        public Task<ActionResult<IEnumerable<CartItemResponse>>> GetItemsByUserId(string userId)
        {

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId && c.Status == "Active");
            if (cart == null) throw new InvalidOperationException("Cart not found for user.");

            //return cart.CartItems.Select(MapToResponse).ToList();
            return cart.CartItems != null
                ? Task.FromResult<ActionResult<IEnumerable<CartItemResponse>>>(cart.CartItems.Select(MapToResponse).ToList())
                : Task.FromResult<ActionResult<IEnumerable<CartItemResponse>>>(new List<CartItemResponse>());
        }

        public Task<ActionResult<bool>> RemoveItem(string userId, int cartItemId)
        {
            var item = _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefault(ci => ci.Id == cartItemId && ci.Cart.UserId == userId);
            if(item == null) throw new InvalidOperationException("Cart item not found or does not belong to user.");
            _context.CartItems.Remove(item);
            _context.Carts.Update(item.Cart);
            _context.SaveChanges();
            return Task.FromResult<ActionResult<bool>>(true);

        }

        public Task<ActionResult<CartItemResponse>> UpdateItem(string userId, CartItemUpdateVModel request)
        {
            var item = _context.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product)
                .FirstOrDefault(ci => ci.Id == request.CartItemId && ci.Cart.UserId == userId);
            if (item == null) throw new InvalidOperationException("Cart item not found or does not belong to user.");
            if (request.Quantity <= 0) throw new ArgumentException("Quantity must be greater than 0.");
            item.Quantity = request.Quantity;
            item.Cart.UpdatedAt = DateTime.UtcNow;
            _context.CartItems.Update(item);
            _context.Carts.Update(item.Cart);
            _context.SaveChanges();
            return Task.FromResult<ActionResult<CartItemResponse>>(MapToResponse(item));
        }

        private CartItemResponse MapToResponse(CartItem item)
        {
            return new CartItemResponse
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "Unknown",
                ProductPrice = item.Product?.Price ?? 0,
                Quantity = item.Quantity,
                Subtotal = (item.Product?.Price ?? 0) * item.Quantity,
                AddedAt = item.AddedAt
            };
        }
    }
}
