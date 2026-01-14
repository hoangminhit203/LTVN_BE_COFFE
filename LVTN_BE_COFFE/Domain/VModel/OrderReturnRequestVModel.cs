using System.ComponentModel.DataAnnotations;

namespace LVTN_BE_COFFE.Domain.VModel
{
    public class ReturnOrderInputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập lý do")]
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
}
