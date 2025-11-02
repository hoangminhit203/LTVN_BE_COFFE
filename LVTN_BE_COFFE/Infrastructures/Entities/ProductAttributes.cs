using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ProductAttribute
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    public string? RoastLevel { get; set; } // ENUM: light, medium, dark
    public string? BeanType { get; set; } // ENUM: arabica, robusta, blend

    [StringLength(100)]
    public string? Origin { get; set; }

    public int? Acidity { get; set; }

    public decimal? Weight { get; set; }

    [StringLength(255)]
    public string? Certifications { get; set; }

    // Navigation property
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }
}