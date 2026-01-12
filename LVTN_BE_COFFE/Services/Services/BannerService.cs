using DocumentFormat.OpenXml.Office2010.Excel;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LVTN_BE_COFFE.Services.Services
{
    public class BannerService : IBannerService
    {
        private readonly AppDbContext _context;
        private readonly CloudinaryService _cloudinaryService;
        public BannerService(AppDbContext context, CloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<ActionResult<ResponseResult>> CreateBannerAsync(BannerCreateVmodel vmodel)
        {
            var existingBanner = await _context.Banners
                .FirstOrDefaultAsync(b => b.Position == vmodel.Position &&  b.Type == vmodel.Type);

            if (existingBanner!=null)
            {
                var responseFail = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Vị trí banner trong banner type đã tồn tại",
                    Data = null
                };
                return new ActionResult<ResponseResult>(responseFail);
            }
            string url = "";
            string publicId = "";
            if (vmodel.File != null)
            {
                var upload = await _cloudinaryService.UploadImageAsync(vmodel.File);
                url =upload.Url ;
                publicId= upload.PublicId;
            }
            var banner = new Banner
            { 
                PublicId = publicId,
                ImageUrl = url,
                IsActive = vmodel.IsActive,
                Position = vmodel.Position,
                Type = vmodel.Type,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Banners.AddAsync(banner);
            _context.SaveChanges();
            var response = new ResponseResult
            {
                IsSuccess = true,
                Message = "Tạo banner thành công",
                Data = banner
            };
            return new ActionResult<ResponseResult>(response);
        }
        public async Task<ActionResult<ResponseResult>> DeleteBannerAsync(int id)
        {
            var Exits = await _context.Banners.FindAsync(id);
            if (Exits == null)
            {
                var notFoundResponse = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy banner",
                    Data = null
                };
                return new ActionResult<ResponseResult>(notFoundResponse);
            }
            _context.Banners.Remove(Exits);
            _context.SaveChanges();
            var response = new ResponseResult
            {
                IsSuccess = true,
                Message = "Xóa banner thành công",
                Data = null
            };
            return new ActionResult<ResponseResult>(response);
        }

        public async Task<ActionResult<List<ResponseResult>>> GetAllBannersAsync()
        {
            var banners = await _context.Banners.ToListAsync();
            if(banners ==null)
            {
                var responseFail = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Không có banner nào",
                    Data = null
                };
                return new ActionResult<List<ResponseResult>>(new List<ResponseResult> { responseFail });
            }    
            var response = new ResponseResult
            {
                IsSuccess = true,
                Message = "Lấy danh sách banner thành công",
                Data = banners
            };
            return new ActionResult<List<ResponseResult>>(new List<ResponseResult> { response });
        }

        public async Task<ActionResult<ResponseResult>> GetBannerByIdAsync(int id)
        {
            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                var notFoundResponse = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy banner",
                    Data = null
                };
                return new ActionResult<ResponseResult>(notFoundResponse);
            }
            var response = new ResponseResult
            {
                IsSuccess = true,
                Message = "Lấy banner thành công",
                Data = banner
            };
            return new ActionResult<ResponseResult>(response);
        }

        public async Task<ActionResult<ResponseResult>> GetBannerIsActive()
        {
            
            var banners= await _context.Banners.Where(i => i.IsActive == true).ToListAsync();
            if(banners == null)
            {
                var responseFail = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Không có banner active",
                    Data = null
                };
                return new ActionResult<ResponseResult>(responseFail);
            }
            var response = new ResponseResult
            {
                IsSuccess = true,
                Message = "Lấy banner active thành công",
                Data = banners
            };
            return new ActionResult<ResponseResult>(response);

            
        }

        public async Task<ActionResult<ResponseResult>> UpdateBannerAsync(int id, BannerUpdateVmodel vmodel)
        {

            var banner = await _context.Banners.FindAsync(id);
            if (banner == null)
            {
                var notFoundResponse = new ResponseResult
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy banner",
                    Data = null
                };
                return new ActionResult<ResponseResult>(notFoundResponse);
            }
            if (vmodel.File != null)
            {
                var update = await _cloudinaryService.UpdateImage(vmodel.File,banner.PublicId);
                banner.ImageUrl = update.Url;
                banner.PublicId = update.PublicId;
            }
            banner.IsActive = vmodel.IsActive;
            banner.Position = vmodel.Position;
            banner.UpdatedAt = DateTime.UtcNow;
            _context.Banners.Update(banner);
            await _context.SaveChangesAsync();
            var response = new ResponseResult
            {
                IsSuccess = true,
                Message = "Cập nhật banner thành công",
                Data = banner
            };
            return new ActionResult<ResponseResult>(response);
        }
    }
}
