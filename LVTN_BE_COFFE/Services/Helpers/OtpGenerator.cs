namespace LVTN_BE_COFFE.Services.Helpers
{
    public class OtpGenerator
    {
        public static string GenerateOtpCode(int length = 6)
        {
            var random = new Random();
            var otpCode = new char[length];
            for (int i = 0; i < length; i++)
            {
                otpCode[i] = (char)random.Next('0', '9' + 1);
            }
            return new string(otpCode);
        }
    }
}
