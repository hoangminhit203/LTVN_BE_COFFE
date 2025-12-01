using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IReviewService
    {
        /// Lấy danh sách tất cả đánh giá cho một sản phẩm cụ thể.
        Task<ActionResult<PaginationModel<ReviewResponseVModel>>> GetReviewsByProductId(int productId);

        /// Tạo một đánh giá mới.
        Task<ActionResult<ReviewResponseVModel>> CreateReview(ReviewCreateVModel model, string userId);

        /// Cập nhật một đánh giá đã tồn tại.
        Task<ActionResult<ReviewResponseVModel>> UpdateReview(ReviewUpdateVModel model, string UserId);

        /// Xóa một đánh giá dựa trên ID.
        Task<ActionResult<bool>> DeleteReview(int reviewId, string UserId);

        // (Tùy chọn) Lấy điểm trung bình của sản phẩm
        Task<(decimal AverageRating, int ReviewCount)> GetAverageRating(int productId);
    }
}
