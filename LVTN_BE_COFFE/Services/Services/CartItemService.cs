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
    public class CartItemService : ControllerBase, ICartItemService
    {
        private readonly AppDbContext _context;

        public CartItemService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<CartItemResponse>> AddItemAsync(CartItemCreateVModel request)
        {
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.Id == request.CartId);
            if (cart == null) return NotFound("Cart not found.");

            var product = await _context.Products.FindAsync(request.ProductId);
            if (product == null) return NotFound("Product not found.");

            var existing = cart.CartItems.FirstOrDefault(x => x.ProductId == request.ProductId);
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
                    CartId = request.CartId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    AddedAt = DateTime.UtcNow
                };
                _context.CartItems.Add(newItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var item = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.CartId == request.CartId && ci.ProductId == request.ProductId);

            return Ok(MapToResponse(item));
        }

        public async Task<ActionResult<CartItemResponse>> UpdateItemAsync(CartItemUpdateVModel request)
        {
            var item = await _context.CartItems.Include(ci => ci.Product).FirstOrDefaultAsync(ci => ci.Id == request.CartItemId);
            if (item == null) return NotFound("Cart item not found.");

            item.Quantity = request.Quantity;
            item.AddedAt = DateTime.UtcNow;

            _context.CartItems.Update(item);
            await _context.SaveChangesAsync();
            return Ok(MapToResponse(item));
        }

        public async Task<ActionResult<bool>> RemoveItemAsync(int cartItemId)
        {
            var item = await _context.CartItems.FindAsync(cartItemId);
            if (item == null) return NotFound("Cart item not found.");

            _context.CartItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(true);
        }

        public async Task<ActionResult<IEnumerable<CartItemResponse>>> GetItemsByCartAsync(int cartId)
        {
            var items = await _context.CartItems.Include(ci => ci.Product).Where(ci => ci.CartId == cartId).ToListAsync();
            return Ok(items.Select(MapToResponse));
        }

        private static CartItemResponse MapToResponse(CartItem i)
        {
            return new CartItemResponse
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? "",
                ProductPrice = i.Product?.Price ?? 0,
                Quantity = i.Quantity,
                Subtotal = i.Subtotal,
                AddedAt = i.AddedAt
            };
        }
    }
}
