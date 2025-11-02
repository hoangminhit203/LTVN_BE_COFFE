using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        // ✅ GET: api/Order
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetAll([FromQuery] OrderFilterVModel filter)
        {
            return await _orderService.GetOrdersAsync(filter);
        }

        // ✅ GET: api/Order/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponse>> GetById(int id)
        {
            return await _orderService.GetOrderByIdAsync(id);
        }

        // ✅ POST: api/Order
        [HttpPost]
        public async Task<ActionResult<OrderResponse>> Create([FromBody] OrderCreateVModel request)
        {
            return await _orderService.CreateOrderAsync(request);
        }

        // ✅ PUT: api/Order
        [HttpPut]
        public async Task<ActionResult<OrderResponse>> Update([FromBody] OrderUpdateVModel request)
        {
            return await _orderService.UpdateOrderAsync(request);
        }

        // ✅ DELETE: api/Order/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(int id)
        {
            return await _orderService.DeleteOrderAsync(id);
        }
    }
}
