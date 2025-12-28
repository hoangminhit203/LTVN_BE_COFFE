using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace LVTN_BE_COFFE.Domain.Common
{
    // Cmt: Bỏ static ở class và properties để an toàn cho từng Request
    public class Globals
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public Globals(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        // Cmt: Helper lấy User hiện tại
        protected ClaimsPrincipal? User => _contextAccessor.HttpContext?.User;

        // Cmt: SỬA LỖI: Tìm chính xác theo Key thay vì đoán mò vị trí [2]
        protected string GlobalUserId
        {
            get
            {
                if (User?.Identity?.IsAuthenticated != true) return string.Empty;

                // Cmt: Ưu tiên tìm claim chuẩn NameIdentifier, nếu không thấy thì tìm "sub", "id"
                return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value
                       ?? User.FindFirst("id")?.Value
                       ?? string.Empty;
            }
        }

        protected string GlobalEmail
        {
            get
            {
                if (User?.Identity?.IsAuthenticated != true) return string.Empty;

                return User.FindFirst(ClaimTypes.Email)?.Value
                       ?? User.FindFirst("email")?.Value
                       ?? string.Empty;
            }
        }

        protected string GlobalUserName
        {
            get
            {
                if (User?.Identity?.IsAuthenticated != true) return string.Empty;

                return User.FindFirst(ClaimTypes.Name)?.Value
                       ?? User.FindFirst("unique_name")?.Value
                       ?? string.Empty;
            }
        }
    }
}