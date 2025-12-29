using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly AppDbContext _context;

        public WishlistService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WishlistResponseVModel> AddToWishlist(string userId, WishlistCreateVModel model)
        {
            var exists = await IsProductInWishlist(userId, model.ProductId);
            if (exists)
            {
                throw new Exception("Sản phẩm đã có trong danh sách yêu thích");
            }

            var wishlist = new Wishlist
            {
                UserId = userId,
                ProductVariantId = model.ProductId,
                AddedAt = DateTime.UtcNow
            };

            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();

            // Load đầy đủ product
            await _context.Entry(wishlist).Reference(w => w.ProductVariant).LoadAsync();
            return MapToWishlistResponseVModel(wishlist);
        }

        public async Task<bool> IsProductInWishlist(string userId, int productId)
        {
            return await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductVariantId == productId);
        }

        public async Task<bool> RemoveFromWishlistById(int wishlistId, string userId)
        {
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == userId);
            if (wishlist == null) return false;

            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ActionResult<WishlistResponseVModel>> AddToCard(string userId, int wishlistId)
        {
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == userId);

            if (wishlist == null)
                throw new Exception("Không tìm thấy sản phẩm trong danh sách yêu thích");

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductVariantId == wishlist.ProductVariantId);

            if (existingItem != null)
            {
                existingItem.Quantity += 1;
            }
            else
            {
                await _context.CartItems.AddAsync(new CartItem
                {
                    CartId = cart.Id, 
                    ProductVariantId = wishlist.ProductVariantId,
                    Quantity = 1
                });
            }

            await _context.SaveChangesAsync();

            return MapToWishlistResponseVModel(wishlist);
        }
        private WishlistResponseVModel MapToWishlistResponseVModel(Wishlist wishlist)
        {
            var mainImageUrl = wishlist.ProductVariant?.Images?
                        .FirstOrDefault(i => i.IsMain)?.ImageUrl
                    ?? wishlist.ProductVariant?.Images?
                        .FirstOrDefault()?.ImageUrl
                    ?? string.Empty;

            return new WishlistResponseVModel
            {
                Id = wishlist.Id,
                ProductId = wishlist.ProductVariantId,
                ProductName = wishlist.ProductVariant?.Product?.Name ?? "Unknown",
                ProductPrice = wishlist.ProductVariant?.Price ?? 0m,
                ProductImageUrl = mainImageUrl,
                AddedAt = wishlist.AddedAt
            };

        }

        public async Task<ActionResult<PaginationModel<WishlistResponseVModel>>> GetUserWishlist(string userId, WishlistFilterVModel filter)
        {
            var query = _context.Wishlists
                .Include(w => w.ProductVariant)
                    .ThenInclude(p => p.Images)
                .Where(w => w.UserId == userId)
                .AsQueryable();

            var totalRecords = await query.CountAsync();

            var wishlists = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var wishlistVModels = wishlists.Select(MapToWishlistResponseVModel).ToList();

            return new PaginationModel<WishlistResponseVModel>
            {
                TotalRecords = totalRecords,
                Records = wishlistVModels
            };
        }
    }
}
