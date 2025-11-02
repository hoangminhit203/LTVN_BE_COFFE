using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<PaginationModel<ProductResponse>>> GetAll([FromQuery] ProductFilterVModel filterVModel)
        {
            try
            {
                var result = await _productService.GetAllProducts(filterVModel);
                if (result == null || !result.IsSuccess)
                    return NotFound(new { message = "Không tìm thấy danh sách sản phẩm." });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Lỗi khi lấy danh sách sản phẩm.", detail = ex.Message });
            }
        }

        // GET: api/Product/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductResponse>> GetById(int id)
        {
            try
            {
                var result = await _productService.GetProduct(id);
                if (result == null || !result.IsSuccess || result.Data == null)
                    return NotFound(new { message = "Không tìm thấy sản phẩm." });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Lỗi khi lấy thông tin sản phẩm.", detail = ex.Message });
            }
        }

        // POST: api/Product
        [HttpPost]
        public async Task<ActionResult<ProductResponse>> Create([FromBody] ProductCreateVModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Dữ liệu đầu vào không hợp lệ.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                var result = await _productService.CreateProduct(request);
                if (result == null || !result.IsSuccess || result.Data == null)
                    return BadRequest(new { message = "Không thể tạo sản phẩm." });

                return CreatedAtAction(nameof(GetById), new { id = result.Data.ProductId }, result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest,
                    new { message = "Lỗi khi tạo sản phẩm.", detail = ex.Message });
            }
        }

        // PUT: api/Product/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ProductResponse>> Update(int id, [FromBody] ProductUpdateVModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Dữ liệu đầu vào không hợp lệ.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            if (id != request.ProductId)
            {
                return BadRequest(new { message = "ProductId không khớp với ID trong URL." });
            }

            try
            {
                var result = await _productService.UpdateProduct(request, id);
                if (result == null || !result.IsSuccess || result.Data == null)
                    return NotFound(new { message = "Không tìm thấy sản phẩm để cập nhật." });

                return Ok(result.Data);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Lỗi khi cập nhật sản phẩm.", detail = ex.Message });
            }
        }

        // DELETE: api/Product/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _productService.DeleteProduct(id);
                if (result == null || !result.IsSuccess || result.Data == false)
                    return NotFound(new { message = "Không tìm thấy sản phẩm để xóa." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Lỗi khi xóa sản phẩm.", detail = ex.Message });
            }
        }
    }
}