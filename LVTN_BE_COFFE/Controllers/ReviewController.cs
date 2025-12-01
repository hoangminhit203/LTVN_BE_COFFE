using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // Lấy userId từ token
        private string GetUserId()
        {
            if (!User.Identity.IsAuthenticated)
            {
                throw new System.Exception("User not authenticated");
            }
            var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? User.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new System.Exception("User ID claim not found in token.");
            }
            return userId;
        }
        // GET: api/Review/Product/{productId}
        [HttpGet]
        [Route("Review/{productId}")]
        public async Task<ActionResult<PaginationModel<ReviewResponseVModel>>> GetReviewsByProductId(int productId)
        {
            var result = await _reviewService.GetReviewsByProductId(productId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult> GetAverageRating(int productId)
        {
            var result = await _reviewService.GetAverageRating(productId);
            return Ok(result);
        }
        // POST: api/Review
        [HttpPost]
        public async Task<ActionResult<ReviewResponseVModel>> CreateReview([FromBody] ReviewCreateVModel model)
        {
            var userId = GetUserId();
            var result = await _reviewService.CreateReview(model, userId);
            return Ok(result);
        }
        // PUT: api/Review
        [HttpPut]
        public async Task<ActionResult<ReviewResponseVModel>> UpdateReview([FromBody] ReviewUpdateVModel model)
        {
            var userId = GetUserId();
            var result = await _reviewService.UpdateReview(model, userId);
            return Ok(result);
        }
        // DELETE: api/Review/{reviewId}
        [HttpDelete("{reviewId}")]
        public async Task<ActionResult<bool>> DeleteReview(int reviewId)
        {
            var userId = GetUserId();
            var result = await _reviewService.DeleteReview(reviewId, userId);
            return Ok(result);
        }

    }
}
