using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IBannerService
    {
        Task<ActionResult<List<ResponseResult>>> GetAllBannersAsync();
        Task<ActionResult<ResponseResult>> GetBannerByIdAsync(int id);
        Task<ActionResult<ResponseResult>> CreateBannerAsync(BannerCreateVmodel vmodel);
        Task<ActionResult<ResponseResult>> GetBannerIsActive();
        Task<ActionResult<ResponseResult>> UpdateBannerAsync(int id, BannerUpdateVmodel vmodel);
        Task<ActionResult<ResponseResult>> DeleteBannerAsync(int id);
    }
}
