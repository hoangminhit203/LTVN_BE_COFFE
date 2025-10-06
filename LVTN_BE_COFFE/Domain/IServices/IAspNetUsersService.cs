using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IAspNetUsersService
    {
        Task<ActionResult<PaginationModel<AspNetUsersGetVModel>>> GetAll(AspNetUsersFilterParams parameters);
        Task<ActionResult<AspNetUsersGetVModel>?> GetById(string id);
        Task<ActionResult<AspNetUsersGetVModel>?> Create(AspNetUsersCreateVModel model);
        Task<int> Update(AspNetUsersUpdateVModel model);
        Task<int> Remove(string id);
    }
}
