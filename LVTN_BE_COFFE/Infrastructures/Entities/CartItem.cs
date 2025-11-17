using LVTN_BE_COFFE.Infrastructures.Entities;
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
    public string UserId { get; set; } = null!;

    [Required]
    public int Quantity { get; set; } = 1;

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(CartId))]
    public Cart Cart { get; set; }

    [ForeignKey(nameof(UserId))]
    public AspNetUsers? User { get; set; } = null!;

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }
    [NotMapped]
    public decimal UnitPrice { get; set; }   // giá lúc thêm vào giỏ
    public decimal Subtotal => UnitPrice * Quantity;
}