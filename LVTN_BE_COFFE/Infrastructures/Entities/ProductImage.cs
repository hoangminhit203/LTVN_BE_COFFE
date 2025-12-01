using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ProductImage
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = null!;

    [MaxLength(255)]
    public string? PublicId { get; set; }

    public int? ProductId { get; set; }
    [ForeignKey(nameof(ProductId))]
    public Product? Product { get; set; }

    public int? ProductVariantId { get; set; }
    [ForeignKey(nameof(ProductVariantId))]
    public ProductVariant? ProductVariant { get; set; }

    public bool IsMain { get; set; } = false;
    public int SortOrder { get; set; } = 0;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}