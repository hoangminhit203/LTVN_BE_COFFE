using LVTN_BE_COFFE.Domain.Common;
using Microsoft.AspNetCore.Http; // để dùng IFormFile

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ProductImageCreateVModel
    {
        /// <summary>
        /// URL ảnh (nếu có sẵn, ví dụ link từ web khác). Bỏ trống nếu upload file.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>cd
        /// File ảnh upload (nếu upload từ máy)
        /// </summary>
        public IFormFile? File { get; set; } = null!;

        public int? ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public bool IsMain { get; set; } = false;
    }

    public class ProductImageUpdateVModel : ProductImageCreateVModel
    {
        public int ImageId { get; set; }
    }

    public class ProductImageResponse
    {
        public int ImageId { get; set; }

        /// <summary>
        /// URL ảnh trên Cloudinary
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Mã định danh duy nhất của ảnh trên Cloudinary
        /// </summary>
        public string? PublicId { get; set; }

        public int? ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public bool IsMain { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ProductImageFilterVModel
    {
        public int? ProductId { get; set; }
        public int? ProductVariantId { get; set; }

        public DateTime? UploadedFrom { get; set; }
        public DateTime? UploadedTo { get; set; }
        public DateTime? UpdatedFrom { get; set; }
        public DateTime? UpdatedTo { get; set; }

        public bool? IsMain { get; set; }

        public int PageNumber { get; set; } = Numbers.Pagination.DefaultPageNumber;
        public int PageSize { get; set; } = Numbers.Pagination.DefaultPageSize;
    }
}
