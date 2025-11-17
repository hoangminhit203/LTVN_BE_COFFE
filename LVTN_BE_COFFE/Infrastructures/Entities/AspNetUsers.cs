using Microsoft.AspNetCore.Identity;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class AspNetUsers : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? AvatarPath { get; set; }

        public bool? Sex { get; set; }

        public DateOnly? Birthday { get; set; }

        public string? JsonUserHasFunctions { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public bool? IsActive { get; set; }

        // Navigation property cho ShippingAddresses
        public ICollection<ShippingAddress> ShippingAddresses { get; set; } = new List<ShippingAddress>();

        // Navigation property cho RefreshTokens
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
