
using LVTN_BE_COFFE.Domain.Ultilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LVTN_BE_COFFE.Domain.VModels
{
    public class LoginVModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"^[A-Za-z0-9._%+-]+@(?!.*?\.\.)(?!\.)(?!\.\.)[A-Za-z0-9.-]+(?<!\.)\.(?:[A-Za-z]{2,4}(?<!\.)\.)?[A-Za-z]{2,4}$", ErrorMessage = "Email is invalid!")]
        public required string Email { get; set; }
        public required string Password { get; set; } = "123456y";
    }
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public MeVModel User { get; set; }
    }
    public class RegisterVModel : LoginVModel
    {
        public required string ConfirmPassword { get; set; }
    }
    public class RegisterResponse
    {
        public string? Message { get; set; }
        public bool? IsSuccess { get; set; }
        public MeVModel? User { get; set; }
    }
    public class ConfirmEmailVModel
    {
        public required string Otp { get; set; }
        public required string Email { get; set; }
    }
    public class MeVModel
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarPath { get; set; }
        public bool? Sex { get; set; }
        public DateOnly? Birthday { get; set; }
        public string? Address { get; set; }
        public dynamic? JsonUserHasFunctions { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsActive { get; set; }
    }
    public class ChangePasswordVModel
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }
    public class ChangePasswordResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
    public class ForgotPasswordVModel
    {
        public required string Email { get; set; }
    }

    public class ResetPasswordVModel
    {
        public required string Otp { get; set; }

        public required string NewPassword { get; set; }

        public required string ConfirmPassword { get; set; }
    }
    public class UpdateProfileVModel
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Address { get; set; }
        public bool? Sex { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? Birthday { get; set; }
    }
}
