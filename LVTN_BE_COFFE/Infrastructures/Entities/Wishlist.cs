using LVTN_BE_COFFE.Infrastructures.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Wishlist
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string? UserId { get; set; } = null!;

    [Required]
    public int ProductVariantId { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public AspNetUsers? User { get; set; }

    [ForeignKey(nameof(ProductVariantId))]
    public ProductVariant ProductVariant { get; set; } = null!;
}
