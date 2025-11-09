using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

[Index(nameof(Name), IsUnique = false)] // FULLTEXT index for search
public class Product
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(255)]
    public string Name { get; set; }

    public string? Description { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int Stock { get; set; } = 0;

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    public bool IsFeatured { get; set; } = false;
    public bool IsOnSale { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ProductAttribute? ProductAttribute { get; set; }
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<ProductFlavorNote> ProductFlavorNotes { get; set; } = new List<ProductFlavorNote>();
    public ICollection<ProductBrewingMethod> ProductBrewingMethods { get; set; } = new List<ProductBrewingMethod>();
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}