using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class OrderCreateVModel
    {
        /// <summary>
        /// Nếu user muốn dùng địa chỉ mặc định, có thể để null
        /// Nếu user nhập địa chỉ mới, truyền vào đây
        /// </summary>
        [StringLength(255)]
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Id của địa chỉ đã lưu (ShippingAddress entity), nếu user muốn dùng địa chỉ cũ
        /// </summary>
        public int? ShippingAddressId { get; set; }

        [StringLength(50)]
        public string ShippingMethod { get; set; } = null!;

        public string? VoucherCode { get; set; }

        // Nếu bạn muốn cho phép order không qua cart
        public List<OrderItemCreateVModel>? OrderItems { get; set; }
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
        public string? ShippingMethod { get; set; }

        public string ShippingAddress { get; set; } = null!;
        public string Status { get; set; }          // Pending, Completed...
        public string? VoucherCode { get; set; }

        public int? PromotionId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public int ItemCount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<OrderItemResponse> OrderItems { get; set; } = new();
    }


    public class OrderFilterVModel
    {
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
