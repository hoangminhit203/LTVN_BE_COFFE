using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class FlavorNote
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; }

    // Navigation property
    public ICollection<ProductFlavorNote> ProductFlavorNotes { get; set; }=new List<ProductFlavorNote>();
}