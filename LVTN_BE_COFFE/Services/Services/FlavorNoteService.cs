using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class FlavorNoteService : IFlavorNotesService
    {
        private readonly AppDbContext _context;
        public FlavorNoteService(AppDbContext context)
        {
            _context = context;
        }
        
        // ✅ Create FlavorNotes
        public async Task<ActionResult<ResponseResult>?> CreateFlavorNotes(FlavorNoteCreateVModel request)
        {
            // Check if flavor note name already exists (case-insensitive)
            var existingFlavorNote = await _context.FlavorNotes
                .FirstOrDefaultAsync(f => f.Name.ToLower() == request.Name.ToLower() && f.IsActive == true);

            if (existingFlavorNote != null)
            {
                // Trả về BadRequest (400) thay vì chỉ ErrorResponseResult
                return new BadRequestObjectResult(new ErrorResponseResult("Đã tồn tại"));
            }

            var flavorNote = new FlavorNote
            {
                Name = request.Name,   
                IsActive = request.IsActive, 
                CreatedDate = DateTime.UtcNow
            };

            _context.FlavorNotes.Add(flavorNote);
            await _context.SaveChangesAsync();

            return new SuccessResponseResult(MapToResponse(flavorNote), "Tạo thành công");
        }

        // ✅ Update FlavorNotes
        public async Task<ActionResult<ResponseResult>?> UpdateFlavorNotes(int id, FlavorNoteUpdateVModel request)
        {
            var flavorNote = await _context.FlavorNotes.FindAsync(id);
            if (flavorNote == null || flavorNote.IsActive != true)
                return new ErrorResponseResult("Không tìm thấy");

            flavorNote.Name = request.Name;
            flavorNote.IsActive = true;
            flavorNote.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new SuccessResponseResult(MapToResponse(flavorNote), "Cập nhật thành công");
        }

        // ✅ Get by ID (only active)
        public async Task<ActionResult<ResponseResult>?> GetFlavorNotesById(int id)
        {
            var flavorNote = await _context.FlavorNotes
                .Include(f => f.ProductFlavorNotes)
                .FirstOrDefaultAsync(f => f.Id == id && f.IsActive == true);

            if (flavorNote == null)
                return new ErrorResponseResult("Không tìm thấy");

            return new SuccessResponseResult(MapToResponse(flavorNote), "Lấy dữ liệu thành công");
        }

        // ✅ Soft Delete (Set IsActive to false)
        public async Task<ActionResult<ResponseResult>?> DeleteFlavorNotes(int id)
        {
            var flavorNote = await _context.FlavorNotes.FindAsync(id);
            if (flavorNote == null)
                return new ErrorResponseResult("Không tìm thấy");

            // Soft delete by setting IsActive to false
            flavorNote.IsActive = false;
            flavorNote.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return new SuccessResponseResult(true, "Xóa thành công");
        }

        // ✅ Get all with pagination + filter (only active)
        public async Task<ActionResult<ResponseResult>> GetAllFlavorNotes(FlavorNoteFilterVModel filter)
        {
            // Filter only active flavor notes
            var query = _context.FlavorNotes.Where(f => f.IsActive == true);

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(x => x.Name.Contains(filter.Name));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / filter.PageSize);

            var items = await query
                .Include(f => f.ProductFlavorNotes)
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

        // ✅ Map entity → response
        private static FlavorNoteResponse MapToResponse(FlavorNote x)
        {
            return new FlavorNoteResponse
            {
                FlavorNoteId = x.Id,
                Name = x.Name,
                IsActive = x.IsActive,
                CreatedDate = x.CreatedDate,
                UpdatedDate = x.UpdatedDate
            };
        }
    }
}
