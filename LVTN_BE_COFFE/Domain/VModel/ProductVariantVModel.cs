using LVTN_BE_COFFE.Infrastructures.Entities;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ProductVariantCreateVModel
    {
        public int ProductId { get; set; }
        public int SizeId { get; set; }
        public decimal Price { get; set; }
        public List<int>? ToppingIds { get; set; } // danh sách topping kèm theo
    }

    public class ProductVariantUpdateVModel : ProductVariantCreateVModel
    {
        public int ProductVariantId { get; set; }
    }

    public class ProductVariantResponse : ProductVariantUpdateVModel
    {
        public string? ProductName { get; set; }
        public string? SizeName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<string>? ToppingNames { get; set; }
    }
}
