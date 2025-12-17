using LVTN_BE_COFFE.Infrastructures.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Category : BaseEntity
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; }

    public string? Description { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();

}
