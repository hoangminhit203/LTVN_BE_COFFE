using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.Ultilities;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LVTN_BE_COFFE.Services.Services
{
    public class AspNetUsersService : Globals, IAspNetUsersService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AspNetUsers> _userManager;
        private readonly RoleManager<AspNetRoles> _roleManager;
        public AspNetUsersService(AppDbContext context, UserManager<AspNetUsers> userManager, RoleManager<AspNetRoles> roleManager, IHttpContextAccessor contextAccessor) : base(contextAccessor)
        {

            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public async Task<ActionResult<PaginationModel<AspNetUsersGetVModel>>> GetAll(AspNetUsersFilterParams parameters)
        {
            // 1. Tạo query lọc dữ liệu như cũ
            IQueryable<AspNetUsers> query = _context.AspNetUsers
                .Where(BuildQueryable(parameters))
                .OrderByDescending(x => x.Id);

            // 2. Đếm tổng số bản ghi (để phân trang)
            var totalRecords = await query.CountAsync();

            // 3. Lấy danh sách Entity (chưa map sang VModel vội để xử lý Role sau)
            var entities = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            // 4. Tạo list kết quả
            var records = new List<AspNetUsersGetVModel>();

            // 5. Lặp qua từng User để lấy Role tương ứng
            foreach (var entity in entities)
            {
                // Map thông tin cơ bản
                var vModel = MapEntityToVModel(entity);

                // --- QUAN TRỌNG: Lấy Role từ bảng AspNetUserRoles ---
                // Hàm GetRolesAsync sẽ trả về danh sách tên quyền (vd: ["Admin", "User"])
                var roles = await _userManager.GetRolesAsync(entity);

                // Lấy quyền đầu tiên gán vào RoleName (nếu có)
                vModel.RoleName = roles.FirstOrDefault();

                records.Add(vModel);
            }

            return new PaginationModel<AspNetUsersGetVModel>()
            {
                Records = records,
                TotalRecords = totalRecords
            };
        }
        public async Task<ActionResult<AspNetUsersGetVModel>?> GetById(string id)
        {
            var entity = await _context.AspNetUsers.FindAsync(id);
            if (entity == null)
            {
                return null;
            }

            // Map thông tin cơ bản
            var vModel = MapEntityToVModel(entity);

            // Lấy Role gán vào
            var roles = await _userManager.GetRolesAsync(entity);
            vModel.RoleName = roles.FirstOrDefault();

            return vModel;
        }
        public async Task<ActionResult<AspNetUsersGetVModel>?> Create(AspNetUsersCreateVModel model)
        {
            var entity = new AspNetUsers
            {   
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                FirstName = model.FirstName,
                LastName = model.LastName,
                AvatarPath = model.AvatarPath,
                Sex = model.Sex,
                Birthday = model.Birthday,
                CreatedDate = DateTime.Now,
                CreatedBy = GlobalUserName,
                IsActive = model.IsActive,
            };

            // 1. Tạo User
            var identityResult = await _userManager.CreateAsync(entity, model.Password);

            if (!identityResult.Succeeded)
            {
                var errors = string.Join("; ", identityResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new Exception("User creation failed: " + errors);
            }

            // ================== SỬA LẠI ĐOẠN NÀY ==================

            // 1. Lấy tên quyền từ Model gửi lên. Nếu không gửi -> mặc định là "Customer"
            string roleToAssign = !string.IsNullOrEmpty(model.RoleName) ? model.RoleName : "Customer";

            // 2. Kiểm tra quyền này đã có trong DB chưa
            if (!await _roleManager.RoleExistsAsync(roleToAssign))
            {
                // Chưa có -> Tạo mới (Ví dụ: Tạo role Admin)
                var newRole = new AspNetRoles
                {
                    Name = roleToAssign,
                    NormalizedName = roleToAssign.ToUpper(),
                    CreatedDate = DateTime.Now,
                    CreatedBy = "System",
                    IsActive = true
                };
                await _roleManager.CreateAsync(newRole);
            }

            // 3. Gán User vào quyền đó
            await _userManager.AddToRoleAsync(entity, roleToAssign);

            // ================== KẾT THÚC SỬA ==================

            return MapEntityToVModel(entity);
        }

        public async Task<int> Update(AspNetUsersUpdateVModel model)
        {
            // 1. Tìm User trong DB
            var entity = await _context.AspNetUsers.FindAsync(model.Id);
            if (entity == null)
            {
                return Numbers.FindResponse.NotFound;
            }

            // 2. Cập nhật các thông tin cơ bản
            entity.FirstName = model.FirstName;
            entity.LastName = model.LastName;
            entity.Sex = model.Sex;
            entity.Birthday = model.Birthday;
            entity.PhoneNumber = model.PhoneNumber;
            entity.AvatarPath = model.AvatarPath; // Nếu có cập nhật ảnh

            // Cập nhật thông tin quản trị
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedBy = GlobalUserName;
            entity.IsActive = model.IsActive;

            // 3. XỬ LÝ CẬP NHẬT ROLE (QUAN TRỌNG)
            if (!string.IsNullOrEmpty(model.RoleName))
            {
                // Lấy danh sách Role hiện tại của User
                var currentRoles = await _userManager.GetRolesAsync(entity);

                // Kiểm tra xem Role mới gửi lên có khác Role hiện tại không
                // (Giả sử 1 user chỉ có 1 role chính, ta lấy role đầu tiên so sánh)
                string currentRole = currentRoles.FirstOrDefault();

                if (currentRole != model.RoleName)
                {
                    // A. Xóa User khỏi tất cả Role cũ
                    if (currentRoles.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(entity, currentRoles);
                    }

                    // B. Kiểm tra Role mới đã tồn tại trong bảng AspNetRoles chưa
                    if (!await _roleManager.RoleExistsAsync(model.RoleName))
                    {
                        // Chưa có thì tạo mới luôn (giống bên Create)
                        var newRole = new AspNetRoles
                        {
                            Name = model.RoleName,
                            NormalizedName = model.RoleName.ToUpper(),
                            CreatedDate = DateTime.Now,
                            CreatedBy = GlobalUserName,
                            IsActive = true
                        };
                        await _roleManager.CreateAsync(newRole);
                    }

                    // C. Thêm User vào Role mới
                    await _userManager.AddToRoleAsync(entity, model.RoleName);
                }
            }

            // 4. Lưu các thay đổi thông tin cơ bản vào DB
            _context.Entry(entity).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();

            return result;
        }

        public async Task<int> Remove(string id)
        {
            var entity = await _context.AspNetUsers.FindAsync(id);
            if (entity == null)
            {
                return Numbers.FindResponse.NotFound;
            }

            _context.AspNetUsers.Remove(entity);

            return await _context.SaveChangesAsync(); ;
        }
        private static AspNetUsersGetVModel MapEntityToVModel(AspNetUsers entity) =>
            new AspNetUsersGetVModel
            {
                Id = entity.Id,
                UserName = entity.UserName,
                Email = entity.Email,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                AvatarPath = entity.AvatarPath,
                Sex = entity.Sex,
                Birthday = entity.Birthday,
                JsonUserHasFunctions = entity.JsonUserHasFunctions != null ? JsonHelper.DeserializeJsonUserHasFunctions(entity.JsonUserHasFunctions) : new List<dynamic>(),
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,
                UpdatedBy = entity.UpdatedBy,
                UpdatedDate = entity.UpdatedDate,
                IsActive = entity.IsActive,
            };
        private Expression<Func<AspNetUsers, bool>> BuildQueryable(AspNetUsersFilterParams fParams)
        {
            return x =>
                (fParams.CreatedDate == null || (x.CreatedDate != null && x.CreatedDate.Value.Year.Equals(fParams.CreatedDate.Value.Year) && x.CreatedDate.Value.Month.Equals(fParams.CreatedDate.Value.Month) && x.CreatedDate.Value.Day.Equals(fParams.CreatedDate.Value.Day))) &&
                (fParams.CreatedBy == null || (x.CreatedBy != null && x.CreatedBy.Contains(fParams.CreatedBy))) &&
                (fParams.UpdatedDate == null || (x.UpdatedDate != null && x.UpdatedDate.Value.Year.Equals(fParams.UpdatedDate.Value.Year) && x.UpdatedDate.Value.Month.Equals(fParams.UpdatedDate.Value.Month) && x.UpdatedDate.Value.Day.Equals(fParams.UpdatedDate.Value.Day))) &&
                (fParams.UpdatedBy == null || (x.UpdatedBy != null && x.UpdatedBy.Contains(fParams.UpdatedBy))) &&
                (fParams.IsActive == null || x.IsActive == fParams.IsActive);
        }
    }
}
