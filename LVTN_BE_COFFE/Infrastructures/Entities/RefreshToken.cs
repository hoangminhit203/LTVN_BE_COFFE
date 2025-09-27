namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; } = null!;
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; }
        public string UserId { get; set; } = null!;
        public AspNetUsers User { get; set; } = null!;
    }
}
