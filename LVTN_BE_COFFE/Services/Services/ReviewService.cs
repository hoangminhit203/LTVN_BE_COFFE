using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
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
            //var reviewsQuery = _context.Reviews
            //    .Where(r => r.ProductId == productId)
            //    .Include(r => r.User); // Giả sử bạn có navigation property User trong Review
            //var reviews = reviewsQuery
            //    .Select(r => MapToResponseModel(r, r.User != null ? r.User.UserName : null))
            //    .ToList();
            var reviews = await (from r in _context.Reviews
                                 join u in _context.Users on r.UserId equals u.Id
                                 where r.VariantId == VariantId
                                 select new ReviewResponseVModel
                                 {
                                     VariantId = r.VariantId,
                                     Rating = r.Rating,
                                     Comment = r.Comment,
                                     CreatedAt = r.CreatedAt,
                                     UserId = r.UserId,
                                     UserName = u.UserName
                                 }).ToListAsync();
            var result = new PaginationModel<ReviewResponseVModel> { 
                Records = reviews, 
                TotalRecords = reviews.Count 
            };
            return result;
        }

        //đảm bảo chho phép người dùng đánh giá 1 lần vào sản phẩm đã mua và có thể cập nhật đánh giá của họ
        public async Task<ActionResult<ReviewResponseVModel>> CreateReview(ReviewCreateVModel model, string userId)
        {
        
            var Purchased = await (from o in _context.Orders
                                  join oi in _context.OrderItems on o.OrderId equals oi.OrderId
                                  where o.UserId == userId && o.Status == "Completed" && oi.ProductVariantId == model.VariantId
                                   select oi).AnyAsync();
            if (!Purchased)
            {
                throw new InvalidOperationException("Bạn không thể đánh giá sản phẩm chưa sử dụng.");
            }
            var existingReview = _context.Reviews
                .AnyAsync(r => r.VariantId == model.VariantId && r.UserId == userId);
            if (existingReview != null)
            {
                throw new InvalidOperationException("Bạn đã đánh giá sản phẩm này rồi.");
            }
            var review = new Review
            {
                VariantId = model.VariantId,
                Rating = model.Rating,
                Comment = model.Comment,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Reviews.Add(review);
            _context.SaveChanges();
            var user = _context.Users.Find(userId);
            return MapToResponseModel(review, user?.UserName);
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
