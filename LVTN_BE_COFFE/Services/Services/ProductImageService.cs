using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using LVTN_BE_COFFE.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LVTN_BE_COFFE.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public ProductImageService(AppDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ProductImageResponse> AddAsync(ProductImageCreateVModel model)
        {
            string imageUrl = model.ImageUrl ?? string.Empty;
            string? publicId = model.PublicId;

            if (model.File != null)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(model.File);
                imageUrl = uploadResult.Url;
                publicId = uploadResult.PublicId;
            }
            else if (string.IsNullOrEmpty(imageUrl))
            {
                throw new InvalidOperationException("Phải cung cấp File hoặc ImageUrl.");
            }

            var entity = new ProductImage
            {
                ProductId = model.ProductId,
                ProductVariantId = model.ProductVariantId,
                ImageUrl = imageUrl,
                PublicId = publicId,
                IsMain = model.IsMain,
                SortOrder = model.SortOrder,
                UploadedAt = DateTime.UtcNow
            };

            _context.ProductImage.Add(entity);
            await _context.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<ProductImageResponse> UpdateAsync(ProductImageUpdateVModel model)
        {
            var entity = await _context.ProductImage.FindAsync(model.Id);
            if (entity == null)
                throw new KeyNotFoundException("Không tìm thấy hình ảnh.");

            string newUrl = entity.ImageUrl;
            string? newPublicId = entity.PublicId;

            if (model.File != null)
            {
                if (!string.IsNullOrEmpty(entity.PublicId))
                    await _cloudinaryService.DeleteImageAsync(entity.PublicId);

                var uploadResult = await _cloudinaryService.UploadImageAsync(model.File);
                newUrl = uploadResult.Url;
                newPublicId = uploadResult.PublicId;
            }
            else if (model.ImageUrl != null)
            {
                newUrl = model.ImageUrl;
                newPublicId = model.PublicId;
            }

            entity.ImageUrl = newUrl;
            entity.PublicId = newPublicId;
            entity.IsMain = model.IsMain;
            entity.SortOrder = model.SortOrder;
            entity.ProductId = model.ProductId;
            entity.ProductVariantId = model.ProductVariantId;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ProductImage.FindAsync(id);
            if (entity == null) return false;

            if (!string.IsNullOrEmpty(entity.PublicId))
                await _cloudinaryService.DeleteImageAsync(entity.PublicId);

            _context.ProductImage.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProductImageResponse?> GetByIdAsync(int id)
        {
            var entity = await _context.ProductImage.FindAsync(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<ProductImageResponse>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductImage
                .Where(x => x.ProductId == productId)
                .Select(MapToResponseExpression)
                .ToListAsync();
        }

        public async Task<List<ProductImageResponse>> GetByProductVariantIdAsync(int variantId)
        {
            return await _context.ProductImage
                .Where(x => x.ProductVariantId == variantId)
                .Select(MapToResponseExpression)
                .ToListAsync();
        }

        public async Task<PaginationModel<ProductImageResponse>> GetAllAsync(ProductImageFilterVModel filterVModel)
        {
            var query = _context.ProductImage.AsQueryable();

            if (filterVModel.ProductId.HasValue)
                query = query.Where(i => i.ProductId == filterVModel.ProductId);
            if (filterVModel.ProductVariantId.HasValue)
                query = query.Where(i => i.ProductVariantId == filterVModel.ProductVariantId);
            if (filterVModel.IsMain.HasValue)
                query = query.Where(i => i.IsMain == filterVModel.IsMain);
            if (filterVModel.UploadedFrom.HasValue)
                query = query.Where(i => i.UploadedAt >= filterVModel.UploadedFrom);
            if (filterVModel.UploadedTo.HasValue)
                query = query.Where(i => i.UploadedAt <= filterVModel.UploadedTo);

            var totalRecords = await query.CountAsync();
            var records = await query
                .OrderByDescending(i => i.UploadedAt)
                .Skip((filterVModel.PageNumber - 1) * filterVModel.PageSize)
                .Take(filterVModel.PageSize)
                .Select(MapToResponseExpression)
                .ToListAsync();

            return new PaginationModel<ProductImageResponse>
            {
                TotalRecords = totalRecords,
                Records = records
            };
        }
        private static ProductImageResponse MapToResponse(ProductImage entity)
        {
            return new ProductImageResponse
            {
                ImageUrl = entity.ImageUrl,
                ProductId = entity.ProductId,
                ProductVariantId = entity.ProductVariantId,
                IsMain = entity.IsMain,
                SortOrder = entity.SortOrder,
                UploadedAt = entity.UploadedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        private static Expression<Func<ProductImage, ProductImageResponse>> MapToResponseExpression =>
            x => new ProductImageResponse
            {
                ImageUrl = x.ImageUrl,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                IsMain = x.IsMain,
                SortOrder = x.SortOrder,
                UploadedAt = x.UploadedAt,
                UpdatedAt = x.UpdatedAt
            };
    }
}
