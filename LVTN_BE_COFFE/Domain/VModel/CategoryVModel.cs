using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.Model;
using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    // Tạo mới category
    public class CategoryCreateVModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }
    }

    // Cập nhật category
    public class CategoryUpdateVModel : CategoryCreateVModel
    {
        [Required]
        public int CategoryId { get; set; }
    }

    // Kết quả trả về cho client
    public class CategoryResponse
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }

        // Nếu bạn muốn hiển thị sản phẩm trong category:
        //public List<ProductResponse>? Products { get; set; }
    }

    // Bộ lọc khi lấy danh sách category
    public class CategoryFilterVModel
    {
        public string? Name { get; set; }
        public int PageNumber { get; set; } = Numbers.Pagination.DefaultPageNumber;
        public int PageSize { get; set; } = Numbers.Pagination.DefaultPageSize;
    }
}
