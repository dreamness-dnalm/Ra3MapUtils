using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ra3MapUtils.API;

/// <summary>
/// 服务状态与伴侣识别信息接口。
/// </summary>
[ApiController]
[Route("api/status")]
public class StatusAPIService
{
    /// <summary>
    /// 轻量存活探针接口。
    /// </summary>
    /// <returns>
    /// 本地服务可用时返回 <c>pong</c>。
    /// </returns>
    /// <remarks>
    /// 业务状态码：
    /// 1000：成功。
    /// </remarks>
    [HttpGet("ping")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public ApiResponse<string> Ping()
    {
        return ApiResponse<string>.Success("pong");
    }

    /// <summary>
    /// 返回用于外部识别的伴侣服务身份与版本信息。
    /// </summary>
    /// <returns>
    /// 包含应用标识、显示名称、版本号等伴侣状态信息。
    /// </returns>
    /// <remarks>
    /// 业务状态码：
    /// 1000：成功。
    /// </remarks>
    [HttpGet("companion")]
    [ProducesResponseType(typeof(ApiResponse<CompanionPresenceResponse>), StatusCodes.Status200OK)]
    public ApiResponse<CompanionPresenceResponse> Companion()
    {
        var asm = Assembly.GetExecutingAssembly();
        var informational = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var version = string.IsNullOrEmpty(informational)
            ? asm.GetName().Version?.ToString() ?? ""
            : informational;

        var data = new CompanionPresenceResponse
        {
            Running = true,
            AppId = "Ra3MapUtils",
            DisplayName = "地编伴侣",
            Version = version
        };
        return ApiResponse<CompanionPresenceResponse>.Success(data);
    }
}

/// <summary>
/// 提供给外部工具的伴侣服务状态模型。
/// </summary>
public class CompanionPresenceResponse
{
    /// <summary>
    /// 服务进程是否正在运行。
    /// </summary>
    public bool Running { get; set; }

    /// <summary>
    /// 稳定的应用标识。
    /// </summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>
    /// 对用户显示的名称。
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// 应用版本号（来自程序集信息版本）。
    /// </summary>
    public string Version { get; set; } = string.Empty;
}
