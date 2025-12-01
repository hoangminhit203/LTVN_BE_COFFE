using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ProductVariantCreateVM
    {
        [Required]
        public string Sku { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; } = 0;

        public string? RoastLevel { get; set; }
        public string? BeanType { get; set; }
        public string? Origin { get; set; }
        public int? Acidity { get; set; }
        public decimal? Weight { get; set; }
        public string? Certifications { get; set; }
    }
    public class ProductVariantResponse
    {
        public int VariantId { get; set; }
        public string Sku { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int Stock { get; set; }

        public string? RoastLevel { get; set; }
        public string? BeanType { get; set; }
        public string? Origin { get; set; }
        public int? Acidity { get; set; }
        public decimal? Weight { get; set; }
        public string? Certifications { get; set; }

        public List<ProductImageResponse>? Images { get; set; }
    }

}
