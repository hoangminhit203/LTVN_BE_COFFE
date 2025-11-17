using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum PromotionType
{
    Percentage,
    Fixed
}

public class Promotion
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    [Required]
    public PromotionType DiscountType { get; set; }

    [Required]
    // Điều chỉnh Range: Percentage chỉ từ 0-100, Fixed có thể lớn hơn 100
    [Range(0, double.MaxValue, ErrorMessage = "Discount value must be greater than or equal to 0.")]
    public decimal DiscountValue { get; set; }

    // Giới hạn giảm giá tối đa (chỉ áp dụng cho PromotionType.Percentage)
    [Column(TypeName = "decimal(18, 2)")]
    [Range(0, double.MaxValue)]
    public decimal? MaxDiscountAmount { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinOrderValue { get; set; }

    // Giới hạn số lần sử dụng tổng cộng của mã (Optional)
    [Range(0, int.MaxValue)]
    public int? UsageLimit { get; set; }

    [StringLength(500)]
    public string? ApplicableProducts { get; set; } // Comma-separated IDs or JSON string

    // 🔹 Navigation property
    public ICollection<Order> Orders { get; set; } = new List<Order>();

    [NotMapped]
    public bool IsActive =>
        (!StartDate.HasValue || StartDate <= DateTime.UtcNow) &&
        (!EndDate.HasValue || EndDate >= DateTime.UtcNow);
}