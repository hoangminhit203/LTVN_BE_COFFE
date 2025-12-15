using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Model;
using LVTN_BE_COFFE.Domain.Ultilities;
using LVTN_BE_COFFE.Domain.VModels;
using LVTN_BE_COFFE.Infrastructures.Entities;
using LVTN_BE_COFFE.Services.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static LVTN_BE_COFFE.Domain.Common.Strings;

namespace LVTN_BE_COFFE.Services.Services
{
    public class AuthService : Globals, IAuthService
    {
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<AspNetUsers> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Globals _globals;
        private readonly IEmailSenderService _emailSender;

        public AuthService(
            UserManager<AspNetUsers> userManager,
            Globals globals,
            IEmailSenderService emailSender,
            IConfiguration configuration,
            IHttpContextAccessor contextAccessor,
            IWebHostEnvironment env
        ) : base(contextAccessor)
        {
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = contextAccessor;
            _emailSender = emailSender;
            _globals = globals;
            _env = env;
        }

        #region AUTH

        public async Task<ResponseResult> Login(LoginVModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && user.IsActive == true)
            {
                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var token = GenerateToken.GenerateTokenJWT(
                        _configuration,
                        user.Id,
                        user.Email,
                        user.UserName
                    );

                    var loginResponse = new LoginResponse
                    {
                        Token = token,
                        IsSuccess = true,
                        User = MapEntityToVModel(user)
                    };

                    return new ResponseResult
                    {
                        IsSuccess = true,
                        Message = "Login successful",
                        Data = loginResponse
                    };
                }
                else
                {
                    return new ResponseResult
                    {
                        IsSuccess = false,
                        Message = Messages.InValidPasswword
                    };
                }
            }
            else if (user != null && user.IsActive != true)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.InActiveAccount
                };
            }
            else
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.NotFoundEmail
                };
            }
        }

        public async Task<ResponseResult> Me()
        {
            var user = await _userManager.FindByIdAsync(GlobalUserId);
            
            if (user != null)
            {
                return new ResponseResult
                {
                    IsSuccess = true,
                    Data = MapEntityToVModel(user)
                };
            }

            return new ResponseResult
            {
                IsSuccess = false,
                Message = "User not found"
            };
        }

        #endregion

        #region REGISTER

        public async Task<ResponseResult> Register(RegisterVModel model)
        {
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.EmailAlreadyExists
                };
            }

            if (model.Password != model.ConfirmPassword)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.PasswordsDoNotMatch
                };
            }

            var newUser = new AspNetUsers
            {
                UserName = model.Email,
                Email = model.Email,
                CreatedDate = DateTime.UtcNow,
                IsActive = false
            };

            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (!result.Succeeded)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = string.Format(
                        Messages.RegistrationFailed,
                        string.Join(", ", result.Errors.Select(e => e.Description))
                    )
                };
            }

            try
            {
                var otpCode = OtpGenerator.GenerateOtpCode();
                var otpExpiry = DateTime.UtcNow.AddMinutes(15);

                _httpContextAccessor.HttpContext?.Session?.SetString("OtpCode", otpCode);
                _httpContextAccessor.HttpContext?.Session?.SetString("OtpExpiry", otpExpiry.ToString("o"));

                var subject = "Your OTP Code for Account Activation";
                var body = await GetEmailTemplateAsync(
                    "sendmail.html",
                    new Dictionary<string, string>
                    {
                        { "{{OTP_CODE}}", otpCode },
                        { "{{EXPIRE_MINUTES}}", "15" },
                        { "{{YEAR}}", DateTime.UtcNow.Year.ToString() }
                    }
                );

                var sendMailResult = await _emailSender.SendMailAsync(
                    "tranhoangngoc112@gmail.com",
                    "mffilftdavfmvvyg",
                    newUser.Email,
                    subject,
                    body
                );

                if (!sendMailResult.IsSuccess)
                {
                    return new ResponseResult
                    {
                        IsSuccess = false,
                        Message = $"Registration successful but failed to send email: {sendMailResult.Message}"
                    };
                }

                var registerResponse = new RegisterResponse
                {
                    User = MapEntityToVModel(newUser),
                    Message = "Registration successful! Please check your email for OTP.",
                    IsSuccess = true
                };

                return new ResponseResult
                {
                    IsSuccess = true,
                    Message = "Registration successful! Please check your email for OTP.",
                    Data = registerResponse
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = $"Failed to send email: {ex.Message}"
                };
            }
        }

        #endregion

        #region EMAIL CONFIRMATION

        public async Task<ResponseResult> ConfirmEmail(string otp, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.NotFoundEmail
                };
            }

            var storedOtp = _httpContextAccessor.HttpContext?.Session?.GetString("OtpCode");
            var storedExpiry = _httpContextAccessor.HttpContext?.Session?.GetString("OtpExpiry");

            if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(storedExpiry))
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.InvalidRequestInfo
                };
            }

            var expiry = DateTime.Parse(storedExpiry, null, System.Globalization.DateTimeStyles.RoundtripKind);
            if (storedOtp != otp || DateTime.UtcNow > expiry)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.InvalidOtp
                };
            }

            user.IsActive = true;
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            _httpContextAccessor.HttpContext?.Session?.Remove("OtpCode");
            _httpContextAccessor.HttpContext?.Session?.Remove("OtpExpiry");

            var registerResponse = new RegisterResponse
            {
                User = MapEntityToVModel(user),
                Message = Messages.EmailConfirmationSuccessful,
                IsSuccess = true
            };

            return new ResponseResult
            {
                IsSuccess = true,
                Message = Messages.EmailConfirmationSuccessful,
                Data = registerResponse
            };
        }

        #endregion

        #region MISSING METHODS IMPLEMENTATION

        public async Task<ResponseResult> ChangePassword(ChangePasswordVModel model)
        {
            var user = await _userManager.FindByIdAsync(GlobalUserId);
            if (user == null)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = "User not found"
                };
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.PasswordsDoNotMatch
                };
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            return new ResponseResult
            {
                IsSuccess = true,
                Message = "Password changed successfully"
            };
        }

        public async Task<ResponseResult> ForgotPassword(ForgotPasswordVModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.NotFoundEmail
                };
            }

            try
            {
                var otpCode = OtpGenerator.GenerateOtpCode();
                var otpExpiry = DateTime.UtcNow.AddMinutes(15);

                _httpContextAccessor.HttpContext?.Session?.SetString("ResetOtpCode", otpCode);
                _httpContextAccessor.HttpContext?.Session?.SetString("ResetOtpExpiry", otpExpiry.ToString("o"));
                _httpContextAccessor.HttpContext?.Session?.SetString("ResetEmail", model.Email);

                var subject = "Password Reset OTP";
                var body = await GetEmailTemplateAsync(
                    "sendmail.html",
                    new Dictionary<string, string>
                    {
                        { "{{OTP_CODE}}", otpCode },
                        { "{{EXPIRE_MINUTES}}", "15" },
                        { "{{YEAR}}", DateTime.UtcNow.Year.ToString() }
                    }
                );

                var sendMailResult = await _emailSender.SendMailAsync(
                    "tranhoangngoc112@gmail.com",
                    "mffilftdavfmvvyg",
                    user.Email,
                    subject,
                    body
                );

                if (!sendMailResult.IsSuccess)
                {
                    return new ResponseResult
                    {
                        IsSuccess = false,
                        Message = $"Failed to send email: {sendMailResult.Message}"
                    };
                }

                return new ResponseResult
                {
                    IsSuccess = true,
                    Message = "OTP sent to your email for password reset"
                };
            }
            catch (Exception ex)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = $"Failed to send email: {ex.Message}"
                };
            }
        }

        public async Task<ResponseResult> ResetPassword(ResetPasswordVModel model)
        {
            var storedOtp = _httpContextAccessor.HttpContext?.Session?.GetString("ResetOtpCode");
            var storedExpiry = _httpContextAccessor.HttpContext?.Session?.GetString("ResetOtpExpiry");
            var storedEmail = _httpContextAccessor.HttpContext?.Session?.GetString("ResetEmail");

            if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(storedExpiry) || string.IsNullOrEmpty(storedEmail))
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.InvalidRequestInfo
                };
            }

            var expiry = DateTime.Parse(storedExpiry, null, System.Globalization.DateTimeStyles.RoundtripKind);
            if (storedOtp != model.Otp || DateTime.UtcNow > expiry)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.InvalidOtp
                };
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.PasswordsDoNotMatch
                };
            }

            var user = await _userManager.FindByEmailAsync(storedEmail);
            if (user == null)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = Messages.NotFoundEmail
                };
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (!result.Succeeded)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            _httpContextAccessor.HttpContext?.Session?.Remove("ResetOtpCode");
            _httpContextAccessor.HttpContext?.Session?.Remove("ResetOtpExpiry");
            _httpContextAccessor.HttpContext?.Session?.Remove("ResetEmail");

            return new ResponseResult
            {
                IsSuccess = true,
                Message = "Password reset successfully"
            };
        }

        public async Task<ResponseResult> UpdateProfile(UpdateProfileVModel model)
        {
            var user = await _userManager.FindByIdAsync(GlobalUserId);
            if (user == null)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = "User not found"
                };
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Sex = model.Sex;
            user.Birthday = model.Birthday;
            user.UpdatedDate = DateTime.UtcNow;
            user.UpdatedBy = GlobalUserId;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new ResponseResult
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            return new ResponseResult
            {
                IsSuccess = true,
                Message = "Profile updated successfully",
                Data = MapEntityToVModel(user)
            };
        }

        #endregion

        #region EMAIL TEMPLATE

        private async Task<string> GetEmailTemplateAsync(
            string templateName,
            Dictionary<string, string> replacements
        )
        {
            var templatePath = Path.Combine(
                _env.ContentRootPath,
                "Resources",
                "Template",
                templateName
            );

            if (!File.Exists(templatePath))
                throw new FileNotFoundException($"Email template not found: {templateName}");

            var body = await File.ReadAllTextAsync(templatePath);

            foreach (var item in replacements)
            {
                body = body.Replace(item.Key, item.Value);
            }

            return body;
        }

        #endregion

        #region MAPPING

        private static MeVModel MapEntityToVModel(AspNetUsers entity)
        {
            return new MeVModel
            {
                UserId = entity.Id,
                UserName = entity.UserName,
                Email = entity.Email,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                AvatarPath = entity.AvatarPath,
                Sex = entity.Sex,
                Birthday = entity.Birthday,
                JsonUserHasFunctions = entity.JsonUserHasFunctions != null
                    ? JsonHelper.DeserializeJsonUserHasFunctions(entity.JsonUserHasFunctions)
                    : new List<dynamic>(),
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,
                UpdatedBy = entity.UpdatedBy,
                UpdatedDate = entity.UpdatedDate,
                IsActive = entity.IsActive
            };
        }

        #endregion
    }
}