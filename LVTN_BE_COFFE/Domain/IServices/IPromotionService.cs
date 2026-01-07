using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

public interface IPromotionService
{
    // Admin
    Task<ActionResult<ResponseResult>?> GetPromotionByIdAsync(int id);
    Task<ActionResult<ResponseResult>> GetAllPromotionsAsync();
    Task<ActionResult<ResponseResult>> CreatePromotionAsync(PromotionCreateVModel model);
    Task<ActionResult<ResponseResult>?> UpdatePromotionAsync(int id, PromotionUpdateVModel model);
    Task<ActionResult<ResponseResult>> DeletePromotionAsync(int id);
    Task<ActionResult<ResponseResult>?> ApplyPromotionAsync(string code, decimal orderTotal);
}
