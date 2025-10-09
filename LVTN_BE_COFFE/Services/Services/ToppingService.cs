using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class ToppingService : IToppingService
    {
        private readonly AppDbContext _context;

        public ToppingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ToppingResponse?> CreateTopping(ToppingCreateVModel model)
        {
            var exist = await _context.Toppings
                .FirstOrDefaultAsync(x => x.Name == model.Name);

            if (exist != null)
                throw new Exception("Topping đã tồn tại");

            var topping = new Topping
            {
                Name = model.Name,
                ExtraPrice = model.ExtraPrice,
                CreatedAt = DateTime.UtcNow
            };

            _context.Toppings.Add(topping);
            await _context.SaveChangesAsync();

            return new ToppingResponse
            {
                ToppingId = topping.ToppingId,
                Name = topping.Name,
                ExtraPrice = topping.ExtraPrice,
                CreatedAt = topping.CreatedAt
            };
        }

        public async Task<bool> DeleteTopping(int toppingId)
        {
            var topping = await _context.Toppings.FindAsync(toppingId);
            if (topping == null)
                throw new Exception("Không tìm thấy topping để xóa");

            _context.Toppings.Remove(topping);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ToppingResponse>> GetAllToppings()
        {
            return await _context.Toppings
                .Select(t => new ToppingResponse
                {
                    ToppingId = t.ToppingId,
                    Name = t.Name,
                    ExtraPrice = t.ExtraPrice,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<ToppingResponse?> GetTopping(int toppingId)
        {
            var t = await _context.Toppings.FindAsync(toppingId);
            if (t == null) return null;

            return new ToppingResponse
            {
                ToppingId = t.ToppingId,
                Name = t.Name,
                ExtraPrice = t.ExtraPrice,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };
        }

        public async Task<ToppingResponse?> UpdateTopping(ToppingUpdateVModel model, int toppingId)
        {
            var t = await _context.Toppings.FindAsync(toppingId);
            if (t == null)
                throw new Exception("Không tìm thấy topping để cập nhật");

            t.Name = model.Name;
            t.ExtraPrice = model.ExtraPrice;
            t.UpdatedAt = DateTime.UtcNow;

            _context.Toppings.Update(t);
            await _context.SaveChangesAsync();

            return new ToppingResponse
            {
                ToppingId = t.ToppingId,
                Name = t.Name,
                ExtraPrice = t.ExtraPrice,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            };
        }
    }
}
