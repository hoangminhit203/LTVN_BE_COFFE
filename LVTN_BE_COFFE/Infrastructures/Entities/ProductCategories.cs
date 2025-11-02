using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ProductCategory
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }

    [ForeignKey(nameof(CategoryId))]
    public Category Category { get; set; }
}