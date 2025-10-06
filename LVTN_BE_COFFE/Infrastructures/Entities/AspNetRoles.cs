using Microsoft.AspNetCore.Identity;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class AspNetRoles : IdentityRole
    {
        public string? JsonRoleHasFunctions { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public bool? IsActive { get; set; }
    }
}
