using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Infrastructures.Entities;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ProductCreateVModel
    {
   
        public required string Sku { get; set; }//mã sản phẩm
        public required string Name { get; set; }
        public decimal BasePrice { get; set; }// giá cơ bản
        public bool IsActive { get; set; }// trạng thái sản phẩm đã được kích hoạt hay chưa
        public required int CategoryId { get; set; }
        public required int BranchId { get; set; }
    }
    public class ProductUpdateVModel : ProductCreateVModel
    {
        public int ProductId { get; set; }//id sản phẩm
    }
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public bool IsActive { get; set; }

        public int BranchId { get; set; }
        public int CategoryId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public BranchResponse Branch { get; set; } = null!;
        public CategoryResponse Category { get; set; } = null!;

    }
    public class ProductFilterVModel
    {
        public string? Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedBy { get; set; }
        public int? CategoryId { get; set; }
        public int? BranchId { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = Numbers.Pagination.DefaultPageNumber;
        public int PageSize { get; set; } = Numbers.Pagination.DefaultPageSize;
    }
}
