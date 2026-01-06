using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    // Bảng lưu thông tin yêu cầu Trả hàng/Hủy đơn khi có sự cố
    public class OrderReturn
    {
        [Key]
        public int Id { get; set; }

        // Liên kết với bảng Order
        [Required]
        public string OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        // Lý do khách hàng muốn trả/hủy (VD: Hàng lỗi, vỡ, giao sai...)
        public string Reason { get; set; }

        // Lưu danh sách link ảnh chứng minh (có thể lưu dạng chuỗi json hoặc chuỗi cách nhau bằng dấu phẩy)
        // Ví dụ: "img1.jpg;img2.jpg"
        public string ProofImages { get; set; }

        // Trạng thái của yêu cầu này (pending, approved, rejected)
        public string Status { get; set; } = "pending";

        // Phản hồi của Admin (nếu từ chối thì ghi lý do vào đây)
        public string? AdminNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}