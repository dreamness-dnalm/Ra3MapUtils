namespace Ra3MapUtils.API;

/// <summary>
/// 服务 API 统一响应结构。
/// </summary>
/// <typeparam name="T">接口返回的数据类型。</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 业务状态码，定义见 <see cref="ApiResponseCode"/>。
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// 用于排查问题和提示调用方的信息。
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 业务数据。请求失败时可能为 null 或默认值。
    /// </summary>
    public T Data { get; }

    /// <summary>
    /// 创建包含业务数据的响应。
    /// </summary>
    /// <param name="code">业务状态码。</param>
    /// <param name="message">返回消息。</param>
    /// <param name="data">业务数据。</param>
    public ApiResponse(ApiResponseCode code, string message, T data)
    {
        Code = (int)code;
        Message = message;
        Data = data;
    }
    
    /// <summary>
    /// 创建不包含业务数据的响应。
    /// </summary>
    /// <param name="code">业务状态码。</param>
    /// <param name="message">返回消息。</param>
    public  ApiResponse(ApiResponseCode code, string message)
    {
        Code = (int)code;
        Message = message;
        Data = default!;
    }

    /// <summary>
    /// 创建成功响应。
    /// </summary>
    /// <param name="data">业务数据。</param>
    /// <returns>状态码为 <see cref="ApiResponseCode.Success"/> 的响应。</returns>
    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>(ApiResponseCode.Success, "success", data);
    }

    /// <summary>
    /// 创建参数不合法响应。
    /// </summary>
    /// <param name="message">可选的错误消息。</param>
    /// <returns>状态码为 <see cref="ApiResponseCode.IllegalArgument"/> 的响应。</returns>
    public static ApiResponse<T> IllegalArgument(string message = "Illegal argument")
    {
        return new ApiResponse<T>(ApiResponseCode.IllegalArgument, message);
    }

    /// <summary>
    /// 创建未知异常响应。
    /// </summary>
    /// <param name="message">错误详情。</param>
    /// <returns>状态码为 <see cref="ApiResponseCode.UnknownError"/> 的响应。</returns>
    public static ApiResponse<T> UnknownError(string message)
    {
        return new ApiResponse<T>(ApiResponseCode.UnknownError, message);
    }
    
    
    
}

/// <summary>
/// <see cref="ApiResponse{T}.Code"/> 字段使用的统一业务状态码。
/// </summary>
public enum ApiResponseCode
{
    /// <summary>
    /// 请求成功。
    /// </summary>
    Success = 1000,

    /// <summary>
    /// 发生未知运行时异常。
    /// </summary>
    UnknownError = 1001,

    /// <summary>
    /// C# 脚本执行失败。
    /// </summary>
    ExecuteCSharpScriptFailed = 1002,

    /// <summary>
    /// 请求参数不合法。
    /// </summary>
    IllegalArgument = 1003,
}
