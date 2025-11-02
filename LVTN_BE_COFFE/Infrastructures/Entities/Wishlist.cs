using LVTN_BE_COFFE.Infrastructures.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Wishlist
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int ProductId { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public AspNetUsers? User { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }
}