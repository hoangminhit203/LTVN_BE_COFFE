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
    public class AspNetUsersController : ControllerBase
    {
        private readonly IAspNetUsersService _usersService;

        public AspNetUsersController(IAspNetUsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationModel<AspNetUsersGetVModel>>> GetAll([FromQuery] AspNetUsersFilterParams parameters)
        {
            var result = await _usersService.GetAll(parameters);
            return result;
        }

        [HttpGet(Strings.IdRoute)]
        public async Task<ActionResult<AspNetUsersGetVModel>> GetById(string id)
        {
            var result = await _usersService.GetById(id);

            if (result == null)
            {
                return NotFound();
            }

            return result;
        }

        [HttpPost]
        public async Task<ActionResult<AspNetUsersGetVModel>?> Create(AspNetUsersCreateVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            var result = await _usersService.Create(model);
            return result;
        }

        [HttpPut(Strings.IdRoute)]
        public async Task<ActionResult<AspNetUsersGetVModel>> Update(string id, AspNetUsersUpdateVModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }
            var result = await _usersService.Update(model);
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

            var result = await _usersService.Remove(id);
            if (result == Numbers.FindResponse.NotFound)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
