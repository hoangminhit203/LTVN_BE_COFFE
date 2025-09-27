namespace LVTN_BE_COFFE.Domain.Model
{
    public class ResponseResult
    {
        public bool IsSuccess { get; set; } = false;
        public string? Message { get; set; }
        public dynamic? Data { get; set; }
    }

    public class SuccessResponseResult : ResponseResult
    {
        public SuccessResponseResult()
        {
            IsSuccess = true;
        }

        public SuccessResponseResult(dynamic? data)
        {
            IsSuccess = true;
            Data = data;
        }

        public SuccessResponseResult(string? message)
        {
            IsSuccess = true;
            Message = message;
        }

        public SuccessResponseResult(dynamic? data, string? message)
        {
            IsSuccess = true;
            Data = data;
            Message = message;
        }
    }

    public class ErrorResponseResult : ResponseResult
    {

        public ErrorResponseResult()
        {
            IsSuccess = false;
        }
        public ErrorResponseResult(string? message)
        {
            IsSuccess = false;
            Message = message;
        }
    }
}

