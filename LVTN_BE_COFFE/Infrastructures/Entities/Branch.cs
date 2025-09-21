using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;//Tên chi chánh (vd: Chi Nhánh Hồ Chí Minh, Vũng Tàu...)

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty; // địa chỉ chi nhánh

        public string PhoneNumber { get; set; } = string.Empty; // số điện thoại cho từng chi nhánh

        // Navigation: 1 chi nhánh có nhiều sản phẩm
        public ICollection<Products> Products { get; set; }
    }
}
