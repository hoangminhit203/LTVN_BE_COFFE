using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly AppDbContext _context;

        public PromotionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<ResponseResult>?> ApplyPromotionAsync(string code, decimal orderTotal)
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p =>
                    p.Code == code &&
                    p.IsEnabled &&
                    (!p.StartDate.HasValue || p.StartDate <= DateTime.Now) &&
                        (!p.EndDate.HasValue || p.EndDate >= DateTime.Now));

            if (promotion == null)
            {
                var Fail = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Mã khuyến mãi không hợp lệ hoặc đã hết hạn."
                };
                return new ActionResult<ResponseResult>(Fail);
            }

            if (promotion.MinOrderValue.HasValue &&
                orderTotal < promotion.MinOrderValue.Value)
            {
                var Fail = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Đơn hàng chưa đạt giá trị tối thiểu để áp dụng mã."
                };
                return new ActionResult<ResponseResult>(Fail);
            }

            if (promotion.UsageLimit.HasValue &&
                promotion.UsageCount >= promotion.UsageLimit.Value)
            {
                var Fail = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Mã khuyến mãi đã hết lượt sử dụng."
                };
                return new ActionResult<ResponseResult>(Fail);
            }

            decimal discountAmount = promotion.DiscountType switch
            {
                PromotionType.Percentage => orderTotal * promotion.DiscountValue / 100,
                PromotionType.Fixed => promotion.DiscountValue,
                _ => 0
            };

            if (promotion.MaxDiscountAmount.HasValue)
                discountAmount = Math.Min(discountAmount, promotion.MaxDiscountAmount.Value);
            discountAmount = Math.Min(discountAmount, orderTotal);

            return new ActionResult<ResponseResult>(new ResponseResult
            {
                IsSuccess = true,
                Message = "Áp dụng mã khuyến mãi thành công.",
                Data = new
                {
                    PromotionId = promotion.Id,
                    DiscountAmount = discountAmount
                }
            });
        }


        public async Task<ActionResult<ResponseResult>> CreatePromotionAsync(PromotionCreateVModel model)
        {
            var existingPromotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == model.Code);
            if (existingPromotion != null)
                {
                var Fail = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Mã khuyến mãi đã tồn tại."
                };
                return new ActionResult<ResponseResult>(Fail);
            }
            var promotion = new Promotion
            {
                Code = model.Code,
                Description = model.Description,
                DiscountType = model.DiscountType,
                DiscountValue = model.DiscountValue,
                MaxDiscountAmount = model.MaxDiscountAmount,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                MinOrderValue = model.MinOrderValue,
                UsageLimit = model.UsageLimit,
                IsEnabled = true
            };
            _context.Promotions.Add(promotion);
            _context.SaveChanges();
            var success = new ResponseResult
            {
                IsSuccess = true,
                Message = "Tạo mã khuyến mãi thành công.",
                Data = promotion
            };
            return new ActionResult<ResponseResult>(success);
        }

        public async Task<ActionResult<ResponseResult>> DeletePromotionAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                var notFoundResponse = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy mã khuyến mãi",
                    Data = null
                };
                return new ActionResult<ResponseResult>(notFoundResponse);
            }
            _context.Promotions.Remove(promotion);
            _context.SaveChanges();
            var response = new ResponseResult
            {
                IsSuccess = true,
                Message = "Xóa mã khuyến mãi thành công",
                Data = null
            };
            return new ActionResult<ResponseResult>(response);
        }

        public async Task<ActionResult<ResponseResult>> GetAllPromotionsAsync()
        {
            var promotions =await _context.Promotions.ToListAsync();
            var response = new ResponseResult
            {
                IsSuccess = true,
                Message = "Lấy danh sách mã khuyến mãi thành công",
                Data = promotions
            };
            return new ActionResult<ResponseResult>(response);
        }

        public async Task<ActionResult<ResponseResult>?> GetPromotionByIdAsync(int id)
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                var notFoundResponse = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy mã khuyến mãi",
                    Data = null
                };
                return new ActionResult<ResponseResult>(notFoundResponse);
            }
            var response = new ResponseResult
            {
                IsSuccess = true,
                Message = "Lấy mã khuyến mãi thành công",
                Data = promotion
            };
            return new ActionResult<ResponseResult>(response);
        }

        public async Task<ActionResult<ResponseResult>?> UpdatePromotionAsync(int id, PromotionUpdateVModel model)
        {
          var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                var notFoundResponse = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy mã khuyến mãi",
                    Data = null
                };
                return new ActionResult<ResponseResult>(notFoundResponse);
            }
            promotion.Description = model.Description ?? promotion.Description;
            promotion.StartDate = model.StartDate ?? promotion.StartDate;
            promotion.EndDate = model.EndDate ?? promotion.EndDate;
            promotion.MinOrderValue = model.MinOrderValue ?? promotion.MinOrderValue;
            promotion.MaxDiscountAmount = model.MaxDiscountAmount ?? promotion.MaxDiscountAmount;
            promotion.UsageLimit = model.UsageLimit ?? promotion.UsageLimit;
            promotion.IsEnabled = model.IsEnabled;
            if (model.DiscountType.HasValue)
                promotion.DiscountType = model.DiscountType.Value;
            if (model.DiscountValue.HasValue)
                promotion.DiscountValue = model.DiscountValue.Value;
            _context.Promotions.Update(promotion); 
            _context.SaveChanges();
            var response = new ResponseResult
            {
                IsSuccess = true,
                Message = "Cập nhật mã khuyến mãi thành công",
                Data = promotion
            };
            return new ActionResult<ResponseResult>(response);
        }
    }
}
