using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class OrderItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal PriceAtPurchase { get; set; }

    [NotMapped]
    public decimal Subtotal => PriceAtPurchase * Quantity;

    // Navigation properties
    [ForeignKey(nameof(OrderId))]
    public Order Order { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }
}
