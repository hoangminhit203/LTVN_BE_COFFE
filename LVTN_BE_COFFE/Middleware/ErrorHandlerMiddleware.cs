using LVTN_BE_COFFE.Domain.Model;
using System.Net;
using System.Text.Json;
namespace LVTN_BE_COFFE.Middleware
{
    public class ErrorHandlerMiddleware
    {

        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                var errCode = HttpStatusCode.InternalServerError;
                switch (error)
                {
                    case AppException e:
                        // custom application error
                        break;
                    case KeyNotFoundException e:
                        // not found error
                        errCode = HttpStatusCode.NotFound;
                        break;
                    default:
                        // unhandled error
                        break;
                }

                response.StatusCode = (int)errCode;
                var result = JsonSerializer.Serialize(new ErrorResponseResult(error.Message));
                await response.WriteAsync(result);
            }
        }
    }
}
