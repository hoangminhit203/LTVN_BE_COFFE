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
        Task<ActionResult<ResponseResult>> AddToWishlist(string userId, WishlistCreateVModel model);

        /// Xóa một sản phẩm khỏi danh sách yêu thích dựa trên ID của Wishlist item.
        Task<ActionResult<ResponseResult>> RemoveFromWishlistById(int wishlistId, string userId);

        /// Kiểm tra xem sản phẩm có tồn tại trong Wishlist của người dùng hay không.
        Task<ActionResult<ResponseResult>> IsProductInWishlist(string userId, int productId);
        Task<ActionResult<ResponseResult>> AddToCard(string userId, int wishlistId);
    }
}
