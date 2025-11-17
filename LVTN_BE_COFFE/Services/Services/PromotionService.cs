using LVTN_BE_COFFE.Domain.VModel;
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

        // ----------------------------------------------------
        // CRUD Operations
        // ----------------------------------------------------

        public async Task<Promotion?> GetPromotionByIdAsync(int id)
        {
            return await _context.Promotions.FindAsync(id);
        }

        public async Task<IEnumerable<Promotion>> GetAllPromotionsAsync()
        {
            return null;
        }

        public async Task<Promotion> CreatePromotionAsync(PromotionCreateVModel model)
        {
            // Kiểm tra trùng lặp Code trước khi tạo
            if (await _context.Promotions.AnyAsync(p => p.Code == model.Code))
            {
                throw new InvalidOperationException($"Mã khuyến mãi '{model.Code}' đã tồn tại.");
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
                ApplicableProducts = model.ApplicableProducts
            };

            _context.Promotions.Add(promotion);
            await _context.SaveChangesAsync();
            return promotion;
        }

        // ... (Thêm logic Update và Delete tương tự) ...

        public Task<Promotion?> UpdatePromotionAsync(int id, PromotionUpdateVModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePromotionAsync(int id)
        {
            throw new NotImplementedException();
        }


        // ----------------------------------------------------
        // Apply Promotion Logic
        // ----------------------------------------------------

        public async Task<decimal?> ApplyPromotionAsync(string code, decimal orderTotal, List<int> productIds)
        {
            var promotion = await _context.Promotions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code);

            if (promotion == null)
            {
                return null; // Mã không tồn tại
            }

            // 1. Kiểm tra thời hạn
            if (!promotion.IsActive) // Dùng thuộc tính [NotMapped] IsActive
            {
                return null; // Hết hạn
            }

            // 2. Kiểm tra giá trị đơn hàng tối thiểu
            if (promotion.MinOrderValue.HasValue && orderTotal < promotion.MinOrderValue.Value)
            {
                return null; // Không đạt giá trị tối thiểu
            }

            // 3. Kiểm tra sản phẩm áp dụng (Rất phức tạp, chỉ làm logic cơ bản)
            // Nếu promotion.ApplicableProducts có giá trị, cần phải kiểm tra xem
            // có sản phẩm nào trong productIds nằm trong danh sách áp dụng không.
            // Tạm thời bỏ qua logic chi tiết này.

            // 4. Tính toán số tiền giảm giá
            decimal discountAmount;

            if (promotion.DiscountType == PromotionType.Percentage)
            {
                discountAmount = orderTotal * (promotion.DiscountValue / 100m);

                // Áp dụng giới hạn giảm giá tối đa
                if (promotion.MaxDiscountAmount.HasValue && discountAmount > promotion.MaxDiscountAmount.Value)
                {
                    discountAmount = promotion.MaxDiscountAmount.Value;
                }
            }
            else // Fixed
            {
                // Số tiền giảm giá cố định không được vượt quá tổng đơn hàng
                discountAmount = Math.Min(promotion.DiscountValue, orderTotal);
            }

            // 5. Kiểm tra giới hạn sử dụng (Nếu cần) - Tạm thời bỏ qua logic đếm lượt sử dụng.

            return discountAmount;
        }

    }
}
