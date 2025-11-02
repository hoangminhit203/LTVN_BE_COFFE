//using LVTN_BE_COFFE.Domain.IServices;
//using LVTN_BE_COFFE.Domain.VModel;
//using Microsoft.AspNetCore.Mvc;

//namespace LVTN_BE_COFFE.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ToppingController : ControllerBase
//    {
//        private readonly IToppingService _toppingService;

//        public ToppingController(IToppingService toppingService)
//        {
//            _toppingService = toppingService;
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAll()
//        {
//            try
//            {
//                var toppings = await _toppingService.GetAllToppings();
//                return Ok(toppings);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Lỗi khi lấy danh sách topping", detail = ex.Message });
//            }
//        }

//        [HttpGet("{id:int}")]
//        public async Task<IActionResult> Get(int id)
//        {
//            try
//            {
//                var topping = await _toppingService.GetTopping(id);
//                if (topping == null)
//                    return NotFound(new { message = "Không tìm thấy topping" });

//                return Ok(topping);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Lỗi khi lấy topping", detail = ex.Message });
//            }
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create([FromBody] ToppingCreateVModel model)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            try
//            {
//                var topping = await _toppingService.CreateTopping(model);
//                return Ok(topping);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Lỗi khi tạo topping", detail = ex.Message });
//            }
//        }

//        [HttpPut("{id:int}")]
//        public async Task<IActionResult> Update(int id, [FromBody] ToppingUpdateVModel model)
//        {
//            if (id != model.ToppingId)
//                return BadRequest(new { message = "ID không khớp" });

//            try
//            {
//                var topping = await _toppingService.UpdateTopping(model, id);
//                return Ok(topping);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Lỗi khi cập nhật topping", detail = ex.Message });
//            }
//        }

//        [HttpDelete("{id:int}")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            try
//            {
//                var success = await _toppingService.DeleteTopping(id);
//                return success ? NoContent() : NotFound();
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "Lỗi khi xóa topping", detail = ex.Message });
//            }
//        }
//    }
//}
