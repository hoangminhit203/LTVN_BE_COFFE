using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Services;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{

    [Controller]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IProductImageService _imageService;
        private readonly ILogger<ImageController> _logger;

        public ImageController(IProductImageService imageService, ILogger<ImageController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductImageResponse>> AddProductImage([FromForm] ProductImageCreateVModel model)
        {
            try
            {
                var response = await _imageService.AddAsync(model);

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Lỗi nghiệp vụ khi thêm ảnh.");
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi server khi thêm ảnh.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi server khi thêm ảnh.");
            }
        }
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductImageResponse>> UpdateProductImage(int id, [FromForm] ProductImageUpdateVModel model)
        {
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
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi server khi cập nhật ảnh.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Lỗi server khi cập nhật ảnh.");
            }
        }
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
            return NoContent(); // 204 No Content cho thao tác xóa thành công
        }

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
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginationModel<ProductImageResponse>>> GetAllProductImages([FromQuery] ProductImageFilterVModel filter)
        {
            var response = await _imageService.GetAllAsync(filter);
            return Ok(response);
        }
        [HttpGet("by-variant/{variantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductImageResponse>>> GetImagesByVariantId(int variantId)
        {
            var response = await _imageService.GetByProductVariantIdAsync(variantId);
            return Ok(response);
        }
        [HttpGet("by-product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ProductImageResponse>>> GetImagesByProductId(int productId)
        {
            var response = await _imageService.GetByProductIdAsync(productId);
            return Ok(response);
        }
    }
}
