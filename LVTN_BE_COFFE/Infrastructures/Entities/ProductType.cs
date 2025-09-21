using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
	public class ProductType
	{
        [Key] // Khóa chính
        public int ProductTypeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string TypeName { get; set; } = string.Empty;

        // Navigation: 1 ProductType có nhiều Products
        public ICollection<Products> Products { get; set; } = new List<Products>();
    }
}