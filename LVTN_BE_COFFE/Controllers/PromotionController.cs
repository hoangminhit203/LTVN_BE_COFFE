using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;
        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }
        [HttpGet("apply")]
        public async Task<ActionResult<ResponseResult>?> ApplyPromotion([FromQuery] string code, [FromQuery] decimal orderTotal)
        {
            return await _promotionService.ApplyPromotionAsync(code, orderTotal);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>?> GetPromotionByIdAsync(int id)
        {
            return await _promotionService.GetPromotionByIdAsync(id);
        }
        [HttpGet]
        public async Task<ActionResult<ResponseResult>> GetAllPromotionsAsync()
        {
            return await _promotionService.GetAllPromotionsAsync();
        }
        [HttpPost]
        public async Task<ActionResult<ResponseResult>> CreatePromotionAsync(PromotionCreateVModel model)
        {
            return await _promotionService.CreatePromotionAsync(model);
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseResult>?> UpdatePromotionAsync(int id, PromotionUpdateVModel model)
        {
            return await _promotionService.UpdatePromotionAsync(id, model);
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseResult>> DeletePromotionAsync(int id)
        {
            return await _promotionService.DeletePromotionAsync(id);
        }
    }
}
