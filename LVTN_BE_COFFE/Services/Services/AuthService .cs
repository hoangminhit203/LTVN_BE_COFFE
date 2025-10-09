﻿using LVTN_BE_COFFE.Domain.Common;
using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.Ultilities;
using LVTN_BE_COFFE.Domain.VModels;
using LVTN_BE_COFFE.Infrastructures.Entities;
using LVTN_BE_COFFE.Services.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using static LVTN_BE_COFFE.Domain.Common.Strings;

namespace LVTN_BE_COFFE.Services.Services
{
    public class AuthService : Globals, IAuthService
    {
        private readonly UserManager<AspNetUsers> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Globals _globals;
        private readonly IEmailSenderService _emailSender;
        public AuthService(UserManager<AspNetUsers> userManager, Globals globals, IEmailSenderService emailSender, IConfiguration configuration, IHttpContextAccessor contextAccessor) : base(contextAccessor)
        {
            _userManager = userManager;
            _configuration = configuration;
            _httpContextAccessor = contextAccessor;
            _emailSender = emailSender;
            _globals = globals;
            _emailSender = emailSender;
        }
        public async Task<LoginResponse> Login(LoginVModel model)
        {
            var result = new LoginResponse();
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && user.IsActive == true)
            {
                var hashedPassword = _userManager.PasswordHasher.HashPassword(user, model.Password);
                if (await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    //var roles = await _userManager.GetRolesAsync(user);
                    //result.Identity = GenerateToken.GenerateClaimsIdentity(model.Email, user.Id, user.FirstName + user.LastName, roles.ToList());
                    result.Token = GenerateToken.GenerateTokenJWT(_configuration, user.Id, user.Email, user.UserName);
                    result.IsSuccess = true;
                    result.User = MapEntityToVModel(user);
                }
                else
                {
                    result.Message = Strings.Messages.InValidPasswword;
                    result.IsSuccess = false;
                }
            }
            else if (user != null && user.IsActive == false)
            {
                result.Message = Strings.Messages.InActiveAccount;
                result.IsSuccess = false;
            }
            else
            {
                result.Message = Strings.Messages.NotFoundEmail;
                result.IsSuccess = false;
            }
            return result;
        }

        public async Task<ActionResult<MeVModel>> Me()
        {
            MeVModel result = new MeVModel();
            var user = await _userManager.FindByIdAsync(GlobalUserId);
            if (user != null) result = MapEntityToVModel(user);
            return result;
        }

        public async Task<RegisterResponse> Register(RegisterVModel model)
        {
            var response = new RegisterResponse();

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                response.Message = Strings.Messages.EmailAlreadyExists;
                response.IsSuccess = false;
                return response;
            }

            if (model.Password != model.ConfirmPassword)
            {
                response.Message = Strings.Messages.PasswordsDoNotMatch;
                response.IsSuccess = false;
                return response;
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
                response.Message = string.Format(Strings.Messages.RegistrationFailed,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                response.IsSuccess = false;
                return response;
            }

            var otpCode = OtpGenerator.GenerateOtpCode();
            var otpExpiry = DateTime.UtcNow.AddMinutes(1);

            _httpContextAccessor.HttpContext?.Session?.SetString("OtpCode", otpCode);
            _httpContextAccessor.HttpContext?.Session?.SetString("OtpExpiry", otpExpiry.ToString("o"));

            var scheme = _httpContextAccessor.HttpContext?.Request?.Scheme ?? "http";
            var host = _httpContextAccessor.HttpContext?.Request?.Host.ToString() ?? "localhost";

            var subject = "Your OTP Code";
            var body = $"Your OTP code is {otpCode}. Please don't share it with anyone!";

            var sendMailResult = await _emailSender.SendMailAsync(
                fromEmail: "tranhoangngoc112@gmai.com",
                fromPassWord: "ngoc123456",
                toEmail: newUser.Email,
                sendMailTitle: subject,
                sendMailBody: body
            );

            if (!sendMailResult.IsSuccess)
            {
                response.Message = sendMailResult.Message;
                response.IsSuccess = true;
                return response;
            }

            response.User = MapEntityToVModel(newUser);
            response.Message = Strings.Messages.RegistrationSuccessful;
            response.IsSuccess = true;
            return response;
        }


        public async Task<RegisterResponse> ConfirmEmail(string otp, string email)
        {
            var response = new RegisterResponse();
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                response.Message = Strings.Messages.NotFoundEmail;
                response.IsSuccess = false;
                return response;
            }

            var storedOtp = _httpContextAccessor.HttpContext?.Session?.GetString("OtpCode");
            var storedOtpExpiryString = _httpContextAccessor.HttpContext?.Session?.GetString("OtpExpiry");

            if (string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(storedOtpExpiryString))
            {
                response.Message = Strings.Messages.InvalidRequestInfo;
                response.IsSuccess = false;
                return response;
            }

            var storedOtpExpiry = DateTime.Parse(storedOtpExpiryString, null, System.Globalization.DateTimeStyles.RoundtripKind);

            if (storedOtp != otp || DateTime.UtcNow > storedOtpExpiry)
            {
                response.Message = Strings.Messages.InvalidOtp;
                response.IsSuccess = false;
                return response;
            }

            user.IsActive = true;
            await _userManager.UpdateAsync(user);

            _httpContextAccessor.HttpContext?.Session?.Remove("OtpCode");
            _httpContextAccessor.HttpContext?.Session?.Remove("OtpExpiry");

