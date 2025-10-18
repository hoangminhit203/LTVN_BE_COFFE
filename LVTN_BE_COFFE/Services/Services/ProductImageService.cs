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

        /// <summary>
        /// Thêm mới ảnh — upload lên Cloudinary nếu có file.
        /// </summary>
        public async Task<ProductImageResponse> AddAsync(ProductImageCreateVModel model)
        {
            // Upload ảnh lên Cloudinary
            Console.WriteLine("Start AddAsync");
            var (url, publicId) = await _cloudinaryService.UploadImageAsync(model.File);
            Console.WriteLine($"Uploaded URL: {url}");

            // Tạo entity và gán thông tin URL + PublicId
            var entity = new ProductImage
            {
                ProductId = model.ProductId,
                ProductVariantId = model.ProductVariantId,
                ImageUrl = url,
                PublicId = publicId,
                UploadedAt = DateTime.UtcNow
            };
            Console.WriteLine($"Entity before save: {entity.ImageUrl}");


            // ✅ 3. Lưu vào DB
            try
            {
                _context.Images.Add(entity);
                var result = await _context.SaveChangesAsync();
                Console.WriteLine($"Saved rows: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("EF Save Error: " + ex.InnerException?.Message ?? ex.Message);
                throw;
            }


            // ✅ 4. Map sang response trả về
            return new ProductImageResponse
            {
                ImageId = entity.ImageId,
                ProductId = entity.ProductId,
                ProductVariantId = entity.ProductVariantId,
                ImageUrl = entity.ImageUrl
            };
        }

        /// <summary>
        /// Cập nhật thông tin ảnh (có thể thay đổi file).
        /// </summary>
        public async Task<ProductImageResponse> UpdateAsync(ProductImageUpdateVModel model)
        {
            var entity = await _context.Images.FindAsync(model.ImageId);
            if (entity == null)
                throw new KeyNotFoundException("Không tìm thấy hình ảnh.");

            string newUrl = model.ImageUrl ?? entity.ImageUrl;
            string? newPublicId = entity.PublicId;

            // Nếu có file mới => upload lại và thay PublicId cũ
            if (model.File != null)
            {
                // Xóa ảnh cũ trên Cloudinary
                if (!string.IsNullOrEmpty(entity.PublicId))
                    await _cloudinaryService.DeleteImageAsync(entity.PublicId);

                var uploadResult = await _cloudinaryService.UploadImageAsync(model.File);
                newUrl = uploadResult.Url;
                newPublicId = uploadResult.PublicId;
            }

            entity.ImageUrl = newUrl;
            entity.PublicId = newPublicId;
            entity.IsMain = model.IsMain;
            entity.ProductId = model.ProductId;
            entity.ProductVariantId = model.ProductVariantId;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToResponse(entity);
        }

        /// <summary>
        /// Xóa ảnh khỏi DB và Cloudinary.
        /// </summary>
        public async Task<bool> DeleteAsync(int imageId)
        {
            var entity = await _context.Images.FindAsync(imageId);
            if (entity == null) return false;

            // Xóa trên Cloudinary
            if (!string.IsNullOrEmpty(entity.PublicId))
                await _cloudinaryService.DeleteImageAsync(entity.PublicId);

            _context.Images.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Lấy ảnh theo ID.
        /// </summary>
        public async Task<ProductImageResponse?> GetByIdAsync(int imageId)
        {
            var entity = await _context.Images.FindAsync(imageId);
            return entity == null ? null : MapToResponse(entity);
        }

        /// <summary>
        /// Lấy tất cả ảnh của 1 sản phẩm.
        /// </summary>
        public async Task<List<ProductImageResponse>> GetByProductIdAsync(int productId)
        {
            return await _context.Images
                .Where(x => x.ProductId == productId)
                .Select(MapToResponseExpression)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy ảnh theo biến thể sản phẩm.
        /// </summary>
        public async Task<List<ProductImageResponse>> GetByProductVariantIdAsync(int variantId)
        {
            return await _context.Images
                .Where(x => x.ProductVariantId == variantId)
                .Select(MapToResponseExpression)
                .ToListAsync();
        }

        /// <summary>
        /// Lọc & phân trang ảnh.
        /// </summary>
        public async Task<ActionResult<PaginationModel<ProductImageResponse>>> GetAllAsync(ProductImageFilterVModel filterVModel)
        {
            var query = _context.Images.AsQueryable();

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

        // Helper: Map entity -> DTO
        private static ProductImageResponse MapToResponse(ProductImage entity)
        {
            return new ProductImageResponse
            {
                ImageId = entity.ImageId,
                ImageUrl = entity.ImageUrl,
                PublicId = entity.PublicId, // ✅ thêm vào response
                ProductId = entity.ProductId,
                ProductVariantId = entity.ProductVariantId,
                IsMain = entity.IsMain,
                UploadedAt = entity.UploadedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        private static Expression<Func<ProductImage, ProductImageResponse>> MapToResponseExpression =>
            x => new ProductImageResponse
            {
                ImageId = x.ImageId,
                ImageUrl = x.ImageUrl,
                PublicId = x.PublicId, // ✅ thêm vào response mapping
                ProductId = x.ProductId,
                ProductVariantId = x.ProductVariantId,
                IsMain = x.IsMain,
                UploadedAt = x.UploadedAt,
                UpdatedAt = x.UpdatedAt
            };
    }
}
