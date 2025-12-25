using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrewingMethodsController : ControllerBase
    {
        private readonly IBrewingMethodsService _brewingMethodsService;

        public BrewingMethodsController(IBrewingMethodsService brewingMethodsService)
        {
            _brewingMethodsService = brewingMethodsService;
        }

        // GET: api/brewingmethods
        [HttpGet]
        public async Task<ActionResult<ResponseResult>> GetAll([FromQuery] BrewingMethodsFilterVModel filter)
        {
            return await _brewingMethodsService.GetAllBrewingMethods(filter);
        }

        // GET: api/brewingmethods/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>?> GetById(int id)
        {
            return await _brewingMethodsService.GetBrewingMethodsById(id);
        }

        // POST: api/brewingmethods
        [HttpPost]
        public async Task<ActionResult<ResponseResult>?> Create([FromBody] BrewingMethodsCreateVModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseResult("Invalid model state"));

            return await _brewingMethodsService.CreateBrewingMethods(request);
        }

        // PUT: api/brewingmethods/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseResult>?> Update(int id, [FromBody] BrewingMethodsUpdateVModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseResult("Invalid model state"));

            return await _brewingMethodsService.UpdateBrewingMethods(id, request);
        }

        // DELETE: api/brewingmethods/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseResult>> Delete(int id)
        {
            return await _brewingMethodsService.DeleteBrewingMethods(id);
        }
    }
}