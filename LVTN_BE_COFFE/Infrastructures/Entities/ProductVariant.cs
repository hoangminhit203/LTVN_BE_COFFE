using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

[Index(nameof(Sku), IsUnique = true)]
public class ProductVariant
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required, StringLength(100)]
    public string Sku { get; set; } = null!;

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required]
    public int Stock { get; set; } = 0;

    public string? RoastLevel { get; set; }
    public string? BeanType { get; set; }

    [StringLength(100)]
    public string? Origin { get; set; }

    public int? Acidity { get; set; }

    public decimal? Weight { get; set; }

    [StringLength(255)]
    public string? Certifications { get; set; }

    // Navigation property
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Wishlist> Wishlist { get; set; }= new List<Wishlist>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}