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