            response.User = MapEntityToVModel(user);
            response.Message = Strings.Messages.EmailConfirmationSuccessful;
            response.IsSuccess = true;
            return response;
        }


        public async Task<IActionResult> ChangePassword(ChangePasswordVModel model)
        {

            var userId = Globals.GlobalUserId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new NotFoundObjectResult(new ChangePasswordResponse
                {
                    IsSuccess = false,
                    Message = Messages.EmailNotFound
                });
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return new OkObjectResult(new ChangePasswordResponse
                {
                    IsSuccess = true,
                    Message = Messages.PasswordChangedSuccessfully,
                });
            }
            else
            {
                return new BadRequestObjectResult(new ChangePasswordResponse
                {
                    IsSuccess = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                });
            }
        }
        public async Task<RegisterResponse> ForgotPassword(ForgotPasswordVModel model)
        {
            var response = new RegisterResponse();
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || string.IsNullOrEmpty(user.Email))
                {
                    response.Message = Strings.Messages.EmailNotFound;
                    response.IsSuccess = false;
                    return response;
                }

                var otpCode = OtpGenerator.GenerateOtpCode();
                var otpExpiry = DateTime.UtcNow.AddMinutes(30);

                _httpContextAccessor.HttpContext?.Session?.SetString("OtpCode", otpCode);
                _httpContextAccessor.HttpContext?.Session?.SetString("OtpExpiry", otpExpiry.ToString("o"));
                _httpContextAccessor.HttpContext?.Session?.SetString("UserEmail", user.Email);

                var scheme = _httpContextAccessor.HttpContext?.Request?.Scheme ?? "http";
                var host = _httpContextAccessor.HttpContext?.Request?.Host.ToString() ?? "localhost";

                var subject = "Your OTP Code";
                var body = $"Your OTP code is {otpCode}. Please don't share it with anyone!";

                var sendMailResult = await _emailSender.SendMailAsync(
                   fromEmail: "tranhoangngoc112@gmai.com",
                   fromPassWord: "ngoc123456",
                    toEmail: user.Email,
                    sendMailTitle: subject,
                    sendMailBody: body
                );

                if (!sendMailResult.IsSuccess)
                {
                    response.Message = Strings.Messages.InvalidEmailAddress;
                    response.IsSuccess = false;
                    return response;
                }

                response.User = MapEntityToVModel(user);
                response.Message = Strings.Messages.PasswordResetRequestSuccess;
                response.IsSuccess = true;
                return response;
            }
            catch
            {
                response.Message = Strings.Messages.ErrorSendingEmail;
                return response;
            }
        }


        public async Task<RegisterResponse> ResetPassword(ResetPasswordVModel model)
        {
            var response = new RegisterResponse();
            var email = _httpContextAccessor.HttpContext?.Session?.GetString("UserEmail");
            var storedOtp = _httpContextAccessor.HttpContext?.Session?.GetString("OtpCode");
            var storedOtpExpiryString = _httpContextAccessor.HttpContext?.Session?.GetString("OtpExpiry");

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(storedOtp) || string.IsNullOrEmpty(storedOtpExpiryString))
            {
                response.Message = Messages.InvalidRequestInfo;
                response.IsSuccess = false;
                return response;
            }

            var storedOtpExpiry = DateTime.Parse(storedOtpExpiryString, null, System.Globalization.DateTimeStyles.RoundtripKind);

            if (DateTime.UtcNow > storedOtpExpiry)
            {
                response.Message = Messages.ExpiredOtp;
                response.IsSuccess = false;
                return response;
            }

            if (storedOtp != model.Otp)
            {
                response.Message = Messages.InvalidOtp;
                response.IsSuccess = false;
                return response;
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                response.Message = Messages.EmailNotFound;
                response.IsSuccess = false;
                return response;
            }

            var removePasswordResult = await _userManager.RemovePasswordAsync(user);
            if (!removePasswordResult.Succeeded)
            {
                response.Message = string.Join(", ", removePasswordResult.Errors.Select(e => e.Description));
                response.IsSuccess = false;
                return response;
            }

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                response.Message = string.Join(", ", addPasswordResult.Errors.Select(e => e.Description));
                response.IsSuccess = false;
                return response;
            }

            _httpContextAccessor.HttpContext?.Session?.Remove("OtpCode");
            _httpContextAccessor.HttpContext?.Session?.Remove("OtpExpiry");
            _httpContextAccessor.HttpContext?.Session?.Remove("UserEmail");

            response.User = MapEntityToVModel(user);
            response.Message = Messages.PasswordResetSuccess;
            response.IsSuccess = true;
            return response;
        }


        public async Task<RegisterResponse> UpdateProfile(UpdateProfileVModel model)
        {
            var response = new RegisterResponse();
            var userId = Globals.GlobalUserId;

            if (string.IsNullOrEmpty(userId))
            {
                response.Message = Strings.Messages.UserNotAuthenticated;
                response.IsSuccess = false;
                return response;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.Message = Strings.Messages.EmailNotFound;
                response.IsSuccess = false;
                return response;
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Sex = model.Sex;
            user.Birthday = model.Birthday;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                response.Message = Strings.Messages.ProfileUpdateFailed;
                response.IsSuccess = false;
                return response;
            }
            response.User = MapEntityToVModel(user);
            response.Message = Strings.Messages.ProfileUpdateSuccess;
            response.IsSuccess = true;
            return response;
        }


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
                JsonUserHasFunctions = entity.JsonUserHasFunctions != null ? JsonHelper.DeserializeJsonUserHasFunctions(entity.JsonUserHasFunctions) : new List<dynamic>(),
                CreatedBy = entity.CreatedBy,
                CreatedDate = entity.CreatedDate,
                UpdatedBy = entity.UpdatedBy,
                UpdatedDate = entity.UpdatedDate,
                IsActive = entity.IsActive,
            };
        }


    }

}
