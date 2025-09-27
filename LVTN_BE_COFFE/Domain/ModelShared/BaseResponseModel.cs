using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using static LVTN_BE_COFFE.Domain.Common.Enums;

namespace LVTN_BE_COFFE.Domain.ModelShared
{
    [DebuggerStepThrough]
    public class BaseResponseModel
    {
        public ErrorCodes Code { get; set; }
        public string Message { get; set; } = null!;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
