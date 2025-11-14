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
        [AllowAnonymous]
        [HttpGet("Me")]
        public async Task<ActionResult<MeVModel>> Me()
        {
            return await _authService.Me();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) return Unauthorized();

            var (accessToken, refreshToken) = await _tokenService.CreateTokensAsync(user);
            return Ok(new { accessToken, refreshToken });
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
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            return await _authService.Register(model);
        }
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            return await _authService.ChangePassword(model);
        }
        [HttpPost("ForgotPassword")]
        public async Task<ActionResult<RegisterResponse>> ForgotPassword([FromBody] ForgotPasswordVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            return await _authService.ForgotPassword(model);
        }
        [HttpPost("ResetPassword")]
        public async Task<ActionResult<RegisterResponse>> ResetPassword([FromBody] ResetPasswordVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }
            return await _authService.ResetPassword(model);
        }
        [HttpPost("ConfirmEmail")]
        public async Task<ActionResult<RegisterResponse>> ConfirmEmail([FromBody] ConfirmEmailVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            return await _authService.ConfirmEmail(model.Otp, model.Email);
        }
        [HttpPut("UpdateProfile")]
        [Authorize]
        public async Task<ActionResult<RegisterResponse>> UpdateProfile([FromBody] UpdateProfileVModel model)
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestObjectResult(ModelState);
            }

            return await _authService.UpdateProfile(model);
        }
    }
    public class LoginRequest { public string Email { get; set; } = null!; public string Password { get; set; } = null!; }
    public class RefreshRequest { public string RefreshToken { get; set; } = null!; }
    public class RevokeRequest { public string RefreshToken { get; set; } = null!; }
}
