using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [Route(Strings.BaseRoute)]
    [ApiController]
    public class AspNetRolesController : ControllerBase
    {
        private readonly IAspNetRolesService _rolesService;

        public AspNetRolesController(IAspNetRolesService rolesService)
        {
            _rolesService = rolesService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationModel<AspNetRolesGetVModel>>> GetAll([FromQuery] AspNetRolesFilterParams parameters)
        {
            var result = await _rolesService.GetAll(parameters);
            return result;
        }

        [HttpGet(Strings.IdRoute)]
        public async Task<ActionResult<AspNetRolesGetVModel>> GetById(string id)
        {
            var result = await _rolesService.GetById(id);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseResult>> Create(AspNetRolesCreateVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            var result = await _rolesService.Create(model);
            return result;
        }

        [HttpPut(Strings.IdRoute)]
        public async Task<ActionResult<AspNetRolesGetVModel>> Update(string id, AspNetRolesUpdateVModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }
            var result = await _rolesService.Update(model);
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
        public async Task<IActionResult> Remove(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var result = await _rolesService.Remove(id);
            if (result == Numbers.FindResponse.NotFound)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
