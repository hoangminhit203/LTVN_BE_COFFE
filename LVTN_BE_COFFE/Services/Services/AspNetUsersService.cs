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
            IQueryable<AspNetUsers> query = _context.AspNetUsers.Where(BuildQueryable(parameters)).OrderByDescending(x => x.Id);
            var records = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize).Select(x => MapEntityToVModel(x)).ToListAsync();
            return new PaginationModel<AspNetUsersGetVModel>()
            {
                Records = records,
                TotalRecords = query.Count()
            };
        }

        public async Task<ActionResult<AspNetUsersGetVModel>?> GetById(string id)
        {
            var entity = await _context.AspNetUsers.FindAsync(id);
            if (entity == null)
            {
                return null;
            }
            return MapEntityToVModel(entity);
        }

        public async Task<ActionResult<AspNetUsersGetVModel>?> Create(AspNetUsersCreateVModel model)
        {
            var entity = new AspNetUsers
            {
                UserName = model.UserName,
                Email = model.Email,
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
            var entity = await _context.AspNetUsers.FindAsync(model.Id);
            if (entity == null)
            {
                return Numbers.FindResponse.NotFound;
            }
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedBy = GlobalUserName;
            entity.IsActive = model.IsActive;
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
