namespace LVTN_BE_COFFE.Domain.VModel
{
    public class LoginResponse
    {
        public string Token { get; set; }

        public LoginResponse(string token)
        {
            Token = token;
        }
    }
}
