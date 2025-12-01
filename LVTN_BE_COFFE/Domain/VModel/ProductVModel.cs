using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ProductCreateVModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public List<string>? FlavorNotes { get; set; }
        public List<string>? BrewingMethods { get; set; }
        public List<ProductVariantCreateVM>? Variants { get; set; }
    }

    // ProductUpdateVModel
    public class ProductUpdateVModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int CategoryId { get; set; }

        public List<string>? FlavorNotes { get; set; }
        public List<string>? BrewingMethods { get; set; }

        public List<ProductVariantCreateVM>? Variants { get; set; }
    }

    // ProductResponse (Loại bỏ Sku, IsActive, BranchId; thêm từ ProductAttributes và navigation)
    public class ProductResponse
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<string>? FlavorNotes { get; set; }
        public List<string>? BrewingMethods { get; set; }

        public List<ProductVariantResponse>? Variants { get; set; }

        public List<CategoryResponse>? Category { get; set; }
    }

    // ProductFilterVModel (Loại bỏ BranchId, thêm lọc cho ProductAttributes)
    public class ProductFilterVModel
    {
        public string? Name { get; set; }
        public int? CategoryId { get; set; }

        public string? RoastLevel { get; set; }
        public string? BeanType { get; set; }
        public string? Origin { get; set; }

        public int? MinAcidity { get; set; }
        public int? MaxAcidity { get; set; }

        public decimal? MinWeight { get; set; }
        public decimal? MaxWeight { get; set; }

        public string? FlavorNote { get; set; }
        public string? BrewingMethod { get; set; }

        public int PageNumber { get; set; } = Numbers.Pagination.DefaultPageNumber;
        public int PageSize { get; set; } = Numbers.Pagination.DefaultPageSize;
    }
}