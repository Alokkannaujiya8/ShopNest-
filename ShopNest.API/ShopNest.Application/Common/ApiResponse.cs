namespace ShopNest.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = [];

    public static ApiResponse<T> SuccessResult(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> FailureResult(List<string> errors, string? message = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };

    public static ApiResponse<T> FailureResult(string error, string? message = null) => new()
    {
        Success = false,
        Message = message,
        Errors = [error]
    };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResult(string? message = null) => new()
    {
        Success = true,
        Message = message
    };

    public static new ApiResponse FailureResult(List<string> errors, string? message = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };

    public static new ApiResponse FailureResult(string error, string? message = null) => new()
    {
        Success = false,
        Message = message,
        Errors = [error]
    };
}
