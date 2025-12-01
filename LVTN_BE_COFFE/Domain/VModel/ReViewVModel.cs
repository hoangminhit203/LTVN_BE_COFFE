using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ReviewCreateVModel
    {
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc.")]
        public int VariantId { get; set; }

        [Required(ErrorMessage = "Điểm đánh giá là bắt buộc.")]
        // Thêm ràng buộc để đảm bảo điểm nằm trong phạm vi cho phép (ví dụ: 1 đến 5)
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Bình luận không được vượt quá 500 ký tự.")]
        public string? Comment { get; set; }
    }
    public class ReviewUpdateVModel
    {
        [Required]
        public int Id { get; set; } // ID của đánh giá cần cập nhật

        [Required(ErrorMessage = "Điểm đánh giá là bắt buộc.")]
        [Range(1, 5, ErrorMessage = "Điểm đánh giá phải từ 1 đến 5.")]
        public int Rating { get; set; }

        [StringLength(500, ErrorMessage = "Bình luận không được vượt quá 500 ký tự.")]
        public string? Comment { get; set; }
    }
    public class ReviewResponseVModel
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public float Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        // Thông tin người dùng (được Load từ AspNetUsers Entity)
        public string? UserId { get; set; }
        public string? UserName { get; set; } // Tên hiển thị của người đánh giá
    }
}
