using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannerController : ControllerBase
    {
        private readonly IBannerService _bannerService;
        public BannerController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }
        // GET: api/Banner
        [HttpGet]
        public async Task<ActionResult<List<ResponseResult>>> GetAllBannersAsync()
        {
            return await _bannerService.GetAllBannersAsync();
        }
        // GET: api/Banner/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>?> GetById(int id)
        {
            return await _bannerService.GetBannerByIdAsync(id);
        }
        // POST: api/Banner
        [HttpPost]
        public async Task<ActionResult<ResponseResult>?> Create([FromForm] BannerCreateVmodel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseResult("Dữ liệu không hợp lệ"));
            }
            var result = await _bannerService.CreateBannerAsync(request);
            return result;
        }
        // PUT: api/Banner/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseResult>?> Update(int id, [FromForm] BannerUpdateVmodel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponseResult("Dữ liệu không hợp lệ"));
            }
            var result = await _bannerService.UpdateBannerAsync(id, request);
            return result;
        }
        // DELETE: api/Banner/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseResult>> Delete(int id)
        {
            return await _bannerService.DeleteBannerAsync(id);
        }

        [HttpGet]
        [Route("active-banners")]
        public async Task<ActionResult<ResponseResult>> GetBannerIsActive()
        {
            return await _bannerService.GetBannerIsActive();
        }
    }
}
