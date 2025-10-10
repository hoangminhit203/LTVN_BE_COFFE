namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class ProductVariant
    {
        public int ProductVariantId { get; set; }

        // Mối quan hệ 1 - N với Product
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Mối quan hệ 1 - N với Size
        public int SizeId { get; set; }
        public Size Size { get; set; } = null!;

        // Giá riêng cho từng size
        public decimal Price { get; set; }

        // Theo dõi thời gian tạo & cập nhật
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Liên kết N - N với topping thông qua bảng trung gian
        public ICollection<ProductVariantTopping> ProductVariantToppings { get; set; } = new List<ProductVariantTopping>();
    }
}
