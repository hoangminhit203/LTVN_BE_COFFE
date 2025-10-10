using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ToppingCreateVModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 999999)]
        public decimal ExtraPrice { get; set; }
    }

    public class ToppingUpdateVModel : ToppingCreateVModel
    {
        public int ToppingId { get; set; }
    }

    public class ToppingResponse
    {
        public int ToppingId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal ExtraPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
