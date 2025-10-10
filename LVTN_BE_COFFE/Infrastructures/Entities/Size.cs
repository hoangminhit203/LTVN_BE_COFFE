using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class Size
    {
        [Key]
        public int SizeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!; // S, M, L,...

        [Range(0, double.MaxValue)]
        public decimal ExtraPrice { get; set; }

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}
