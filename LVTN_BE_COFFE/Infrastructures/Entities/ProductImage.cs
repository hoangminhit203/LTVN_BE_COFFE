using LVTN_BE_COFFE.Infrastructures.Entities;
using System;
using System.ComponentModel.DataAnnotations;

public class ProductImage
{
    [Key]
    public int ImageId { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = null!;

    [MaxLength(255)]
    public string? PublicId { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public int? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public bool IsMain { get; set; } = false;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
