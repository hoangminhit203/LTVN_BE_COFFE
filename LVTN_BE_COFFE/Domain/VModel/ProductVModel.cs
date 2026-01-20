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

        // Đổi từ List<string> sang List<int> để nhận ID
        public List<int>? FlavorNoteIds { get; set; }
        public List<int>? BrewingMethodIds { get; set; }

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

        // Đổi từ List<string> sang List<int> để nhận ID
        public List<int>? FlavorNoteIds { get; set; }
        public List<int>? BrewingMethodIds { get; set; }

        public List<ProductVariantUpdateVModel>? Variants { get; set; }
    }

    // ProductResponse (Loại bỏ Sku, IsActive, BranchId; thêm từ ProductAttributes và navigation)
    public class ProductResponse
    {
        public int ProductId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Đổi từ List<string> sang List<FlavorNoteResponse> để trả về đầy đủ thông tin
        public List<FlavorNoteResponse>? FlavorNotes { get; set; }
        public List<BrewingMethodsResponse>? BrewingMethods { get; set; }

        public List<ProductVariantResponse>? Variants { get; set; }

        public List<CategoryResponse>? Category { get; set; }
    }

    // ProductFilterVModel (Loại bỏ BranchId, thêm lọc cho ProductAttributes)
    public class ProductFilterVModel
    {
        public string? Name { get; set; }
        public string? Origin { get; set; }
        
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int PageNumber { get; set; } = Numbers.Pagination.DefaultPageNumber;
        public int PageSize { get; set; } = Numbers.Pagination.DefaultPageSize;
    }
}