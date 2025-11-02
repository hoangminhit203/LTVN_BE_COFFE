using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Domain.Model;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface ICategoryService
    {
        Task<ActionResult<CategoryResponse>?> CreateCategory(CategoryCreateVModel request);
        Task<ActionResult<CategoryResponse>?> UpdateCategory(CategoryUpdateVModel request, int id);
        Task<ActionResult<bool>> DeleteCategory(int id);
        Task<ActionResult<CategoryResponse>?> GetCategory(int id);
        Task<ActionResult<PaginationModel<CategoryResponse>>> GetAllCategories(CategoryFilterVModel filter);
    }
}
