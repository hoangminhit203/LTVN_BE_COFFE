using LVTN_BE_COFFE.Infrastructures.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Order : BaseEntity
{
    [Key]
    public int OrderId { get; set; }

    public string? UserId { get; set; }

    [Required]
    // Đây là Tổng tiền hàng (SubTotal) = Tổng (Giá * Số lượng) của các món
    public decimal TotalAmount { get; set; }

    // --- MỚI THÊM: Phí vận chuyển ---
    [Required]
    public decimal ShippingFee { get; set; } = 0;

    // --- MỚI THÊM: Số tiền được giảm giá (Lưu cứng giá trị lúc đặt hàng) ---
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

    public int ShippingAddressId { get; set; }
    [ForeignKey(nameof(ShippingAddressId))]
    public ShippingAddress ShippingAddress { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [NotMapped]
    public int ItemCount => OrderItems?.Sum(i => i.Quantity) ?? 0;

    // --- CẬP NHẬT: Công thức tính tổng tiền cuối cùng ---
    // Công thức: (Tiền hàng + Tiền ship) - Tiền giảm giá
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