using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class PromotionCreateVModel : IValidatableObject
    {
        [Required(ErrorMessage = "Mã khuyến mãi là bắt buộc.")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Loại giảm giá là bắt buộc.")]
        public PromotionType DiscountType { get; set; }

        [Required(ErrorMessage = "Giá trị giảm giá là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá giảm giá khấu phải >= 0.")]
        public decimal DiscountValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giới hạn giảm giá tối đa phải >= 0.")]
        public decimal? MaxDiscountAmount { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu phải >= 0.")]
        public decimal? MinOrderValue { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Giới hạn sử dụng phải >= 0.")]
        public int? UsageLimit { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DiscountType == PromotionType.Percentage && DiscountValue > 100)
            {
                yield return new ValidationResult(
                    "Giảm giá theo phần trăm không được vượt quá 100%",
                    new[] { nameof(DiscountValue) });
            }

            if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
            {
                yield return new ValidationResult(
                    "Ngày bắt đầu không được lớn hơn ngày kết thúc",
                    new[] { nameof(StartDate), nameof(EndDate) });
            }
        }
    }

    public class PromotionUpdateVModel : IValidatableObject
    {
        [StringLength(255)]
        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MinOrderValue { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? MaxDiscountAmount { get; set; }

        [Range(0, int.MaxValue)]
        public int? UsageLimit { get; set; }

        public bool IsEnabled { get; set; }

        public PromotionType? DiscountType { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? DiscountValue { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (DiscountType == PromotionType.Percentage &&
                DiscountValue.HasValue &&
                DiscountValue > 100)
            {
                yield return new ValidationResult(
                    "Giảm giá phần trăm không được vượt quá 100%");
            }

            if (StartDate.HasValue && EndDate.HasValue &&
                StartDate > EndDate)
            {
                yield return new ValidationResult(
                    "Ngày bắt đầu không được lớn hơn ngày kết thúc");
            }
        }
    }

}