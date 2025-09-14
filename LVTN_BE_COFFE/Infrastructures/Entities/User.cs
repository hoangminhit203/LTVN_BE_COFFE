using Microsoft.AspNetCore.Identity;

namespace LVTN_BE_COFFE.Infrastructures.Entities
{
    public class User : IdentityUser
    {
        public string? Initials { get; set; }
    }
}
