using LVTN_BE_COFFE.Infrastructures.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

public class Order
{
    [Key]
    public int Id { get; set; }

    public int? UserId { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    [Required]
    [StringLength(255)]
    public string ShippingAddress { get; set; }

    [StringLength(50)]
    public string? ShippingMethod { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "pending"; // pending, processing, shipped, delivered, cancelled

    [StringLength(50)]
    public string? VoucherCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public AspNetUsers? User { get; set; }

    [ForeignKey(nameof(VoucherCode))]
    public Promotion? Promotion { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [NotMapped]
    public int ItemCount => OrderItems?.Sum(i => i.Quantity) ?? 0;

    [NotMapped]
    public decimal FinalAmount
    {
        get
        {
            var discount = GetDiscountAmount();
            var final = TotalAmount - discount;
            return final < 0 ? 0 : final; // không để âm
        }
    }

    private decimal GetDiscountAmount()
    {
        if (Promotion == null) return 0m;

        // optional: kiểm tra thời hạn và min order
        if (!Promotion.IsActive) return 0m;
        if (Promotion.MinOrderValue.HasValue && TotalAmount < Promotion.MinOrderValue.Value) return 0m;

        decimal discount = 0m;

        switch (Promotion.DiscountType)
        {
            case PromotionType.Percentage:
                // DiscountValue là percent, ví dụ 10 => 10%
                discount = (Promotion.DiscountValue / 100m) * TotalAmount;
                break;

            case PromotionType.Fixed:
                // DiscountValue là số tiền cố định
                discount = Promotion.DiscountValue;
                break;

            default:
                discount = 0m;
                break;
        }

        // Không được lớn hơn tổng tiền
        if (discount > TotalAmount) discount = TotalAmount;

        return discount;
    }

}