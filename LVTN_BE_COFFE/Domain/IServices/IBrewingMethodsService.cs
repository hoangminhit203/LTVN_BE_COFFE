using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IBrewingMethodsService
    {
        Task<ActionResult<ResponseResult>?> CreateBrewingMethods(BrewingMethodsCreateVModel request);
        Task<ActionResult<ResponseResult>?> GetBrewingMethodsById(int id);
        Task<ActionResult<ResponseResult>?> UpdateBrewingMethods(int id, BrewingMethodsUpdateVModel request);
        Task<ActionResult<ResponseResult>?> DeleteBrewingMethods(int id);
        Task<ActionResult<ResponseResult>> GetAllBrewingMethods(BrewingMethodsFilterVModel filter);
    }
}
