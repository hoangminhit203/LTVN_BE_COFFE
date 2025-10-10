using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class BranchService : IBranchService
    {
        private readonly AppDbContext _context;

        public BranchService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<BranchResponse> CreateBranch(BranchVModel model)
        {
            var branch = new Branch
            {
                Name = model.Name,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
            };
            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();
            return new BranchResponse
            {
                BranchId=branch.BranchId,
                Name = branch.Name,
                Address = branch.Address,
                PhoneNumber = branch.PhoneNumber,
                CreatedAt = branch.CreatedAt,
                //UpdateAt = branch.UpdateAt
            };

        }

        public Task<BranchResponse> DeleteBranch(int branchId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<BranchResponse>> GetAll()
        {
            var branches = await _context.Branches.ToListAsync(); // Lấy tất cả Branch

            return branches.Select(branch => new BranchResponse
            {
                BranchId = branch.BranchId,
                Name = branch.Name,
                Address = branch.Address,
                PhoneNumber = branch.PhoneNumber,
                CreatedAt = branch.CreatedAt,
                UpdateAt = branch.UpdateAt
            }).ToList();
        }

        public async Task<BranchResponse> GetBranch(int branchId)
        {
            var branch= await _context.Branches.SingleOrDefaultAsync(branch => branch.BranchId == branchId);
            return new BranchResponse
            {
                BranchId = branch.BranchId,
                Name = branch.Name,
                Address = branch.Address,
                PhoneNumber = branch.PhoneNumber,
                CreatedAt = branch.CreatedAt,
                UpdateAt = branch.UpdateAt
            };
        }

        public Task<BranchResponse> UpdateBranch(BranchVModel model, int branchId)
        {
            var update= _context.Branches.SingleOrDefault(branch => branch.BranchId == branchId);
            if (update != null)
            {
                // Id không thay đổi
                update.Name = model.Name;
                update.Address = model.Address;
                update.PhoneNumber = model.PhoneNumber;
                update.UpdateAt = DateTime.UtcNow;
                _context.SaveChanges();
            }
            return Task.FromResult(new BranchResponse
            {
                BranchId = update.BranchId,// Cần có BranchId trong phản hồi
                Name = update.Name,
                Address = update.Address,
                PhoneNumber = update.PhoneNumber,
                CreatedAt = update.CreatedAt,
                UpdateAt = update.UpdateAt
            });
        }
    }
}
