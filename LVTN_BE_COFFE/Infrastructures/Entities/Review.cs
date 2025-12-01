using LVTN_BE_COFFE.Infrastructures.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Review
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int VariantId { get; set; }

    [Required]
    public string? UserId { get; set; }

    [Required]
    public float Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey(nameof(VariantId))]
    public ProductVariant? Variant { get; set; }

    [ForeignKey(nameof(UserId))]
    public AspNetUsers? User { get; set; }
}