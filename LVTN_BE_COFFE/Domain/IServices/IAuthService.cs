
using Microsoft.AspNetCore.Mvc;
using LVTN_BE_COFFE.Domain.VModels;


namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginVModel model);
        Task<RegisterResponse> Register(RegisterVModel model);
        Task<RegisterResponse> ConfirmEmail(string token, string email);
        Task<ActionResult<MeVModel>> Me();
        Task<IActionResult> ChangePassword(ChangePasswordVModel model);
        Task<RegisterResponse> ForgotPassword(ForgotPasswordVModel model);
        Task<RegisterResponse> ResetPassword(ResetPasswordVModel model);

        Task<RegisterResponse> UpdateProfile(UpdateProfileVModel model);
    }
}
