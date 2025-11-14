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
        public static string GenerateTokenJWT(IConfiguration configuration, string userId, string? email, string? userName)
        {
            var claims = new[]
              { new Claim(JwtRegisteredClaimNames.Sub, configuration["Jwt:Subject"] ?? "JWT Login"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                 new Claim(ClaimTypes.NameIdentifier, userId),      // ✅ đổi từ "UserId" sang ClaimTypes.NameIdentifier
                new Claim(ClaimTypes.Email, email ?? ""),
                new Claim(ClaimTypes.Name, userName ?? "")
                };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "4335d179-a729-489c-82ec-b5ccd05a10f5"));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                                configuration["Jwt:Issuer"],
                                configuration["Jwt:Audience"],
                                claims,
                                expires: DateTime.UtcNow.AddMinutes(120),
                                signingCredentials: signIn);

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
