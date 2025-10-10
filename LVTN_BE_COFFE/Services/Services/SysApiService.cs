using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LVTN_BE_COFFE.Services.Services
{
    public class SysApiService : Globals, ISysApiService
    {
        private readonly AppDbContext _context;

        public SysApiService(AppDbContext context, IHttpContextAccessor contextAccessor) : base(contextAccessor)
        {
            _context = context;
        }
        public async Task<ActionResult<PaginationModel<SysApiGetVModel>>> GetAll(SysApiFilterParams parameters)
        {
            IQueryable<SysApi> query = _context.SysApis.Where(BuildQueryable(parameters)).OrderByDescending(x => x.Id);
            var records = await query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize).Select(x => MapEntityToVModel(x)).ToListAsync();
            return new PaginationModel<SysApiGetVModel>()
            {
                Records = records,
                TotalRecords = query.Count()
            };
        }

        public async Task<ActionResult<SysApiGetVModel>?> GetById(long id)
        {
            var entity = await _context.SysApis.FindAsync(id);
            if (entity == null)
            {
                return null;
            }
            return MapEntityToVModel(entity);
        }

        public async Task<ActionResult<SysApiGetVModel>> Create(SysApiCreateVModel model)
        {
            var entity = new SysApi
            {
                ControllerName = model.ControllerName,
                ActionName = model.ActionName,
                HttpMethod = model.HttpMethod,
                CreatedDate = DateTime.Now,
                CreatedBy = GlobalUserName,
                IsActive = model.IsActive,
            };
            _context.SysApis.Add(entity);
            await _context.SaveChangesAsync();

            return MapEntityToVModel(entity);
        }

        public async Task<int> Update(SysApiUpdateVModel model)
        {
            var entity = await _context.SysApis.FindAsync(model.Id);
            if (entity == null)
            {
                return Numbers.FindResponse.NotFound;
            }
            entity.ControllerName = model.ControllerName;
            entity.ActionName = model.ActionName;
            entity.HttpMethod = model.HttpMethod;
            entity.UpdatedDate = DateTime.Now;
            entity.UpdatedBy = GlobalUserName;
            entity.IsActive = model.IsActive;
            _context.Entry(entity).State = EntityState.Modified;

            var result = await _context.SaveChangesAsync();
            return result;
        }

        public async Task<int> Remove(long id)
        {
            var entity = await _context.SysApis.FindAsync(id);
            if (entity == null)
            {
                return Numbers.FindResponse.NotFound;
            }

            _context.SysApis.Remove(entity);

            return await _context.SaveChangesAsync(); ;
        }
        private static SysApiGetVModel MapEntityToVModel(SysApi entity) =>
            new SysApiGetVModel
            {
                Id = entity.Id,
                ControllerName = entity.ControllerName,
                ActionName = entity.ActionName,
                HttpMethod = entity.HttpMethod,
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,
                UpdatedBy = entity.UpdatedBy,
                UpdatedDate = entity.UpdatedDate,
                IsActive = entity.IsActive,
            };
        private Expression<Func<SysApi, bool>> BuildQueryable(SysApiFilterParams fParams)
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
