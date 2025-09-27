using Newtonsoft.Json;

namespace LVTN_BE_COFFE.Domain.Ultilities
{
    public static class Utilities
    {
        public static string MakeExceptionMessage(Exception ex)
        {
            return ex.InnerException == null ? ex.Message : ex.InnerException.Message;
        }
        public static string RandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string RandomNumber(int length)
        {
            var random = new Random();
            const string chars = "01234567899876543211357986420";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static T ConvertModel<T>(object entity)
        {
            string origin = JsonConvert.SerializeObject(entity);
            return JsonConvert.DeserializeObject<T>(origin);
        }
        public static T ConvertModel<T>(string entity)
        {
            return JsonConvert.DeserializeObject<T>(entity);
        }
        public class ValidationException : Exception
        {
            public ValidationException(string message) : base(message) { }
        }
    }
}
