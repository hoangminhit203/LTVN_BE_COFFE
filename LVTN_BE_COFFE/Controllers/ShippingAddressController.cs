using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel; // Thay bằng namespace đúng của bạn
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [Route("api/shipping-address")] // URL sẽ là: domain/api/shipping-address
    [ApiController]
    [Authorize] // Bắt buộc phải đăng nhập (có Token) mới gọi được API này
    public class ShippingAddressController : ControllerBase
    {
        private readonly IShippingAddressService _shippingAddressService;

        public ShippingAddressController(IShippingAddressService shippingAddressService)
        {
            _shippingAddressService = shippingAddressService;
        }

        /// POST: api/shipping-address
        [HttpPost]
        public async Task<ActionResult<ResponseResult>> Create([FromBody] ShippingAddressCreateVModel request)
        {
            // Gọi service xử lý logic
            var result = await _shippingAddressService.CreateShippingAddress(request);

            // Vì Service đã trả về ActionResult (Ok/BadRequest) nên return thẳng kết quả
            return result!;
        }

        /// PUT: api/shipping-address/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseResult>> Update(int id, [FromBody] ShippingAddressUpdateVModel request)
        {
            // Kiểm tra ID trên URL có khớp với ID trong body không (để an toàn)
            if (id != request.Id)
            {
                return BadRequest(new ResponseResult
                {
                    IsSuccess = false,
                    Message = "ID trên URL không khớp với ID trong dữ liệu gửi lên."
                });
            }

            var result = await _shippingAddressService.UpdateShippingAddress(request, id);
            return result!;
        }


        /// DELETE: api/shipping-address/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseResult>> Delete(int id)
        {
            var result = await _shippingAddressService.DeleteShippingAddress(id);
            return result;
        }
        /// GET: api/shipping-address/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>> GetById(int id)
        {
            var result = await _shippingAddressService.GetShippingAddress(id);
            return result!;
        }

        /// GET: api/shipping-address?keyword=Hcm&pageIndex=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<ResponseResult>> GetAll([FromQuery] ShippingAddressFilterVModel filter)
        {
            // Dùng [FromQuery] để nhận tham số từ URL
            var result = await _shippingAddressService.GetAllShippingAddresses(filter);
            return result;
        }
    }
}