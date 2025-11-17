using LVTN_BE_COFFE.Infrastructures.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

public class Order
{
    [Key]
    public int OrderId { get; set; }

    public string? UserId { get; set; }

    [Required]
    public decimal TotalAmount { get; set; }

    [StringLength(50)]
    public string? ShippingMethod { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "pending";

    // 🔹 Dùng PromotionId làm khóa ngoại (chuẩn EF)
    public int? PromotionId { get; set; }

    [StringLength(50)]
    public string? VoucherCode { get; set; } // chỉ lưu mã code nếu cần

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // 🔹 Navigation
    [ForeignKey(nameof(UserId))]
    public AspNetUsers? User { get; set; }

    [ForeignKey(nameof(PromotionId))]
    public Promotion? Promotion { get; set; }

    public int ShippingAddressId { get; set; }
    [ForeignKey(nameof(ShippingAddressId))]
    public ShippingAddress ShippingAddress { get; set; }
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
            return final < 0 ? 0 : final;
        }
    }

    private decimal GetDiscountAmount()
    {
        if (Promotion == null) return 0m;
        if (!Promotion.IsActive) return 0m;
        if (Promotion.MinOrderValue.HasValue && TotalAmount < Promotion.MinOrderValue.Value)
            return 0m;

        decimal discount = Promotion.DiscountType switch
        {
            PromotionType.Percentage => (Promotion.DiscountValue / 100m) * TotalAmount,
            PromotionType.Fixed => Promotion.DiscountValue,
            _ => 0m
        };

        return Math.Min(discount, TotalAmount);
    }
}
