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
        public async Task<ActionResult<PaginationModel<ProductResponse>>> GetAll([FromQuery] ProductFilterVModel filter)
        {
            return await _productService.GetAllProducts(filter);
        }

        // GET: api/Product/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponse>?> GetById(int id)
        {
            var product = await _productService.GetProduct(id);
            if (product?.Value == null)
                return NotFound("Product not found.");
            return product;
        }

        //// ✅ GET: api/Product/find-by-name?name=Capuchino
        //[HttpGet("find-by-name")]
        //public async Task<ActionResult<ProductResponse>?> FindByName([FromQuery] string name)
        //{
        //    var product = await _productService.FindByName(name);
        //    if (product?.Value == null)
        //        return NotFound("Product not found.");
        //    return product;
        //}

        // POST: api/Product
        [HttpPost]
        public async Task<ActionResult<ProductResponse>?> Create([FromBody] ProductCreateVModel request)
        {
            var created = await _productService.CreateProduct(request);
            if (created?.Value == null)
                return BadRequest("Create failed.");
            return CreatedAtAction(nameof(GetById), new { id = created.Value.ProductId }, created);
        }

        // PUT: api/Product/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ProductResponse>?> Update([FromBody] ProductUpdateVModel request, int id)
        {
            var updated = await _productService.UpdateProduct(request, id);
            if (updated?.Value == null)
                return NotFound("Product not found.");
            return Ok(updated);
        }

        // DELETE: api/Product/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            var result = await _productService.DeleteProduct(id);
            if (result?.Value == false)
                return NotFound("Product not found.");
            return Ok(true);
        }
    }
}
