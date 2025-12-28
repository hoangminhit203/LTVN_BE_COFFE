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
        /// <summary>
        /// Cách 1: Nhập địa chỉ text mới (Ví dụ: "123 Đường A, Quận B...")
        /// Hệ thống sẽ tự tạo địa chỉ mới trong bảng ShippingAddress
        /// </summary>
        [StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự.")]
        public string? ShippingAddress { get; set; }

        /// <summary>
        /// Cách 2: Chọn ID của địa chỉ đã lưu sẵn (Ưu tiên dùng cái này)
        /// </summary>
        public int? ShippingAddressId { get; set; }

        /// <summary>
        /// Phương thức vận chuyển (Standard, Express...)
        /// </summary>
        [StringLength(50)]
        public string ShippingMethod { get; set; } = "Standard"; // Mặc định là Standard

        /// <summary>
        /// Mã giảm giá (nếu có)
        /// </summary>
        [StringLength(50)]
        public string? VoucherCode { get; set; }

        /// <summary>
        /// Ghi chú của khách hàng cho đơn hàng (Ví dụ: Giao giờ hành chính)
        /// </summary>
        [StringLength(500)]
        public string? Note { get; set; }

        // [Option] Nếu bạn muốn hỗ trợ "Mua ngay" mà không cần thêm vào giỏ hàng trước
        // Nếu dùng logic CartService như mình viết thì field này có thể để null
        public List<OrderItemCreateVModel>? OrderItems { get; set; }
    }

    // ==========================================
    // 2. MODEL CẬP NHẬT ĐƠN HÀNG (Dành cho Admin/Shipper)
    // ==========================================
    public class OrderUpdateVModel
    {
        [Required]
        public int OrderId { get; set; }

        // Thường khi đã đặt hàng, ít khi cho sửa địa chỉ để tránh sai phí ship.
        // Chỉ nên cho sửa khi trạng thái là "Pending"
        [StringLength(255)]
        public string? ShippingAddress { get; set; }

        [StringLength(50)]
        public string? ShippingMethod { get; set; }

        // Quan trọng: Dùng để chuyển trạng thái (Pending -> Shipping -> Completed -> Cancelled)
        [StringLength(20)]
        public string? Status { get; set; }
    }

    // ==========================================
    // 3. MODEL TRẢ VỀ (RESPONSE) - QUAN TRỌNG
    // ==========================================
    public class OrderResponse
    {
        public int Id { get; set; }

        // Thông tin vận chuyển
        public string? ShippingMethod { get; set; }
        public string ShippingAddress { get; set; } = null!; // Trả về text địa chỉ đầy đủ
        public string Status { get; set; }                   // Pending, Completed...

        // Thông tin Voucher
        public string? VoucherCode { get; set; }
        public int? PromotionId { get; set; }

        // --- PHẦN TÀI CHÍNH (Cần bổ sung để khớp với Entity mới) ---

        public decimal TotalAmount { get; set; }    // Tổng tiền hàng (Chưa cộng ship, chưa trừ voucher)

        public decimal ShippingFee { get; set; }    // Phí ship (Mới thêm)

        public decimal DiscountAmount { get; set; } // Số tiền được giảm (Mới thêm)

        public decimal FinalAmount { get; set; }    // Số tiền khách phải trả cuối cùng

        // Thông tin phụ
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Chi tiết sản phẩm
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
        public string? Status { get; set; }      // Lọc theo trạng thái
        public string? Keyword { get; set; }     // Tìm theo Mã đơn hoặc Tên người nhận
        public DateTime? FromDate { get; set; }  // Từ ngày
        public DateTime? ToDate { get; set; }    // Đến ngày

        // Sắp xếp
        public string? SortBy { get; set; } = "Date"; // Date, Amount
        public bool IsAscending { get; set; } = false; // Mặc định mới nhất lên đầu

        // Phân trang
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}