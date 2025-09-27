namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public int ProductTypeId { get; set; }
        public string Sku { get; set; }//mã sản phẩm
        public string Name { get; set; }
        public decimal BasePrice { get; set; }// giá cơ bản
        public bool IsActive { get; set; }
    }
}
