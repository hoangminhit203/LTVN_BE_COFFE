namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class Banner
    {
        public int Id { get; set; }

        public string PublicId { get; set; } =null!;
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
