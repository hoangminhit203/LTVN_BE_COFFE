using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IWishlistService
    {
        /// Lấy danh sách yêu thích của người dùng (có phân trang).
        Task<ActionResult<PaginationModel<WishlistResponseVModel>>> GetUserWishlist(string userId, WishlistFilterVModel filter);

        /// Thêm một sản phẩm vào danh sách yêu thích của người dùng.
        Task<WishlistResponseVModel> AddToWishlist(string userId, WishlistCreateVModel model);

        /// Xóa một sản phẩm khỏi danh sách yêu thích dựa trên ID của Wishlist item.
        Task<bool> RemoveFromWishlistById(int wishlistId, string userId);

        /// Kiểm tra xem sản phẩm có tồn tại trong Wishlist của người dùng hay không.
        Task<bool> IsProductInWishlist(string userId, int productId);
        Task<ActionResult<WishlistResponseVModel>> AddToCard(string userId, int wishlistId);
    }
}
