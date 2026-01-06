using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LVTN_BE_COFFE.Services.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;

        public ProductImageService(
            AppDbContext context,
            CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        #region Add single image
        public async Task<ProductImageResponse> AddAsync(ProductImageCreateVModel model)
        {
            string imageUrl;
            string? publicId;

            if (model.File != null)
            {
                var upload = await _cloudinaryService.UploadImageAsync(model.File);
                imageUrl = upload.Url;
                publicId = upload.PublicId;
            }
            else
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

            await HandleMainImageAsync(entity);

            await _context.SaveChangesAsync();

            return MapToResponse(entity);
        }
        #endregion

        #region Add multiple images
        public async Task<List<ProductImageResponse>> AddMultipleAsync(ProductImageMultiCreateVModel model)
        {
            var responses = new List<ProductImageResponse>();

            foreach (var file in model.Files)
            {
                var upload = await _cloudinaryService.UploadImageAsync(file);

                var entity = new ProductImage
                {
                    ProductId = model.ProductId,
                    ProductVariantId = model.ProductVariantId,
                    ImageUrl = upload.Url,
                    PublicId = upload.PublicId,
                    IsMain = false,
                    UploadedAt = DateTime.UtcNow
                };

                _context.ProductImage.Add(entity);
                responses.Add(MapToResponse(entity));
            }

            await _context.SaveChangesAsync();
            return responses;
        }
        #endregion

        #region Update image
        public async Task<ProductImageResponse> UpdateAsync(ProductImageUpdateVModel model, int id)
        {
            string imageUrl;
            string? publicId;
            var img = await _context.ProductImage.FindAsync(id);
            if (img == null)
                throw new KeyNotFoundException("Image không tồn tại.");
            if (model.File != null)
            {
                var update= await _cloudinaryService.UpdateImage(model.File, img.PublicId);
                imageUrl = update.Url;
                publicId = update.PublicId;
            }
            else
            {
                throw new InvalidOperationException("Phải cung cấp File hoặc ImageUrl.");
            }
            img.ImageUrl = imageUrl;
            img.PublicId = publicId;
            img.IsMain = model.IsMain;
            img.UploadedAt = DateTime.UtcNow;
            await HandleMainImageAsync(img);
            await _context.SaveChangesAsync();
            return MapToResponse(img);
        }
        #endregion

        #region Delete image
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.ProductImage.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return false;

            if (!string.IsNullOrEmpty(entity.PublicId))
                await _cloudinaryService.DeleteImageAsync(entity.PublicId);

            _context.ProductImage.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        #endregion

        #region Get
        public async Task<ProductImageResponse?> GetByIdAsync(int id)
        {
            var entity = await _context.ProductImage.FindAsync(id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<List<ProductImageResponse>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductImage
                .Where(x => x.ProductId == productId)
                .OrderBy(x => x.SortOrder)
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
        #endregion

        #region Pagination + Filter
        public async Task<PaginationModel<ProductImageResponse>> GetAllAsync(ProductImageFilterVModel filter)
        {
            var query = _context.ProductImage.AsQueryable();

            if (filter.ProductId.HasValue)
                query = query.Where(x => x.ProductId == filter.ProductId);

            if (filter.ProductVariantId.HasValue)
                query = query.Where(x => x.ProductVariantId == filter.ProductVariantId);

            if (filter.IsMain.HasValue)
                query = query.Where(x => x.IsMain == filter.IsMain);

            var totalRecords = await query.CountAsync();

            var records = await query
                .OrderByDescending(x => x.UploadedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(MapToResponseExpression)
                .ToListAsync();

            return new PaginationModel<ProductImageResponse>
            {
                TotalRecords = totalRecords,
                Records = records
            };
        }
        #endregion

        #region Private helpers
        private async Task HandleMainImageAsync(ProductImage entity)
        {
            if (!entity.IsMain) return;

            await _context.ProductImage
                .Where(x => x.ProductId == entity.ProductId && x.Id != entity.Id)
                .ExecuteUpdateAsync(x =>
                    x.SetProperty(p => p.IsMain, false));
        }
        #endregion

        #region Mapping
        private static ProductImageResponse MapToResponse(ProductImage entity)
        {
            return new ProductImageResponse
            {
                Id = entity.Id,
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
                Id = x.Id,
                ImageUrl = x.ImageUrl,
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                IsMain = x.IsMain,
                SortOrder = x.SortOrder,
                UploadedAt = x.UploadedAt,
                UpdatedAt = x.UpdatedAt
            };
        #endregion
    }
}
