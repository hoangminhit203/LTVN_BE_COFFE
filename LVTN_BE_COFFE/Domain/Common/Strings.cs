namespace LVTN_BE_COFFE.Domain.Common
{
    public class Strings
    {
        public const string BaseRoute = "api/[controller]";
        public const string ActionRoute = "api/[controller]/[action]";
        public const string IdRoute = "{id}";
        public const string Bearer = "Bearer";
        public struct StaticRoles
        {
            public const string Admin = "Administrator";
            public const string Els = "Emloyee";
            public const string Mem = "Member";
        }
        public struct JwtClaims
        {
            public const string Id = "id";
            public const string Name = "name";
            public const string Rol = "rol";
            public const string ApiAccess = "api_access";
            public const string Token = "token";
        }
        public struct Messages
        {
            // Success Messages
            public const string RegistrationSuccessful = "Success: Registration successful!";
            public const string EmailConfirmationSuccessful = "Success: Email confirmed successfully!";
            public const string PasswordChangedSuccessfully = "Success: Password changed successfully.";
            public const string PasswordResetRequestSuccess = "Success: Password reset request sent successfully.";
            public const string PasswordResetSuccess = "Success: Password changed successfully.";
            public const string ProfileUpdateSuccess = "Success: Profile updated successfully.";
            public const string EmailSentSuccess = "Success: The email has been successfully sent.";
            public const string LanguageActivated = "Success: {0} is activated";
            public const string OtpResentSuccess = "Success: OTP resent successfully.";
            public const string AvatarUpdateSuccess = "Success: Avatar updated successfully.";

            // Error Messages
            public const string NotFoundEmail = "Error: The email is not found in the system!";
            public const string InActiveAccount = "Error: The account is inactive!";
            public const string InValidPasswword = "Error: The password is incorrect!";
            public const string DuplicateKey = "Error: The key is duplicated!";
            public const string EmailAlreadyExists = "Error: Email already exists!";
            public const string PasswordsDoNotMatch = "Error: Passwords do not match.";
            public const string RegistrationFailed = "Error: Registration failed: {0}";
            public const string EmailConfirmationFailed = "Error: Email confirmation failed!";
            public const string EmailNotFound = "Error: Email not found!";
            public const string SessionExpired = "Error: Session has expired!";
            public const string InvalidToken = "Error: The token is invalid.";
            public const string ExpiredToken = "Error: The token has expired.";
            public const string ProfileUpdateFailed = "Error: Profile update failed.";
            public const string UserNotAuthenticated = "Error: User is not authenticated.";
            public const string InvalidRequestInfo = "Error: Invalid request information.";
            public const string ErrorSendingEmail = "Error: Email Sending Error: {0}";
            public const string InvalidEmailAddress = "Error: Invalid Email";
            public const string DeniedMobile = "Error: Access denied for mobile devices.";
            public const string DeniedBrowser = "Error: Access denied for browser devices.";
            public const string InvalidOtp = "Error: Invalid OTP";
            public const string ExpiredOtp = "Error: Expired OTP";
            public const string LanguageNotFound = "Error: Can't find your language.";
            public const string GeneralError = "Error: {0}";
            public const string EmailAlreadyConfirmed = "Email is already confirmed.";
            public const string FailedToUpdateOtp = "Error: Failed to update OTP.";
            public const string FailedToSendEmail = "Error: Failed to send email.";
            public const string InvalidFile = "Error: Invalid file. Please upload a valid image.";
        }
    }
}
