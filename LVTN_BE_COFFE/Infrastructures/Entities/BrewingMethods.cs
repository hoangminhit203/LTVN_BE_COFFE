using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class BrewingMethod
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; }

    public string? Description { get; set; }

    // Navigation property
    public ICollection<ProductBrewingMethod> ProductBrewingMethods { get; set; }
}