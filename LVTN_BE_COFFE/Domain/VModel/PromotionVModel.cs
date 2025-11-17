using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class PromotionCreateVModel
    {
        [Required(ErrorMessage = "Mã khuyến mãi là bắt buộc.")]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Loại chiết khấu là bắt buộc.")]
        public PromotionType DiscountType { get; set; }

        [Required(ErrorMessage = "Giá trị chiết khấu là bắt buộc.")]
        [Range(0, 100, ErrorMessage = "Giá trị chiết khấu phải từ 0 trở lên.")]
        public decimal DiscountValue { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giới hạn giảm giá tối đa phải từ 0 trở lên.")]
        public decimal? MaxDiscountAmount { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trị đơn hàng tối thiểu phải từ 0 trở lên.")]
        public decimal? MinOrderValue { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Giới hạn sử dụng phải từ 0 trở lên.")]
        public int? UsageLimit { get; set; }

        [StringLength(500)]
        public string? ApplicableProducts { get; set; } // Định dạng chuỗi ID hoặc JSON
    }

    public class PromotionUpdateVModel : PromotionCreateVModel
    {
        [Required(ErrorMessage = "Id khuyến mãi là bắt buộc.")]
        public int Id { get; set; }
    }
}
