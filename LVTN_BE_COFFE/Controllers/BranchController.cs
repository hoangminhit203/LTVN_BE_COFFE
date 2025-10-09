using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : ControllerBase
    {
        private readonly IBranchService _branchService;

        public BranchController(IBranchService branchService)
        {
            _branchService = branchService;
        }

        // POST: api/branch
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BranchVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var branch = await _branchService.CreateBranch(model);
                return CreatedAtAction(nameof(GetById), new { id = branch.BranchId }, branch);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/branch
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var branches = await _branchService.GetAll();
            return Ok(branches);
        }

        // GET: api/branch/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var branch = await _branchService.GetBranch(id);
                if (branch == null)
                    return NotFound(new { message = "Branch not found" });

                return Ok(branch);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // PUT: api/branch/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] BranchVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updated = await _branchService.UpdateBranch(model, id);
                if (updated == null)
                    return NotFound(new { message = "Branch not found" });

                return Ok(updated);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/branch/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _branchService.DeleteBranch(id);
                if (result == null)
                    return NotFound(new { message = "Branch not found" });

                return Ok(new { message = "Branch deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
