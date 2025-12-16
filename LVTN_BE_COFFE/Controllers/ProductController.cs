using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<ResponseResult>> GetAll([FromQuery] ProductFilterVModel filter)
        {
            return await _productService.GetAllProducts(filter);
        }

        // GET: api/Product/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>?> GetById(int id)
        {
            return await _productService.GetProduct(id);
        }

        //// ✅ GET: api/Product/find-by-name?name=Capuchino
        //[HttpGet("find-by-name")]
        //public async Task<ActionResult<ResponseResult>?> FindByName([FromQuery] string name)
        //{
        //    var product = await _productService.FindByName(name);
        //    return product;
        //}

        // POST: api/Product
        [HttpPost]
        public async Task<ActionResult<ResponseResult>?> Create([FromBody] ProductCreateVModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseResult("Dữ liệu không hợp lệ"));
            }

            var result = await _productService.CreateProduct(request);

            // Nếu tạo thành công, trả về status 201 Created
            if (result?.Value != null && result.Value.IsSuccess)
            {
                // Extract ProductId from the response data for CreatedAtAction
                try
                {
                    var productData = (dynamic)result.Value.Data;
                    return CreatedAtAction(nameof(GetById), new { id = productData.ProductId }, result.Value);
                }
                catch
                {
                    // Fallback nếu không extract được ProductId
                    return StatusCode(201, result.Value);
                }
            }

            return result;
        }

        // PUT: api/Product/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseResult>?> Update([FromBody] ProductUpdateVModel request, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseResult("Dữ liệu không hợp lệ"));
            }

            return await _productService.UpdateProduct(request, id);
        }

        // DELETE: api/Product/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseResult>> Delete(int id)
        {
            return await _productService.DeleteProduct(id);
        }
    }
}