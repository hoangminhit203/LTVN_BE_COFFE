using System.Security.Claims;

namespace LVTN_BE_COFFE.Domain.Common
{
    public class Globals
    {

        private static IHttpContextAccessor? _contextAccessor;
        public Globals(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        private static ClaimsPrincipal? User => _contextAccessor?.HttpContext?.User;
        protected static string GlobalUserId => User?.Identity != null && User.Identity.IsAuthenticated && User.Claims.Count() > 2 ? User.Claims.ToArray()[2].Value : string.Empty;
        protected static string GlobalEmail => User?.Identity != null && User.Identity.IsAuthenticated && User.Claims.Count() > 3 ? User.Claims.ToArray()[3].Value : string.Empty;
        protected static string GlobalUserName => User?.Identity != null && User.Identity.IsAuthenticated && User.Claims.Count() > 4 ? User.Claims.ToArray()[4].Value : string.Empty;
    }
}
