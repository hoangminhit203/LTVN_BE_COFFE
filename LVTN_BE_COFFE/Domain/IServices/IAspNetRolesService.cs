using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IAspNetRolesService
    {
        Task<ActionResult<PaginationModel<AspNetRolesGetVModel>>> GetAll(AspNetRolesFilterParams parameters);
        Task<ActionResult<AspNetRolesGetVModel>?> GetById(string id);
        Task<ActionResult<AspNetRolesGetVModel>?> Create(AspNetRolesCreateVModel model);
        Task<int> Update(AspNetRolesUpdateVModel model);
        Task<int> Remove(string id);
    }
}
