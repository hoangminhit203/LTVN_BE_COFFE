using LVTN_BE_COFFE.Infrastructures.Entities;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ProductRequest
    {
        public int ProductTypeId { get; set; }
        public string Sku { get; set; }//mã sản phẩm
        public string Name { get; set; }
        public decimal BasePrice { get; set; }// giá cơ bản
        public bool IsActive { get; set; }
        public ProductType ProductType { get; set; } // đay là lí do nó lấy hết product type trong product request
    }
}
