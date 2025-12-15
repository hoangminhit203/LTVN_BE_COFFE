using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.VModels;

namespace LVTN_BE_COFFE.Domain.IServices
{
    public interface IAuthService
    {
        Task<ResponseResult> Login(LoginVModel model);
        Task<ResponseResult> Register(RegisterVModel model);
        Task<ResponseResult> ConfirmEmail(string otp, string email);
        Task<ResponseResult> Me();
        Task<ResponseResult> ChangePassword(ChangePasswordVModel model);
        Task<ResponseResult> ForgotPassword(ForgotPasswordVModel model);
        Task<ResponseResult> ResetPassword(ResetPasswordVModel model);
        Task<ResponseResult> UpdateProfile(UpdateProfileVModel model);
    }
}
