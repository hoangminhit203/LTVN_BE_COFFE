using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModels;
using LVTN_BE_COFFE.Infrastructures.Entities;
using LVTN_BE_COFFE.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace LVTN_BE_COFFE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<AspNetUsers> _signInManager;
        private readonly UserManager<AspNetUsers> _userManager;
        private readonly ITokenService _tokenService;

        public AuthController(
          IAuthService authService,
          UserManager<AspNetUsers> userManager,
          SignInManager<AspNetUsers> signInManager,
          ITokenService tokenService)
        {
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }
        [Authorize]
        [HttpGet("Me")]
        public async Task<ActionResult<MeVModel>> Me()
        {
            var result = await _authService.Me();
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            // 1. Tìm user theo Email
            var user = await _userManager.FindByEmailAsync(model.Email);

            // Nếu không thấy User -> Trả về lỗi chung (để bảo mật)
            if (user == null)
            {
                return Unauthorized("Tài khoản hoặc mật khẩu không chính xác.");
            }

            // 2. Kiểm tra mật khẩu & Trạng thái tài khoản (CheckPasswordSignInAsync)
            // false ở cuối nghĩa là: sai pass thì chưa khóa acc ngay
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.Succeeded)
            {
                // 3. Đăng nhập thành công -> Tạo Token
                var (accessToken, refreshToken) = await _tokenService.CreateTokensAsync(user);

                return Ok(new
                {
                    accessToken = accessToken,
                    refreshToken = refreshToken
                });
            }

            // 4. Xử lý các lỗi cụ thể (nếu cần thiết báo cho user)
            if (result.IsLockedOut)
            {
                return Unauthorized("Tài khoản đã bị khóa do đăng nhập sai nhiều lần.");
            }

            if (result.IsNotAllowed)
            {
                return Unauthorized("Tài khoản chưa được kích hoạt hoặc bị cấm.");
            }

            // Lỗi mặc định (sai pass)
            return Unauthorized("Tài khoản hoặc mật khẩu không chính xác.");
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshRequest model)
        {
            var existingToken = await _tokenService.GetRefreshTokenAsync(model.RefreshToken);
            if (existingToken == null || existingToken.Expires < DateTime.UtcNow) return Unauthorized();

            // revoke old
            await _tokenService.RevokeRefreshTokenAsync(existingToken);

            // create new
            var (accessToken, refreshToken) = await _tokenService.CreateTokensAsync(existingToken.User);
            return Ok(new { accessToken, refreshToken });
        }

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke(RevokeRequest model)
        {
            var token = await _tokenService.GetRefreshTokenAsync(model.RefreshToken);
            if (token == null) return NotFound();
            await _tokenService.RevokeRefreshTokenAsync(token);
            return NoContent();
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            var result = await _authService.Register(model);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            return (IActionResult)await _authService.ChangePassword(model);
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            var result = await _authService.ForgotPassword(model);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            var result = await _authService.ResetPassword(model);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            var result = await _authService.ConfirmEmail(model.Otp, model.Email);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpPut("UpdateProfile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            var result = await _authService.UpdateProfile(model);
            if (result.IsSuccess)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
    public class LoginRequest { public string Email { get; set; } = null!; public string Password { get; set; } = null!; }
    public class RefreshRequest { public string RefreshToken { get; set; } = null!; }
    public class RevokeRequest { public string RefreshToken { get; set; } = null!; }
}
