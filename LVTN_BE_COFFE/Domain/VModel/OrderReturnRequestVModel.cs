using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ReturnOrderInputModel
    {
        [Required]
        public string Reason { get; set; }

        // Sử dụng IFormFile để nhận file ảnh upload
        public List<IFormFile>? Images { get; set; }
    }
    public class OrderReturnRequestVModel
    {
        public string Reason { get; set; }

        // Frontend sẽ gửi lên danh sách link ảnh (sau khi đã upload lên server/cloud)
        public List<string> ImageUrls { get; set; }
    }
    public class ProcessReturnModel
    {
        public string Action { get; set; } // "approve" hoặc "reject"
        public string? Note { get; set; } // Ghi chú của admin (VD: "Ảnh mờ quá", "Đã hoàn tiền")
    }
    public class OrderReturnResponse
    {
        public int Id { get; set; } // ID của yêu cầu trả hàng
        public string OrderId { get; set; }
        public string CustomerName { get; set; } // Tên khách hàng
        public string Reason { get; set; }
        public List<string> ProofImages { get; set; } // List ảnh đã tách ra
        public string Status { get; set; } // pending, approved, rejected
        public string AdminNote { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal RefundAmount { get; set; } // Số tiền dự kiến hoàn (TotalAmount của đơn)
        public string? ReturnRequestStatus { get; set; } // pending, approved, rejected
        public string? ReturnAdminNote { get; set; }     // Lý do từ chối/ghi chú
    }
}
