using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/category
        [HttpGet]
        public async Task<ActionResult<ResponseResult>> GetAll([FromQuery] CategoryFilterVModel filter)
        {
            return await _categoryService.GetAllCategories(filter);
        }

        // GET: api/category/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>?> GetById(int id)
        {
            return await _categoryService.GetCategory(id);
        }

        // POST: api/category
        [HttpPost]
        public async Task<ActionResult<ResponseResult>?> Create([FromBody] CategoryCreateVModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseResult("Invalid model state"));

            return await _categoryService.CreateCategory(request);
        }

        // PUT: api/category/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseResult>?> Update(int id, [FromBody] CategoryUpdateVModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponseResult("Invalid model state"));

            return await _categoryService.UpdateCategory(request, id);
        }

        // DELETE: api/category/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseResult>> Delete(int id)
        {
            return await _categoryService.DeleteCategory(id);
        }
        [HttpGet("{id}/products")]
        public async Task<ActionResult<ResponseResult>> GetProductsByCategory(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            return await _categoryService.GetProductsByCategoryId(id, pageNumber, pageSize);
        }
    }
}
