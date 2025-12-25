using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlavorNoteController : ControllerBase
    {
        private readonly IFlavorNotesService _flavorNotesService;

        public FlavorNoteController(IFlavorNotesService flavorNotesService)
        {
            _flavorNotesService = flavorNotesService;
        }

        // GET: api/FlavorNote
        [HttpGet]
        public async Task<ActionResult<ResponseResult>> GetAll([FromQuery] FlavorNoteFilterVModel filter)
        {
            return await _flavorNotesService.GetAllFlavorNotes(filter);
        }

        // GET: api/FlavorNote/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>?> GetById(int id)
        {
            return await _flavorNotesService.GetFlavorNotesById(id);
        }

        // POST: api/FlavorNote
        [HttpPost]
        public async Task<ActionResult<ResponseResult>?> Create([FromBody] FlavorNoteCreateVModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseResult("Dữ liệu không hợp lệ"));
            }

            // Gọi Service
            var result = await _flavorNotesService.CreateFlavorNotes(request);

            //  QUAN TRỌNG: Kiểm tra xem Service có trả về một HTTP Status Code cụ thể không (ví dụ: 404 Not Found)
            // Khi bạn return 'new NotFoundObjectResult' bên Service, nó nằm trong 'result.Result'
            if (result.Result != null)
            {
                return result.Result; // Trả về ngay lỗi 404 (hoặc bất kỳ lỗi nào service gửi ra)
            }

            // 🟢 Nếu không có lỗi, xử lý trường hợp thành công (Status 201 Created)
            if (result.Value != null && result.Value.IsSuccess)
            {
                try
                {
                    var flavorNoteData = (dynamic)result.Value.Data;
                    // Trả về 201 kèm Header Location trỏ đến API GetById
                    return CreatedAtAction(nameof(GetById), new { id = flavorNoteData.FlavorNoteId }, result.Value);
                }
                catch
                {
                    // Nếu lỗi khi tạo link (ví dụ không tìm thấy hàm GetById), trả về 201 thuần
                    return StatusCode(201, result.Value);
                }
            }

            // Trường hợp còn lại (ví dụ trả về 200 OK nhưng không vào if trên), trả về nguyên gốc
            return result;
        }

        // PUT: api/FlavorNote/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseResult>?> Update([FromBody] FlavorNoteUpdateVModel request, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseResult("Dữ liệu không hợp lệ"));
            }

            return await _flavorNotesService.UpdateFlavorNotes(id, request);
        }

        // DELETE: api/FlavorNote/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseResult>?> Delete(int id)
        {
            return await _flavorNotesService.DeleteFlavorNotes(id);
        }
    }
}
