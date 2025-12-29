using LVTN_BE_COFFE.Domain.Ultilities;
using LVTN_BE_COFFE.Infrastructures.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LVTN_BE_COFFE.Services.Services
{
    public interface ITokenService
    {
        Task<(string accessToken, string refreshToken)> CreateTokensAsync(AspNetUsers user);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(RefreshToken token);
    }
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AspNetUsers> _userManager;
        private readonly AppDbContext _dbContext;

        public TokenService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<AspNetUsers> userManager,
            AppDbContext dbContext)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<(string accessToken, string refreshToken)> CreateTokensAsync(AspNetUsers user)
        {
            // 1. Tạo danh sách Claims cơ bản (Dùng List để dễ thêm bớt)
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id), // ID người dùng
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID của Token
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Name, user.UserName ?? ""),
        new Claim(ClaimTypes.Email, user.Email ?? "")
    };

            // 2. [QUAN TRỌNG] Lấy Role từ DB và nhét vào Claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                // Key mấu chốt để phân quyền Admin/Customer nằm ở đây
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 3. Lấy thêm các Claim tùy chỉnh khác nếu có (Union code cũ của bạn)
            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            // 4. Tạo Key ký tên (Signing Key)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 5. Cấu hình thông tin Token (Access Token)
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // Đưa danh sách claims đã có Role vào đây
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            // 6. Tạo Refresh Token
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                // Tạo chuỗi ngẫu nhiên an toàn 64 bytes
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                UserId = user.Id,
                //// Nên thêm ngày tạo để dễ quản lý
                //CreatedDate = DateTime.UtcNow,
                //IsRevoked = false // Mặc định là chưa bị thu hồi
            };

            // 7. Lưu Refresh Token vào Database
            _dbContext.RefreshTokens.Add(refreshToken);
            await _dbContext.SaveChangesAsync();

            // 8. Trả về kết quả
            return (accessToken, refreshToken.Token);
        }

        public Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return _dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsRevoked);
        }

        public async Task RevokeRefreshTokenAsync(RefreshToken token)
        {
            token.IsRevoked = true;
            await _dbContext.SaveChangesAsync();
        }
    }
}
