using LVTN_BE_COFFE.Infrastructures.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CartItem : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CartId { get; set; }

    [Required]
    public int ProductVariantId { get; set; }

    public string UserId { get; set; } = null!;

    [Required]
    public int Quantity { get; set; } = 1;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(CartId))]
    public Cart Cart { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public AspNetUsers? User { get; set; } = null!;

    [ForeignKey(nameof(ProductVariantId))]
    public ProductVariant ProductVariant { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    [NotMapped]
    public decimal CalculatedSubtotal => UnitPrice * Quantity;
}