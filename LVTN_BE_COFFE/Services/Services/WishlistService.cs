using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
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

        public async Task<ActionResult<ResponseResult>> AddToWishlist(string userId, WishlistCreateVModel model)
        {
            var exists = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductVariantId == model.VariantId);
            if (exists!=null)
            {
               var responseExists = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Sản phẩm đã có trong danh sách yêu thích"
                };
                return new ActionResult<ResponseResult>(responseExists);

            }
            var wishlist = new Wishlist
            {
                UserId = userId,
                ProductVariantId = model.VariantId,
                AddedAt = DateTime.UtcNow
            };
            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();
            await _context.Entry(wishlist).Reference(w => w.ProductVariant).LoadAsync();
            return new ResponseResult
            {
                IsSuccess = true,
                Message = "Thêm sản phẩm vào danh sách yêu thích thành công",
                Data = MapToWishlistResponseVModel(wishlist)
            };
        }

        public async Task<ActionResult<ResponseResult>> RemoveFromWishlistById(int wishlistId, string userId)
        {
            var wishlist =  _context.Wishlists
                .FirstOrDefault(w => w.Id == wishlistId && w.UserId == userId);
            if (wishlist == null)
            {
                var responseNotFound = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy sản phẩm trong danh sách yêu thích"
                };
                return new ActionResult<ResponseResult>(responseNotFound);
            }
            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync();
            var responseSuccess = new ResponseResult
            {
                IsSuccess = true,
                Message = "Xóa sản phẩm khỏi danh sách yêu thích thành công"
            };
            return new ActionResult<ResponseResult>(responseSuccess);
        }

        public async Task<ActionResult<ResponseResult>> IsProductInWishlist(string userId, int productId)
        {
            var exists = await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductVariantId == productId);
            return new ResponseResult
            {
                IsSuccess = true,
                Data = exists
            };
        }

        public async Task<ActionResult<ResponseResult>> AddToCard(string userId, int wishlistId)
        {
            // 1. Tìm item trong wishlist
            var existingItem = await _context.Wishlists
                        .Include(w => w.ProductVariant)
                        .FirstOrDefaultAsync(w => w.Id == wishlistId && w.UserId == userId);

            if (existingItem == null)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy sản phẩm trong danh sách yêu thích"
                };
            }

            // 2. Tìm hoặc Tạo giỏ hàng
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                // Lưu để tránh lỗi khóa ngoại khi thêm CartItem
                await _context.SaveChangesAsync();
            }

            // 3. Xử lý thêm vào giỏ hàng
            var cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductVariantId == existingItem.ProductVariantId);

            if (cartItem != null)
            {
                cartItem.Quantity += 1;
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = existingItem.ProductVariantId,
                    UnitPrice = existingItem.ProductVariant?.Price ?? 0m,
                    Quantity = 1
                });
            }

            await _context.SaveChangesAsync();

            return new ResponseResult
            {
                IsSuccess = true,
                Message = "Thêm sản phẩm vào giỏ hàng thành công",
                Data = MapToWishlistResponseVModel(existingItem)
            };
        }
    }
}
