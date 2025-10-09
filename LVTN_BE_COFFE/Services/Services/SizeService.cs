using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class SizeService : ISizeService
    {
        private readonly AppDbContext _context;

        public SizeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SizeResponse> CreateSize(SizeCreateVModel request)
        {
            // Kiểm tra trùng tên
            var existing = await _context.Sizes.FirstOrDefaultAsync(s => s.Name == request.Name);
            if (existing != null)
                throw new Exception("Size name already exists.");

            var size = new Size
            {
                Name = request.Name,
                ExtraPrice = request.ExtraPrice
            };

            _context.Sizes.Add(size);
            await _context.SaveChangesAsync();

            return new SizeResponse
            {
                SizeId = size.SizeId,
                Name = size.Name,
                ExtraPrice = size.ExtraPrice
            };
        }

        public async Task<bool> DeleteSize(int sizeId)
        {
            var size = await _context.Sizes.FindAsync(sizeId);
            if (size == null)
                throw new Exception("Size not found.");

            _context.Sizes.Remove(size);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<SizeResponse>> GetAllSizes()
        {
            return await _context.Sizes
                .Select(s => new SizeResponse
                {
                    SizeId = s.SizeId,
                    Name = s.Name,
                    ExtraPrice = s.ExtraPrice
                })
                .ToListAsync();
        }

        public async Task<SizeResponse?> GetSize(int sizeId)
        {
            var size = await _context.Sizes.FindAsync(sizeId);
            if (size == null) return null;

            return new SizeResponse
            {
                SizeId = size.SizeId,
                Name = size.Name,
                ExtraPrice = size.ExtraPrice
            };
        }

        public async Task<SizeResponse?> UpdateSize(SizeUpdateVModel request)
        {
            var size = await _context.Sizes.FindAsync(request.SizeId);
            if (size == null)
                throw new Exception("Size not found.");

            size.Name = request.Name;
            size.ExtraPrice = request.ExtraPrice;

            _context.Sizes.Update(size);
            await _context.SaveChangesAsync();

            return new SizeResponse
            {
                SizeId = size.SizeId,
                Name = size.Name,
                ExtraPrice = size.ExtraPrice
            };
        }
    }
}
