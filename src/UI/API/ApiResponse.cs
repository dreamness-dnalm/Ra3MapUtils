namespace UI.API;

/// <summary>
/// 服务 API 统一响应结构（与旧版兼容的业务码）。
/// </summary>
public class ApiResponse<T>
{
    public int Code { get; }

    public string Message { get; }

    public T Data { get; }

    public ApiResponse(ApiResponseCode code, string message, T data)
    {
        Code = (int)code;
        Message = message;
        Data = data;
    }

    public ApiResponse(ApiResponseCode code, string message)
    {
        Code = (int)code;
        Message = message;
        Data = default!;
    }

    public static ApiResponse<T> Success(T data) =>
        new(ApiResponseCode.Success, "success", data);

    public static ApiResponse<T> IllegalArgument(string message = "Illegal argument") =>
        new(ApiResponseCode.IllegalArgument, message);

    public static ApiResponse<T> UnknownError(string message) =>
        new(ApiResponseCode.UnknownError, message);
}

public enum ApiResponseCode
{
    Success = 1000,
    UnknownError = 1001,
    ExecuteCSharpScriptFailed = 1002,
    IllegalArgument = 1003,
}
