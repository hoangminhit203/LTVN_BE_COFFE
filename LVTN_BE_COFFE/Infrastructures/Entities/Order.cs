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
    public decimal TotalAmount { get; set; } // Tổng tiền trước giảm giá và phí ship

    [StringLength(50)]
    public string? ShippingMethod { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "pending";

    public int? PromotionId { get; set; }

    [StringLength(50)]
    public string? VoucherCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
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
        return 0m;
    }
}