using System;
using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class CategoryCreateVModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }

    public class CategoryUpdateVModel : CategoryCreateVModel
    {
        [Required]
        public int CategoryId { get; set; }
    }

    public class CategoryResponse : CategoryUpdateVModel
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
