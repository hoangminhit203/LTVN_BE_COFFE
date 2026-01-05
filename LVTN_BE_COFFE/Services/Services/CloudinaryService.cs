using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using LVTN_BE_COFFE.Services.Helpers;
using Microsoft.Extensions.Options;

namespace LVTN_BE_COFFE.Services.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }
        /// Upload ảnh lên Cloudinary — trả về Url + PublicId.
        public async Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "productsImage" // Ảnh sẽ được lưu trong folder "productsImage"
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception("Lỗi upload Cloudinary: " + result.Error?.Message);

            // ✅ Trả về cả URL và PublicId
            return (result.SecureUrl.AbsoluteUri, result.PublicId);
        }

        // Thay đổi kiểu trả về ở đây ------------------v
        public async Task<(string Url, string PublicId)> UpdateImage(IFormFile file, string publicId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không hợp lệ.");

            // Xóa ảnh cũ nếu publicId được cung cấp
            if (!string.IsNullOrEmpty(publicId))
            {
                var deletionParams = new DeletionParams(publicId);
                var deletionResult = await _cloudinary.DestroyAsync(deletionParams);

                if (deletionResult.Result != "ok" && deletionResult.Result != "not found")
                    throw new Exception("Lỗi khi xóa ảnh Cloudinary: " + deletionResult.Error?.Message);
            }

            // Upload ảnh mới
            // Giả sử hàm UploadImageAsync cũng trả về một Tuple (string, string)
            var (newUrl, newPublicId) = await UploadImageAsync(file);

            // Trả về cả 2 giá trị
            return (newUrl, newPublicId);
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))
                return;

            var deletionParams = new DeletionParams(publicId);
            var result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Result != "ok" && result.Result != "not found")
                throw new Exception("Lỗi khi xóa ảnh Cloudinary: " + result.Error?.Message);
        }
    }
}
