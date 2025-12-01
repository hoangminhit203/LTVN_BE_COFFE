using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Index(nameof(Name), IsUnique = false)]
public class Product
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(255)]
    public string Name { get; set; }
    public string? Description { get; set; }

    public bool IsFeatured { get; set; } = false;
    public bool IsOnSale { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<ProductFlavorNote> ProductFlavorNotes { get; set; } = new List<ProductFlavorNote>();
    public ICollection<ProductBrewingMethod> ProductBrewingMethods { get; set; } = new List<ProductBrewingMethod>();
    public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}