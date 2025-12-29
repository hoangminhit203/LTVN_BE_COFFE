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
    public class AspNetRolesService : Globals, IAspNetRolesService
    {
        private readonly AppDbContext _context;
        private readonly RoleManager<AspNetRoles> _roleManager;

        public AspNetRolesService(AppDbContext context, RoleManager<AspNetRoles> roleManager, IHttpContextAccessor contextAccessor) : base(contextAccessor)
        {
            _context = context;
            _roleManager = roleManager;
        }

        public async Task<ActionResult<PaginationModel<AspNetRolesGetVModel>>> GetAll(AspNetRolesFilterParams parameters)
        {
            IQueryable<AspNetRoles> query = _context.AspNetRoles.Where(BuildQueryable(parameters)).OrderByDescending(x => x.CreatedDate);
            var records = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize).Select(x => MapEntityToVModel(x)).ToListAsync();
            return new PaginationModel<AspNetRolesGetVModel>()
            {
                Records = records,
                TotalRecords = query.Count()
            };
        }

        public async Task<ActionResult<AspNetRolesGetVModel>?> GetById(string id)
        {
            var entity = await _context.AspNetRoles.FindAsync(id);
            if (entity == null)
            {
                return null;
            }
            return MapEntityToVModel(entity);
        }

        public async Task<ActionResult<AspNetRolesGetVModel>?> Create(AspNetRolesCreateVModel model)
        {
            var entity = new AspNetRoles
            {
                Name = model.Name,
                NormalizedName = model.Name.ToUpper(),
                JsonRoleHasFunctions = model.JsonRoleHasFunctions != null ? JsonHelper.SerializeJsonRoleHasFunctions(model.JsonRoleHasFunctions) : null,
                CreatedDate = DateTime.Now,
                CreatedBy = GlobalUserName,
                IsActive = model.IsActive,
            };

            var identityResult = await _roleManager.CreateAsync(entity);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join("; ", identityResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new Exception("Role creation failed: " + errors);
            }

            return MapEntityToVModel(entity);
        }

        public async Task<int> Update(AspNetRolesUpdateVModel model)
        {
            var entity = await _context.AspNetRoles.FindAsync(model.Id);
            if (entity == null)
            {
                return Numbers.FindResponse.NotFound;
            }

            entity.Name = model.Name;
            entity.NormalizedName = model.Name.ToUpper();
            entity.JsonRoleHasFunctions = model.JsonRoleHasFunctions != null ? JsonHelper.SerializeJsonRoleHasFunctions(model.JsonRoleHasFunctions) : null;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedBy = GlobalUserName;
            entity.IsActive = model.IsActive;

            _context.Entry(entity).State = EntityState.Modified;

            var result = await _context.SaveChangesAsync();
            return result;
        }

        public async Task<int> Remove(string id)
        {
            var entity = await _context.AspNetRoles.FindAsync(id);
            if (entity == null)
            {
                return Numbers.FindResponse.NotFound;
            }

            _context.AspNetRoles.Remove(entity);

            return await _context.SaveChangesAsync();
        }

        private static AspNetRolesGetVModel MapEntityToVModel(AspNetRoles entity) =>
            new AspNetRolesGetVModel
            {
                Id = entity.Id,
                Name = entity.Name ?? string.Empty,
                JsonRoleHasFunctions = entity.JsonRoleHasFunctions != null ? JsonHelper.DeserializeJsonRoleHasFunctions(entity.JsonRoleHasFunctions) : new List<dynamic>(),
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,
                UpdatedBy = entity.UpdatedBy,
                UpdatedDate = entity.UpdatedDate,
                IsActive = entity.IsActive,
            };
        private Expression<Func<AspNetRoles, bool>> BuildQueryable(AspNetRolesFilterParams fParams)
        {
            return x =>
                // 1. Lọc theo Name (Giữ nguyên)
                (string.IsNullOrEmpty(fParams.Name) || (x.Name != null && x.Name.Contains(fParams.Name))) &&

                // 2. Lọc CreatedBy (Giữ nguyên - Code bạn đã sửa đúng)
                (string.IsNullOrEmpty(fParams.CreatedBy) || (x.CreatedBy != null && x.CreatedBy.Contains(fParams.CreatedBy))) &&

                // 3. Lọc UpdatedBy (Giữ nguyên)
                (string.IsNullOrEmpty(fParams.UpdatedBy) || (x.UpdatedBy != null && x.UpdatedBy.Contains(fParams.UpdatedBy))) &&

                // 4. Lọc CreatedDate (SỬA: Chỉ so sánh khi trong DB có dữ liệu)
                (!fParams.CreatedDate.HasValue || (x.CreatedDate.HasValue && x.CreatedDate.Value.Date == fParams.CreatedDate.Value.Date)) &&

                // 5. Lọc UpdatedDate (QUAN TRỌNG: Admin đang bị chết ở đây)
                (!fParams.UpdatedDate.HasValue || (x.UpdatedDate.HasValue && x.UpdatedDate.Value.Date == fParams.UpdatedDate.Value.Date)) &&

                // 6. IsActive
                (!fParams.IsActive.HasValue || x.IsActive == fParams.IsActive);
        }
            }
}