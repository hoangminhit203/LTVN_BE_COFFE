
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel; // Nhớ sửa namespace cho đúng project bạn
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class ShippingAddressService : IShippingAddressService
{
    private readonly AppDbContext _context; // DbContext kết nối CSDL
    private readonly IHttpContextAccessor _httpContextAccessor; // Dùng để lấy User ID đang đăng nhập

    public ShippingAddressService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    // ==========================================================
    // Hàm hỗ trợ: Lấy UserID từ Token hiện tại
    // ==========================================================
    private string GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("Không tìm thấy thông tin người dùng.");
        }
        return userId;
    }

    // ==========================================================
    // 1. TẠO MỚI ĐỊA CHỈ
    // ==========================================================
    public async Task<ActionResult<ResponseResult>?> CreateShippingAddress(ShippingAddressCreateVModel request)
    {
        try
        {
            var userId = GetCurrentUserId();

            // LOGIC: Nếu user chọn địa chỉ này là mặc định, phải bỏ mặc định của các địa chỉ cũ
            if (request.IsDefault)
            {
                var existingDefaults = await _context.ShippingAddresses
                    .Where(x => x.UserId == userId && x.IsDefault)
                    .ToListAsync();

                foreach (var item in existingDefaults)
                {
                    item.IsDefault = false; // Tắt mặc định cũ
                }
            }

            // LOGIC: Nếu đây là địa chỉ đầu tiên user tạo, tự động set làm mặc định luôn cho tiện
            var hasAnyAddress = await _context.ShippingAddresses.AnyAsync(x => x.UserId == userId);
            var shouldBeDefault = request.IsDefault || !hasAnyAddress;

            // Map dữ liệu từ VModel sang Entity
            var newAddress = new ShippingAddress
            {
                UserId = userId,
                FullAddress = request.FullAddress,
                ReceiverName = request.ReceiverName,
                Phone = request.Phone,
                IsDefault = shouldBeDefault
          
            };

            _context.ShippingAddresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseResult
            {
                IsSuccess = true,
                Message = "Thêm địa chỉ thành công",
                Data = newAddress // Hoặc map ngược lại sang ResponseVModel nếu cần
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = ex.Message });
        }
    }

    // ==========================================================
    // 2. CẬP NHẬT ĐỊA CHỈ
    // ==========================================================
    public async Task<ActionResult<ResponseResult>?> UpdateShippingAddress(ShippingAddressUpdateVModel request, int id)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Tìm địa chỉ trong DB (Phải check cả UserId để bảo mật, không cho sửa của người khác)
            var address = await _context.ShippingAddresses
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (address == null)
            {
                return new NotFoundObjectResult(new ResponseResult { IsSuccess = false, Message = "Không tìm thấy địa chỉ." });
            }

            // LOGIC: Xử lý vụ set Default tương tự lúc tạo mới
            if (request.IsDefault && !address.IsDefault)
            {
                var existingDefaults = await _context.ShippingAddresses
                    .Where(x => x.UserId == userId && x.IsDefault && x.Id != id)
                    .ToListAsync();

                foreach (var item in existingDefaults)
                {
                    item.IsDefault = false;
                }
            }

            // Cập nhật thông tin
            address.FullAddress = request.FullAddress;
            address.ReceiverName = request.ReceiverName;
            address.Phone = request.Phone;
            address.IsDefault = request.IsDefault;
            // address.UpdatedAt = DateTime.Now; // Nếu có

            _context.ShippingAddresses.Update(address);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseResult
            {
                IsSuccess = true,
                Message = "Cập nhật địa chỉ thành công"
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = ex.Message });
        }
    }

    // ==========================================================
    // 3. XÓA ĐỊA CHỈ
    // ==========================================================
    public async Task<ActionResult<ResponseResult>> DeleteShippingAddress(int id)
    {
        try
        {
            var userId = GetCurrentUserId();

            var address = await _context.ShippingAddresses
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (address == null)
            {
                return new NotFoundObjectResult(new ResponseResult { IsSuccess = false, Message = "Không tìm thấy địa chỉ." });
            }

            _context.ShippingAddresses.Remove(address);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new ResponseResult { IsSuccess = true, Message = "Đã xóa địa chỉ." });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = ex.Message });
        }
    }

    // ==========================================================
    // 4. LẤY 1 ĐỊA CHỈ (DETAIL)
    // ==========================================================
    public async Task<ActionResult<ResponseResult>?> GetShippingAddress(int id)
    {
        var userId = GetCurrentUserId();

        var address = await _context.ShippingAddresses
            .Where(x => x.Id == id && x.UserId == userId)
            .Select(x => new ShippingAddressResponseVModel
            {
                Id = x.Id,
                UserId = x.UserId,
                FullAddress = x.FullAddress,
                ReceiverName = x.ReceiverName,
                Phone = x.Phone,
                IsDefault = x.IsDefault
            })
            .FirstOrDefaultAsync();

        if (address == null)
        {
            return new NotFoundObjectResult(new ResponseResult { IsSuccess = false, Message = "Không tìm thấy." });
        }

        return new OkObjectResult(new ResponseResult { IsSuccess = true, Data = address });
    }

    // ==========================================================
    // 5. LẤY DANH SÁCH (CÓ FILTER)
    // ==========================================================
    public async Task<ActionResult<ResponseResult>> GetAllShippingAddresses(ShippingAddressFilterVModel filter)
    {
        try
        {
            var userId = GetCurrentUserId();

            // Khởi tạo Query, mặc định chỉ lấy của User đang login
            var query = _context.ShippingAddresses.AsQueryable();

            // Nếu User là Admin (check role nếu cần), có thể xem của người khác dựa vào filter.UserId
            // Ở đây mình giả định user thường chỉ xem của chính mình
            query = query.Where(x => x.UserId == userId);

            // 1. Lọc theo từ khóa (Tìm trong tên người nhận, sđt hoặc địa chỉ)
            if (!string.IsNullOrEmpty(filter.Keyword))
            {
                query = query.Where(x => x.FullAddress.Contains(filter.Keyword)
                                      || x.ReceiverName.Contains(filter.Keyword)
                                      || x.Phone.Contains(filter.Keyword));
            }

            // Đếm tổng số bản ghi trước khi phân trang
            var totalCount = await query.CountAsync();

            // 2. Phân trang (Skip & Take)
            var data = await query
                .OrderByDescending(x => x.IsDefault) // Ưu tiên hiện địa chỉ mặc định lên đầu
                .ThenByDescending(x => x.Id)         // Sau đó đến địa chỉ mới nhất
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new ShippingAddressResponseVModel
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    FullAddress = x.FullAddress,
                    ReceiverName = x.ReceiverName,
                    Phone = x.Phone,
                    IsDefault = x.IsDefault
                })
                .ToListAsync();

            // Trả về kết quả kèm thông tin phân trang (nếu cần thiết kế thêm field Total)
            return new OkObjectResult(new ResponseResult
            {
                IsSuccess = true,
                Data = data,
                Message = $"Tìm thấy {totalCount} địa chỉ."
            });
        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(new ResponseResult { IsSuccess = false, Message = ex.Message });
        }
    }
}