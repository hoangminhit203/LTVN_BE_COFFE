namespace LVTN_BE_COFFE.Infrastructures.Entities
{

    public enum bannerType
    {
        MainSlider = 0,
        SubCard = 1
    }
    public class Banner
    {
        public int Id { get; set; }

        public string PublicId { get; set; } =null!;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public bannerType Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
