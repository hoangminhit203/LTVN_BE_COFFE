using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Http.HttpResults;
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
            _productService = productService;
        }
        
        // GET: api/ProductFilter
        [HttpGet]

        public async Task<ActionResult<PaginationModel<ProductResponse>>> getAll([FromQuery] ProductFilterVModel filterVModel)
        {
            try
            {
                var product = await _productService.GetAllProducts(filterVModel);
                return Ok(product);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách sản phẩm fillter.", detail = e.Message });
            }
        }

        // GET: api/Product/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var product = await _productService.GetProduct(id);
                if (product == null)
                    return NotFound(new { message = "Không tìm thấy sản phẩm." });

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin sản phẩm.", detail = ex.Message });
            }
        }

        // POST: api/Product
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateVModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var product = await _productService.CreateProduct(request);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Không thể tạo sản phẩm.", detail = ex.Message });
            }
        }

        // PUT: api/Product/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateVModel request)
        {
            if (id != request.ProductId)
                return BadRequest(new { message = "ProductId không khớp." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedProduct = await _productService.UpdateProduct(request,id);
                if (updatedProduct == null)
                    return NotFound(new { message = "Không tìm thấy sản phẩm để cập nhật." });

                return Ok(updatedProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật sản phẩm.", detail = ex.Message });
            }
        }

        // DELETE: api/Product/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _productService.DeleteProduct(id);
                if (success == null || success == false)
                    return NotFound(new { message = "Không tìm thấy sản phẩm để xóa." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa sản phẩm.", detail = ex.Message });
            }
        }
    }
}
