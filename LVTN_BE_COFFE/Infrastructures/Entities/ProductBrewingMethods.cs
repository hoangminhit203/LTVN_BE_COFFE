using LVTN_BE_COFFE.Infrastructures.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ProductBrewingMethod : BaseEntity
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