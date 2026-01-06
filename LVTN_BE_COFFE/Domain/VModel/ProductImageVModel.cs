using LVTN_BE_COFFE.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ProductImageCreateVModel
    {
        public IFormFile? File { get; set; }
        public int? ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public bool IsMain { get; set; } = false;
        public int SortOrder { get; set; } = 0;
    }

    public class ProductImageUpdateVModel
    {
        public IFormFile? File { get; set; }
        public bool IsMain { get; set; } = false;
    }

    public class ProductImageResponse
    {
        public int Id { get; set; }

        public int? ProductId { get; set; }
        public int? ProductVariantId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsMain { get; set; }
        public int SortOrder { get; set; }

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
    public class ProductImageMultiCreateVModel
    {
        public int ProductId { get; set; }
        public int? ProductVariantId { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
