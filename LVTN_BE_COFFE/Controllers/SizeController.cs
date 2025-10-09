using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SizeController : ControllerBase
    {
        private readonly ISizeService _sizeService;

        public SizeController(ISizeService sizeService)
        {
            _sizeService = sizeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var sizes = await _sizeService.GetAllSizes();
                return Ok(sizes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách size.", detail = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var size = await _sizeService.GetSize(id);
                if (size == null)
                    return NotFound(new { message = "Không tìm thấy size." });

                return Ok(size);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin size.", detail = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SizeCreateVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var size = await _sizeService.CreateSize(model);
                return CreatedAtAction(nameof(GetById), new { id = size.SizeId }, size);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Không thể tạo size.", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SizeUpdateVModel model)
        {
            if (id != model.SizeId)
                return BadRequest(new { message = "SizeId không khớp." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _sizeService.UpdateSize(model);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(new { message = ex.Message });

                return BadRequest(new { message = "Không thể cập nhật size.", detail = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _sizeService.DeleteSize(id);
                if (!result)
                    return NotFound(new { message = "Không tìm thấy size cần xóa." });

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Không thể xóa size.", detail = ex.Message });
            }
        }
    }
}
