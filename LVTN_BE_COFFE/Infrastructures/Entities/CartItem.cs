using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class CartItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CartId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public int Quantity { get; set; } = 1;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(CartId))]
    public Cart Cart { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }
    [NotMapped]
    public decimal Subtotal => Product != null ? Product.Price * Quantity : 0;
}