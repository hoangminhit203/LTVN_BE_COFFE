namespace LVTN_BE_COFFE.Domain.VModel
{
    public class BannerCreateVmodel
    {
        public IFormFile? File { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public DateTime CreatedDate { get; set; }
    }
    public class BannerUpdateVmodel
    {
        public string publicId { get; set; } = null!;
        public IFormFile? File { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public DateTime UpdateDate { get; set; }
    }

    public class BannerResponseVmodel
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
