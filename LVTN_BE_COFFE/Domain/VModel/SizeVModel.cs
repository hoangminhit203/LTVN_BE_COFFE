using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class SizeCreateVModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        public decimal ExtraPrice { get; set; }
    }

    public class SizeUpdateVModel : SizeCreateVModel
    {
        [Required]
        public int SizeId { get; set; }
    }

    public class SizeResponse
    {
        public int SizeId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal ExtraPrice { get; set; }
    }
}
