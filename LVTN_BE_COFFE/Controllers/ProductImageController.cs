using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageService _productImageService;

        public ProductImageController(IProductImageService productImageService)
        {
            _productImageService = productImageService;
        }

        /// <summary>
        /// Lấy danh sách ảnh có phân trang & filter
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PaginationModel<ProductImageResponse>>> GetAll([FromQuery] ProductImageFilterVModel filter)
        {
            var result = await _productImageService.GetAllAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết 1 ảnh theo Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductImageResponse>> GetById(int id)
        {
            var result = await _productImageService.GetByIdAsync(id);
            if (result == null)
                return NotFound(new { Message = "Không tìm thấy hình ảnh." });

            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách ảnh theo ProductId
        /// </summary>
        [HttpGet("product/{productId}")]
        public async Task<ActionResult<List<ProductImageResponse>>> GetByProductId(int productId)
        {
            var result = await _productImageService.GetByProductIdAsync(productId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách ảnh theo ProductVariantId
        /// </summary>
        [HttpGet("variant/{variantId}")]
        public async Task<ActionResult<List<ProductImageResponse>>> GetByVariantId(int variantId)
        {
            var result = await _productImageService.GetByProductVariantIdAsync(variantId);
            return Ok(result);
        }

        /// <summary>
        /// Upload ảnh mới (lưu Cloudinary + DB)
        /// </summary>
        [HttpPost("upload")]
        public async Task<ActionResult<ProductImageResponse>> Upload([FromForm] ProductImageCreateVModel model)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest(new { Message = "Vui lòng chọn file ảnh để upload." });

            try
            {
                var result = await _productImageService.AddAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = result.ImageId }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi upload ảnh.", Error = ex.Message });
            }
        }


        /// <summary>
        /// Thêm mới ảnh
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductImageResponse>> Create([FromBody] ProductImageCreateVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _productImageService.AddAsync(model);
                return CreatedAtAction(nameof(GetById), new { id = result.ImageId }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi tạo mới hình ảnh.", Error = ex.Message });
            }
        }

        /// <summary>
        /// Cập nhật ảnh
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductImageResponse>> Update(int id, [FromForm] ProductImageUpdateVModel model)
        {
            if (id != model.ImageId)
                return BadRequest(new { Message = "ImageId không khớp." });

            try
            {
                var result = await _productImageService.UpdateAsync(model);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi cập nhật hình ảnh.", Error = ex.Message });
            }
        }

        /// <summary>
        /// Xóa ảnh
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productImageService.DeleteAsync(id);
            if (!success)
                return NotFound(new { Message = "Không tìm thấy hình ảnh để xóa." });

            return NoContent();
        }
    }
}
