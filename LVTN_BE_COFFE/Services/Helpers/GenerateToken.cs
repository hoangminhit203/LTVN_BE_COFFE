using LVTN_BE_COFFE.Domain.Common;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace LVTN_BE_COFFE.Services.Helpers
{
    public class GenerateToken
    {
        public static string GenerateTokenJWT(IConfiguration configuration, string userId, string? email, string? userName, IList<string> roles = null)
        {
            var claims = new List<Claim>
    {
        // Lưu ý: Key phải khớp với appsettings.json mới (JwtSettings)
        new Claim(JwtRegisteredClaimNames.Sub, configuration["JwtSettings:Subject"] ?? "JWT Login"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.NameIdentifier, userId),
        new Claim(ClaimTypes.Email, email ?? ""),
        new Claim(ClaimTypes.Name, userName ?? "")
    };

            // 3. Logic thêm Role vào Token (QUAN TRỌNG)
            if (roles != null)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            // 4. Lấy Key từ cấu hình mới (JwtSettings)
            var keyString = configuration["JwtSettings:Key"];
            var issuer = configuration["JwtSettings:Issuer"];
            var audience = configuration["JwtSettings:Audience"];

            // Lấy thời gian hết hạn từ config (hoặc mặc định 120 phút)
            var expireMinutesStr = configuration["JwtSettings:AccessTokenExpirationMinutes"];
            double expireMinutes = !string.IsNullOrEmpty(expireMinutesStr) ? double.Parse(expireMinutesStr) : 120;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims, // Truyền List claims đã có role vào đây
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: signIn
            );

            string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenValue;
        }
        public static ClaimsIdentity GenerateClaimsIdentity(string email, string id, string fullName, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(Strings.JwtClaims.Id, id),
                new Claim(Strings.JwtClaims.Rol, Strings.JwtClaims.ApiAccess), // verify by policy
                new Claim(Strings.JwtClaims.Name, fullName)
            };
            // verify by roles from db
            if (roles != null)
            {
                var claimRoles = new List<Claim>();
                roles.ForEach(x =>
                {
                    claimRoles.Add(new Claim(ClaimTypes.Role, x));
                });
                claims.AddRange(claimRoles);
            }
            return new ClaimsIdentity(new GenericIdentity(email, Strings.JwtClaims.Token), claims);
        }
    }
}
