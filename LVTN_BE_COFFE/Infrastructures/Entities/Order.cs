using LVTN_BE_COFFE.Infrastructures.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Order : BaseEntity
{
    [Key]
    public int OrderId { get; set; }

    public string? UserId { get; set; }
    public string? GuestKey { get; set; }

    [Required, StringLength(150)]
    public string ReceiverName { get; set; } = null!;

    [Required, StringLength(20)]
    public string ReceiverPhone { get; set; } = null!;

    public string? ReceiverEmail { get; set; }

    [Required, StringLength(500)]
    public string ShippingAddressSnapshot { get; set; } = null!;

    [Required]
    public decimal TotalAmount { get; set; }

    [Required]
    public decimal ShippingFee { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;

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
    public DateTime OrderDate { get; set; } = DateTime.Now;

    // Navigation
    [ForeignKey(nameof(UserId))]
    public AspNetUsers? User { get; set; }

    [ForeignKey(nameof(PromotionId))]
    public Promotion? Promotion { get; set; }

    public int? ShippingAddressId { get; set; }
    [ForeignKey(nameof(ShippingAddressId))]
    public ShippingAddress? ShippingAddress { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [NotMapped]
    public int ItemCount => OrderItems?.Sum(i => i.Quantity) ?? 0;

    [NotMapped]
    public decimal FinalAmount
    {
        get
        {
            var final = (TotalAmount + ShippingFee) - DiscountAmount;
            return final < 0 ? 0 : final;
        }
    }
}