using LVTN_BE_COFFE.Infrastructures.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class Product
{
    [Key]
    public int ProductId { get; set; }

    // 🔗 Khóa ngoại đến ProductType
    [Required]
    public int CategoryId { get; set; }

    // 🔗 Khóa ngoại đến Branch
    [Required]
    public int BranchId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty; // Mã sản phẩm riêng (VD: TS001)

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty; // Tên sản phẩm

    [Range(0, double.MaxValue)]
    public decimal BasePrice { get; set; } // Giá cơ bản

    public bool IsActive { get; set; } = true; // Còn bán hay ngừng bán

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public Category Category { get; set; } = null!;
    public Branch Branch { get; set; } = null!;

    // Liên kết sang các biến thể (size, topping...)
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    // Danh sách ảnh của sản phẩm
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}
