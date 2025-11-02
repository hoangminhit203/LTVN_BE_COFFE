using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ProductBrewingMethod
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public int BrewingMethodId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }

    [ForeignKey(nameof(BrewingMethodId))]
    public BrewingMethod BrewingMethod { get; set; }
}