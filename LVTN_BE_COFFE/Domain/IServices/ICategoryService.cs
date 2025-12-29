using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface ICategoryService
    {
        Task<ActionResult<ResponseResult>?> CreateCategory(CategoryCreateVModel request);
        Task<ActionResult<ResponseResult>?> UpdateCategory(CategoryUpdateVModel request, int id);
        Task<ActionResult<ResponseResult>> DeleteCategory(int id);
        Task<ActionResult<ResponseResult>?> GetCategory(int id);
        Task<ActionResult<ResponseResult>> GetAllCategories(CategoryFilterVModel filter);
        Task<ActionResult<ResponseResult>> GetProductsByCategoryId(int categoryId, int pageNumber = 1, int pageSize = 10);
    }
}
