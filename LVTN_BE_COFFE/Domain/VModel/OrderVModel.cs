using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    // ==========================================
    // 1. MODEL TẠO ĐƠN HÀNG
    // ==========================================
    public class OrderCreateVModel
    {
        [Required(ErrorMessage = "Tên người nhận là bắt buộc")]
        public string ReceiverName { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string ReceiverPhone { get; set; } = null!;

        public string? ReceiverEmail { get; set; }

        public string? ShippingAddress { get; set; }
        public int? ShippingAddressId { get; set; }

        public string ShippingMethod { get; set; } = "Standard";
        public string? PromotionCode { get; set; }
    }

    public class OrderUpdateVModel
    {
        [Required]
        public string OrderId { get; set; } = null!;

        [StringLength(255)]
        public string? ShippingAddress { get; set; }

        [StringLength(50)]
        public string? ShippingMethod { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }
    }

    public class OrderResponse
    {
        public string Id { get; set; } = null!;
        public int? ProductId { get; set; } // Thêm nếu cần

     
        public string? ShippingMethod { get; set; }
        public string ShippingAddress { get; set; } = null!;

        public string ReceiverName { get; set; } = null!;
        public string ReceiverPhone { get; set; } = null!;
        public string? ReceiverEmail { get; set; }
        public string Status { get; set; } = null!;

        public decimal TotalAmount { get; set; }

        public decimal ShippingFee { get; set; }

        public decimal DiscountAmount { get; set; }
        public string? PromotionCode { get; set; }

        public decimal FinalAmount { get; set; }

        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<OrderItemResponse> OrderItems { get; set; } = new();
        public string? ReturnRequestStatus { get; set; }
        public string? ReturnAdminNote { get; set; }
    }

    // 5. MODEL LỌC ĐƠN HÀNG
    public class OrderFilterVModel
    {
        public string? Status { get; set; }
        public string? Keyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public string? SortBy { get; set; } = "Date";
        public bool IsAscending { get; set; } = false;

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}