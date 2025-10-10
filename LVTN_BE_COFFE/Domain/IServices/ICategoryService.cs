using LVTN_BE_COFFE.Domain.VModel;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface ICategoryService
    {
        Task<CategoryResponse?> CreateCategory(CategoryCreateVModel request);
        Task<CategoryResponse?> UpdateCategory(CategoryUpdateVModel request);
        Task<bool> DeleteCategory(int categoryId);
        Task<List<CategoryResponse>> GetAllCategories();
        Task<CategoryResponse?> GetCategoryById(int categoryId);
        Task<CategoryResponse?> FindByName(string name);
    }
}
