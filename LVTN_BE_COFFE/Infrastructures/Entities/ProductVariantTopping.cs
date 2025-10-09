namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class ProductVariantTopping
    {
        public ProductVariant ProductVariant { get; set; } = null!;
        public int ProductVariantId { get; set; }
        public Topping Topping { get; set; } = null!;
        public int ToppingId { get; set; }
    }
}
