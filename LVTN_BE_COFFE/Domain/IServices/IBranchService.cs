using LVTN_BE_COFFE.Domain.VModel;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IBranchService
    {
        Task<BranchResponse> CreateBranch(BranchVModel model);
        Task<List<BranchResponse>> GetAll();

        Task<BranchResponse> GetBranch(int branchId);
        Task<BranchResponse> UpdateBranch(BranchVModel model, int branchId);
        Task<BranchResponse> DeleteBranch(int branchId);
    }
}
