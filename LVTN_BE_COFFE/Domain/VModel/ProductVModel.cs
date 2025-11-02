using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    // Result model (giữ nguyên)
    //public class Result<T>
    //{
    //    public bool IsSuccess { get; set; }
    //    public T? Data { get; set; }
    //    public string? Message { get; set; }
    //    public string? Error { get; set; }
    //}

    // ProductCreateVModel (Loại bỏ Sku, IsActive, BranchId; thêm Description, IsFeatured, IsOnSale)
    public class ProductCreateVModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        public int Stock { get; set; } = 0;
        public string? ImageUrl { get; set; }
        public bool IsFeatured { get; set; } = false;
        public bool IsOnSale { get; set; } = false;
        [Required]
        public int CategoryId { get; set; }
        // Thuộc tính từ ProductAttributes
        public string? RoastLevel { get; set; }
        public string? BeanType { get; set; }
        public string? Origin { get; set; }
        public int? Acidity { get; set; }
        public decimal? Weight { get; set; }
        public string? Certifications { get; set; }
        public List<string>? FlavorNotes { get; set; }
        public List<string>? BrewingMethods { get; set; }
    }

    // ProductUpdateVModel
    public class ProductUpdateVModel : ProductCreateVModel
    {
        public int ProductId { get; set; }
    }

    // ProductResponse (Loại bỏ Sku, IsActive, BranchId; thêm từ ProductAttributes và navigation)
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsOnSale { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int CategoryId { get; set; }
        // Từ ProductAttributes
        public string? RoastLevel { get; set; }
        public string? BeanType { get; set; }
        public string? Origin { get; set; }
        public int? Acidity { get; set; }
        public decimal? Weight { get; set; }
        public string? Certifications { get; set; }
        // Từ FlavorNotes và BrewingMethods
        public List<string>? FlavorNotes { get; set; }
        public List<string>? BrewingMethods { get; set; }
        // Navigation property
        public CategoryResponse? Category { get; set; }
    }

    // ProductFilterVModel (Loại bỏ BranchId, thêm lọc cho ProductAttributes)
    public class ProductFilterVModel
    {
        public string? Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedBy { get; set; }
        public int? CategoryId { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsOnSale { get; set; }
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