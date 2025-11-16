namespace Ra3MapUtils.API;

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
    
    public  ApiResponse(ApiResponseCode code, string message)
    {
        Code = (int)code;
        Message = message;
    }

    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>(ApiResponseCode.Success, "success", data);
    }

    public static ApiResponse<T> IllegalArgument(string message = "Illegal argument")
    {
        return new ApiResponse<T>(ApiResponseCode.IllegalArgument, message);
    }

    public static ApiResponse<T> UnknownError(string message)
    {
        return new ApiResponse<T>(ApiResponseCode.UnknownError, message);
    }
    
    
    
}

public enum ApiResponseCode
{
    Success = 1000,
    UnknownError = 1001,
    ExecuteCSharpScriptFailed = 1002,
    IllegalArgument = 1003,
}