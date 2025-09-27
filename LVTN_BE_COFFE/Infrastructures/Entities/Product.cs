using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class Products
    {
        [Key]
        public int ProductId { get; set; }

        // Mỗi sản phẩm thuộc một loại (trà sữa, topping, đồ ăn kèm)
        public int ProductTypeId { get; set; }

        // Mỗi sản phẩm thuộc về một chi nhánh
        public int BranchId { get; set; }

        public string Sku { get; set; } = string.Empty; // Mã sản phẩm riêng (VD: TS001)

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty; // Tên sản phẩm

        public decimal BasePrice { get; set; } // Giá cơ bản

        public bool IsActive { get; set; } = true; // Còn bán hay ngừng bán

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }

        // Navigation
        public ProductType ProductType { get; set; }
        public Branch Branch { get; set; }
    }

}