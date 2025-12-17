using LVTN_BE_COFFE.Infrastructures.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class ProductFlavorNote : BaseEntity
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    public int FlavorNoteId { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }

    [ForeignKey(nameof(FlavorNoteId))]
    public FlavorNote FlavorNote { get; set; }
}