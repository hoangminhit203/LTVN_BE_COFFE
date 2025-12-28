using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    // ==========================================
    // Model dùng cho chức năng TẠO MỚI (Create)
    // ==========================================
    public class ShippingAddressCreateVModel
    {
        // Địa chỉ cụ thể (Số nhà, đường, phường, xã...)
        [Required(ErrorMessage = "Địa chỉ đầy đủ là bắt buộc.")]
        [MaxLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
        public string FullAddress { get; set; } = string.Empty;

        // Tên người nhận hàng
        // LƯU Ý: Nên để Required vì shipper cần biết tên người nhận
        [Required(ErrorMessage = "Tên người nhận là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên người nhận không được vượt quá 100 ký tự.")]
        public string ReceiverName { get; set; } = string.Empty;

        // Số điện thoại liên lạc
        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [MaxLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
        [Phone(ErrorMessage = "Định dạng số điện thoại không hợp lệ.")]
        // Mẹo: Nếu muốn validate số VN chuẩn hơn, có thể dùng [RegularExpression]
        public string Phone { get; set; } = string.Empty;

        // Đặt làm địa chỉ mặc định hay không
        public bool IsDefault { get; set; } = false;
    }

    // ==========================================
    // Model dùng cho chức năng CẬP NHẬT (Update)
    // ==========================================
    public class ShippingAddressUpdateVModel
    {
        // ID của địa chỉ cần sửa (Bắt buộc phải có để biết sửa cái nào)
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Địa chỉ đầy đủ là bắt buộc.")]
        [MaxLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự.")]
        public string FullAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên người nhận là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên người nhận không được vượt quá 100 ký tự.")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
        [MaxLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string Phone { get; set; } = string.Empty;

        // Cho phép user thay đổi trạng thái mặc định khi update
        public bool IsDefault { get; set; }
    }

    // ==========================================
    // Model dùng để TRẢ VỀ dữ liệu (Response)
    // ==========================================
    public class ShippingAddressResponseVModel
    {
        public int Id { get; set; }

        // Trả về UserId để biết địa chỉ này của ai (nếu cần thiết cho Admin)
        public string UserId { get; set; } = string.Empty;

        public string FullAddress { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty; // Đã bỏ dấu ? vì output nên có dữ liệu
        public string Phone { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    // ==========================================
    // Model bổ sung: Dùng cho chức năng LỌC (Filter)
    // (Vì trong Interface Service bạn có hàm GetAllShippingAddresses cần cái này)
    // ==========================================
    public class ShippingAddressFilterVModel
    {
        // Tìm kiếm theo từ khóa (tên, sđt, địa chỉ)
        public string? Keyword { get; set; }

        // Lọc riêng địa chỉ của 1 user cụ thể (dành cho Admin xem)
        public string? UserId { get; set; }

        // Phân trang (nếu cần)
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}