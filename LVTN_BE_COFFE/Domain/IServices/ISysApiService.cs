using LVTN_BE_COFFE.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Services.Services
{
    public interface ISysApiService
    {
        Task<ActionResult<PaginationModel<SysApiGetVModel>>> GetAll(SysApiFilterParams parameters);
        Task<ActionResult<SysApiGetVModel>?> GetById(long id);
        Task<ActionResult<SysApiGetVModel>> Create(SysApiCreateVModel model);
        Task<int> Update(SysApiUpdateVModel model);
        Task<int> Remove(long id);
    }
}
