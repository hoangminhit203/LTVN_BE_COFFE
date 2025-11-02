//using LVTN_BE_COFFE.Domain.IServices;
//using LVTN_BE_COFFE.Domain.VModel;
//using Microsoft.AspNetCore.Mvc;

//namespace LVTN_BE_COFFE.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ProductVariantController : ControllerBase
//    {
//        private readonly IProductVariantService _variantService;

//        public ProductVariantController(IProductVariantService variantService)
//        {
//            _variantService = variantService;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAll()
//        {
//            try
//            {
//                var variants = await _variantService.GetAllVariants();
//                return Ok(variants);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Lỗi khi lấy danh sách biến thể.", detail = ex.Message });
//            }
//        }

//        [HttpGet("{id:int}")]
//        public async Task<IActionResult> GetById(int id)
//        {
//            try
//            {
//                var variant = await _variantService.GetVariantById(id);
//                if (variant == null)
//                    return NotFound(new { message = "Không tìm thấy biến thể sản phẩm." });

//                return Ok(variant);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Lỗi khi lấy thông tin biến thể.", detail = ex.Message });
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create([FromBody] ProductVariantCreateVModel model)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            try
//            {
//                var created = await _variantService.CreateVariant(model);
//                return Ok(created);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Lỗi khi tạo biến thể.", detail = ex.Message });
//            }
//        }

//        [HttpPut("{id:int}")]
//        public async Task<IActionResult> Update(int id, [FromBody] ProductVariantUpdateVModel model)
//        {
//            if (id != model.ProductVariantId)
//                return BadRequest(new { message = "ProductVariantId không khớp." });

//            try
//            {
//                var updated = await _variantService.UpdateVariant(model);
//                if (updated == null)
//                    return NotFound(new { message = "Không tìm thấy biến thể để cập nhật." });

//                return Ok(updated);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Lỗi khi cập nhật biến thể.", detail = ex.Message });
//            }
//        }

//        [HttpDelete("{id:int}")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            try
//            {
//                var deleted = await _variantService.DeleteVariant(id);
//                if (!deleted)
//                    return NotFound(new { message = "Không tìm thấy biến thể để xóa." });

//                return NoContent();
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Lỗi khi xóa biến thể.", detail = ex.Message });
//            }
//        }
//    }
//}
