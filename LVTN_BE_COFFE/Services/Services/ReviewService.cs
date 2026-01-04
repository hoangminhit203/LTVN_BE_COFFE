using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;
        public ReviewService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<ActionResult<PaginationModel<ReviewResponseVModel>>> GetReviewsByProductId(int VariantId)
        {
            // 1. Tìm ProductId từ VariantId truyền vào
            var productInfo = await _context.ProductVariant
                .Where(v => v.Id == VariantId)
                .Select(v => new { v.ProductId })
                .FirstOrDefaultAsync();

            if (productInfo == null)
                return new PaginationModel<ReviewResponseVModel> { Records = new List<ReviewResponseVModel>(), TotalRecords = 0 };

            // 2. Lấy tất cả VariantId của sản phẩm đó để gom tất cả review lại
            var variantIds = await _context.ProductVariant
                .Where(v => v.ProductId == productInfo.ProductId)
                .Select(v => v.Id)
                .ToListAsync();

            // 3. Truy vấn Review và lấy UserName qua Navigation Property (r.User)
            var reviews = await _context.Reviews
                .Include(r => r.User) // Đảm bảo nạp thông tin User
                .Where(r => variantIds.Contains(r.VariantId))
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewResponseVModel
                {
                    Id = r.Id,
                    VariantId = r.VariantId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UserId = r.UserId,
                    // Nếu có User thì lấy UserName, không thì để "Khách ẩn danh"
                    UserName = r.User != null ? r.User.UserName : "Khách ẩn danh"
                })
                .ToListAsync();

            return new PaginationModel<ReviewResponseVModel>
            {
                Records = reviews,
                TotalRecords = reviews.Count
            };
        }

        //đảm bảo chho phép người dùng đánh giá 1 lần vào sản phẩm đã mua và có thể cập nhật đánh giá của họ
        public async Task<ActionResult> SubmitReview(ReviewCreateVModel model, string userId)
        {
            // Tìm đánh giá cũ của User này cho sản phẩm này
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.VariantId == model.VariantId);

            if (existingReview != null)
            {
                // Nếu đã có -> Cập nhật nội dung mới và thời gian mới
                existingReview.Rating = model.Rating;
                existingReview.Comment = model.Comment;
                existingReview.CreatedAt = DateTime.Now; // Cập nhật ngày đánh giá mới nhất
                _context.Reviews.Update(existingReview);
            }
            else
            {
                // Nếu chưa có -> Tạo mới
                var newReview = new Review
                {
                    UserId = userId,
                    VariantId = model.VariantId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.Now
                };
                _context.Reviews.Add(newReview);
            }

            await _context.SaveChangesAsync();
            return new OkResult();
        }

        public async Task<ActionResult<ReviewResponseVModel>> UpdateReview(ReviewUpdateVModel model, string UserId)
        {
            var review = await _context.Reviews.FindAsync(model.Id);
            if (review == null || review.UserId != UserId)
            {
                throw new InvalidOperationException("Không tìm thấy đánh giá hoặc bạn không có quyền chỉnh sửa đánh giá này.");
            }
            review.Rating = model.Rating;
            review.Comment = model.Comment;
            _context.Reviews.Update(review);
            _context.SaveChanges();
            var user = _context.Users.Find(UserId);
            return MapToResponseModel(review, user?.UserName);
        }

        public async Task<ActionResult<bool>> DeleteReview(int reviewId, string UserId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null || review.UserId != UserId)
            {
                throw new InvalidOperationException("Không tìm thấy đánh giá hoặc bạn không có quyền xóa đánh giá này.");
            }
            _context.Reviews.Remove(review);
            _context.SaveChanges();
            return true;
        }

        //hiển thị điểm đánh giá trung bình và số lượng đánh giá cho một sản phẩm
        public async Task<(decimal AverageRating, int ReviewCount)> GetAverageRating(int VariantId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.VariantId == VariantId)
                .ToListAsync();
            if (reviews.Count == 0)
                return (0, 0);
            var averageRating = (decimal)reviews.Average(r => r.Rating);
            return (averageRating, reviews.Count);
        }

        private static ReviewResponseVModel MapToResponseModel(Review review, string? userName)
        {
            return new ReviewResponseVModel
            {
                Id = review.Id,
                VariantId = review.VariantId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                UserId = review.UserId,
                UserName = userName
            };
        }
    }
}
