using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class BranchVModel
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;//Tên chi chánh (vd: Chi Nhánh Hồ Chí Minh, Vũng Tàu...)

        [MaxLength(500)]
        public string Address { get; set; } = string.Empty; // địa chỉ chi nhánh

        public string PhoneNumber { get; set; } = string.Empty;
        public int BranchId { get; internal set; }
    }

    public class BranchResponse : BranchVModel
    {
        public new int BranchId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }
    }
}
