using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Services.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [Route(Strings.BaseRoute)]
    [ApiController]
    public class SysApiController : ControllerBase
    {
        private readonly ISysApiService _apiService;

        public SysApiController(ISysApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationModel<SysApiGetVModel>>> GetAll([FromQuery] SysApiFilterParams parameters)
        {
            var result = await _apiService.GetAll(parameters);
            return result;
        }

        [HttpGet(Strings.IdRoute)]
        public async Task<ActionResult<SysApiGetVModel>> GetById(long id)
        {
            var result = await _apiService.GetById(id);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpPost]
        public async Task<ActionResult<SysApiGetVModel>> Create(SysApiCreateVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            var result = await _apiService.Create(model);
            return result;
        }

        [HttpPut(Strings.IdRoute)]
        public async Task<ActionResult<SysApiGetVModel>> Update(long id, SysApiUpdateVModel model)
        {
            if (id <= 0 || id != model.Id)
            {
                return BadRequest();
            }
            var result = await _apiService.Update(model);
            if (result == Numbers.FindResponse.NotFound)
            {
                return NotFound();
            }
            else
            {
                return await GetById(id);
            }
        }

        [HttpDelete(Strings.IdRoute)]
        public async Task<IActionResult> Remove(long id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var result = await _apiService.Remove(id);
            if (result == Numbers.FindResponse.NotFound)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
