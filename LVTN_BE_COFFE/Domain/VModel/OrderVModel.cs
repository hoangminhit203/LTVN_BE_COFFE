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
        public string? VoucherCode { get; set; }
    }
    public class OrderUpdateVModel
    {
        [Required]
        public int OrderId { get; set; }

        [StringLength(255)]
        public string? ShippingAddress { get; set; }

        [StringLength(50)]
        public string? ShippingMethod { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }
    }

    public class OrderResponse
    {
        public int Id { get; set; }

        // Thông tin vận chuyển
        public string? ShippingMethod { get; set; }
        public string ShippingAddress { get; set; } = null!;

        public string ReceiverName { get; set; } = null!;
        public string ReceiverPhone { get; set; } = null!;
        public string? ReceiverEmail { get; set; }
        public string Status { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal ShippingFee { get; set; } 

        public decimal DiscountAmount { get; set; }

        public decimal FinalAmount { get; set; }

        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<OrderItemResponse> OrderItems { get; set; } = new();
    }

    // ==========================================
    // 4. MODEL ITEM CON (Chi tiết sản phẩm trong đơn)
    // ==========================================
    //public class OrderItemResponse
    //{
    //    public int Id { get; set; }
    //    public int ProductVariantId { get; set; }
    //    public string ProductName { get; set; } = string.Empty;
    //    public int Quantity { get; set; }
    //    public decimal PriceAtPurchase { get; set; } // Giá tại thời điểm mua
    //    public decimal Subtotal { get; set; }        // Quantity * Price
    //    // Có thể thêm ảnh sản phẩm vào đây để hiển thị lịch sử đẹp hơn
    //    public string? ProductImage { get; set; }
    //}

    // ==========================================
    // 5. MODEL LỌC ĐƠN HÀNG
    // ==========================================
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