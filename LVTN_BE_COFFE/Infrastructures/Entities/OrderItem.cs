using LVTN_BE_COFFE.Infrastructures.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class OrderItem : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int ProductVariantId { get; set; }

    [Required, StringLength(255)]
    public string ProductNameAtPurchase { get; set; } = null!;

    public string? VariantDetailsJson { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal PriceAtPurchase { get; set; }

    [NotMapped]
    public decimal Subtotal => PriceAtPurchase * Quantity;

    // Navigation properties
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; } = null!;

    [ForeignKey(nameof(ProductVariantId))]
    public ProductVariant ProductVariant { get; set; } = null!;
}