using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class BrewingMethodsService : IBrewingMethodsService
    {
        private readonly AppDbContext _context;

        public BrewingMethodsService(AppDbContext context)
        {
            _context = context;
        }

        // Create BrewingMethods
        public async Task<ActionResult<ResponseResult>?> CreateBrewingMethods(BrewingMethodsCreateVModel request)
        {
            // Check if brewing method name already exists (case-insensitive)
            var existingBrewingMethod = await _context.BrewingMethods
                .FirstOrDefaultAsync(b => b.Name.ToLower() == request.Name.ToLower() && b.IsActive == true);

            if (existingBrewingMethod != null)
            {
                return new BadRequestObjectResult(new ErrorResponseResult("Đã tồn tại"));
            }

            var brewingMethod = new BrewingMethod
            {
                Name = request.Name,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            _context.BrewingMethods.Add(brewingMethod);
            await _context.SaveChangesAsync();

            return new SuccessResponseResult(MapToResponse(brewingMethod), "Tạo thành công");
        }

        // Update BrewingMethods
        public async Task<ActionResult<ResponseResult>?> UpdateBrewingMethods(int id, BrewingMethodsUpdateVModel request)
        {
            var brewingMethod = await _context.BrewingMethods.FindAsync(id);
            if (brewingMethod == null || brewingMethod.IsActive != true)
                return new ErrorResponseResult("Không tìm thấy");

            // Check if name already exists for another record
            var existingBrewingMethod = await _context.BrewingMethods
                .FirstOrDefaultAsync(b => b.Name.ToLower() == request.Name.ToLower() && b.Id != id && b.IsActive == true);

            if (existingBrewingMethod != null)
            {
                return new BadRequestObjectResult(new ErrorResponseResult("Đã tồn tại"));
            }

            brewingMethod.Name = request.Name;
            brewingMethod.Description = request.Description;
            brewingMethod.IsActive = request.IsActive ?? true;
            brewingMethod.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new SuccessResponseResult(MapToResponse(brewingMethod), "Cập nhật thành công");
        }

        // Get by ID
        public async Task<ActionResult<ResponseResult>?> GetBrewingMethodsById(int id)
        {
            var brewingMethod = await _context.BrewingMethods
                .Include(b => b.ProductBrewingMethods)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive == true);

            if (brewingMethod == null)
                return new ErrorResponseResult("Không tìm thấy");

            return new SuccessResponseResult(MapToResponse(brewingMethod), "Lấy dữ liệu thành công");
        }

        // Soft Delete
        public async Task<ActionResult<ResponseResult>?> DeleteBrewingMethods(int id)
        {
            var brewingMethod = await _context.BrewingMethods.FindAsync(id);
            if (brewingMethod == null)
                return new ErrorResponseResult("Không tìm thấy");

            // Soft delete by setting IsActive to false
            brewingMethod.IsActive = false;
            brewingMethod.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new SuccessResponseResult(true, "Xóa thành công");
        }

        // Get all with pagination + filter (only active)
        public async Task<ActionResult<ResponseResult>> GetAllBrewingMethods(BrewingMethodsFilterVModel filter)
        {
            // Filter only active brewing methods
            var query = _context.BrewingMethods.Where(b => b.IsActive == true);

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(x => x.Name.Contains(filter.Name));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            var items = await query
                .Include(b => b.ProductBrewingMethods)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var paginationResponse = new
            {
                TotalRecords = totalCount,
                TotalPages = totalPages,
                CurrentPage = filter.PageNumber,
                PageSize = filter.PageSize,
                Records = items.Select(MapToResponse).ToList()
            };

            return new SuccessResponseResult(paginationResponse, "Lấy dữ liệu thành công");
        }

        // Map entity → response
        private static BrewingMethodsResponse MapToResponse(BrewingMethod x)
        {
            return new BrewingMethodsResponse
            {
                BrewingMethodId = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate
            };
        }
    }
}