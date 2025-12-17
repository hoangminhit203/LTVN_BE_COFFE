using LVTN_BE_COFFE.Infrastructures.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class BrewingMethod : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; }

    public string? Description { get; set; }

    // Navigation property
    public ICollection<ProductBrewingMethod> ProductBrewingMethods { get; set; } = new List<ProductBrewingMethod>();
}