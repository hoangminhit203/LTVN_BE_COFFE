using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class Topping
    {
        [Key]
        public int ToppingId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Range(0, 999999)]
        public decimal ExtraPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Quan hệ nhiều - nhiều với ProductVariant thông qua bảng trung gian
        public ICollection<ProductVariantTopping> ProductVariantToppings { get; set; } = new List<ProductVariantTopping>();
    }
}
