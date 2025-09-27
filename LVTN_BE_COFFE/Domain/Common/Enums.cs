using System.ComponentModel;

namespace LVTN_BE_COFFE.Domain.Common
{
    public static class Enums
    {
        public static string GetDescription(this Enum en)
        {
            var type = en.GetType();
            var memInfo = type.GetMember(en.ToString());

            if (memInfo?.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs?.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return en.ToString();
        }
        public enum ErrorCodes
        {
            [Description("Success")]
            Success = 0,

            [Description("Bad Request")]
            BadRequest = 1,

            [Description("Invalid Model")]
            InvalidModel = 2,

            [Description("Entity is archived")]
            EntityIsArchived = 3,

            [Description("Internal Server Error")]
            InternalServerError = 4,

            [Description("Data not found")]
            EntityNotFound = 5,

            [Description("Account Invalid")]
            AccountInValid = 6,

            [Description("Account is Exist")]
            AccountExist = 7,

            [Description("Employee not found")]
            EmployeeNotFound = 8,

            [Description("Email not found")]
            EmailNotValid = 9,
        }
    }
}
