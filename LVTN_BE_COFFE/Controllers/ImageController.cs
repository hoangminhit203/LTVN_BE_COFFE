using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Services;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    /// <summary>
    /// Controller quản lý các thao tác liên quan đến hình ảnh sản phẩm (Upload, Cập nhật, Xóa, Lấy danh sách).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")] // Route mặc định: api/Image
    public class ImageController : ControllerBase
    {
        private readonly IProductImageService _imageService;
        private readonly ILogger<ImageController> _logger;

        // Constructor: Sử dụng Dependency Injection để tiêm Service và Logger vào Controller
        public ImageController(IProductImageService imageService, ILogger<ImageController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        /// <summary>
        /// API Thêm mới ảnh sản phẩm (Upload file).
        /// </summary>
        /// <param name="model">Chứa file ảnh và thông tin liên quan (ProductId, VariantId, etc.)</param>
        [HttpPost("product-images")]
        [Consumes("multipart/form-data")] // Quan trọng: Bắt buộc phải có để nhận file từ Client
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductImageResponse>> AddProductImage([FromForm] ProductImageCreateVModel model)
        {
            try
            {
                // Gọi Service để xử lý lưu file và lưu thông tin vào DB
                var response = await _imageService.AddAsync(model);

                // Trả về 200 OK kèm dữ liệu ảnh vừa tạo
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                // Log lỗi và trả về 400 Bad Request nếu có lỗi logic (ví dụ: file không hợp lệ)
                _logger.LogError(ex, "Lỗi nghiệp vụ khi thêm ảnh.");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log lỗi và trả về 500 Internal Server Error cho các lỗi không mong muốn
                _logger.LogError(ex, "Lỗi server khi thêm ảnh.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi server khi thêm ảnh.");
            }
        }

        /// <summary>
        /// API Cập nhật thông tin ảnh (Ví dụ: đổi ảnh khác, đổi trạng thái active).
        /// </summary>
        /// <param name="id">ID của ảnh cần sửa</param>
        /// <param name="model">Dữ liệu cần cập nhật</param>
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")] // Vẫn cần multipart nếu có gửi kèm file mới để thay thế
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductImageResponse>> UpdateProductImage(int id, [FromForm] ProductImageUpdateVModel model)
        {
            // Kiểm tra tính nhất quán: ID trên URL phải khớp với ID trong body gửi lên
            if (id != model.Id)
            {
                return BadRequest(new { Message = "ID trong URL và Request Body không khớp." });
            }

            try
            {
                var response = await _imageService.UpdateAsync(model);
                return Ok(response);
            }
            catch (KeyNotFoundException ex)
            {
                // Trả về 404 Not Found nếu không tìm thấy ảnh với ID cung cấp
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi server khi cập nhật ảnh.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi server khi cập nhật ảnh.");
            }
        }

        /// <summary>
        /// API Xóa ảnh theo ID.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProductImage(int id)
        {
            var result = await _imageService.DeleteAsync(id);
            if (!result)
            {
                return NotFound(new { Message = $"Không tìm thấy ảnh với ID: {id}" });
            }

            // Trả về 204 No Content: Chuẩn RESTful khi xóa thành công và không cần trả về dữ liệu gì thêm
            return NoContent();
        }

        /// <summary>
        /// API Lấy chi tiết một ảnh theo ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductImageResponse>> GetProductImageById(int id)
        {
            var response = await _imageService.GetByIdAsync(id);
            if (response == null)
            {
                return NotFound(new { Message = $"Không tìm thấy ảnh với ID: {id}" });
            }
            return Ok(response);
        }

        /// <summary>
        /// API Lấy danh sách ảnh (có hỗ trợ phân trang và lọc).
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationModel<ProductImageResponse>>> GetAllProductImages([FromQuery] ProductImageFilterVModel filter)
        {
            // [FromQuery]: Lấy tham số từ URL (ví dụ: api/Image?page=1&pageSize=10)
            var response = await _imageService.GetAllAsync(filter);
            return Ok(response);
        }

        /// <summary>
        /// API Lấy tất cả ảnh thuộc về một Biến thể (Variant) cụ thể.
        /// Ví dụ: Lấy tất cả ảnh của gói cà phê 500g.
        /// </summary>
        [HttpGet("by-variant/{variantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductImageResponse>>> GetImagesByVariantId(int variantId)
        {
            var response = await _imageService.GetByProductVariantIdAsync(variantId);
            return Ok(response);
        }

        /// <summary>
        /// API Lấy tất cả ảnh thuộc về một Sản phẩm (Product) cha.
        /// Ví dụ: Lấy tất cả ảnh của dòng sản phẩm "Cà phê Arabica".
        /// </summary>
        [HttpGet("by-product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductImageResponse>>> GetImagesByProductId(int productId)
        {
            var response = await _imageService.GetByProductIdAsync(productId);
            return Ok(response);
        }
    }
}