using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UI.API;

[ApiController]
[Route("api/status")]
public class StatusController : ControllerBase
{
    [HttpGet("ping")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public ApiResponse<string> Ping() => ApiResponse<string>.Success("pong");

    [HttpGet("companion")]
    [ProducesResponseType(typeof(ApiResponse<CompanionPresenceResponse>), StatusCodes.Status200OK)]
    public ApiResponse<CompanionPresenceResponse> Companion()
    {
        var asm = Assembly.GetExecutingAssembly();
        var informational = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        var version = string.IsNullOrEmpty(informational)
            ? asm.GetName().Version?.ToString() ?? ""
            : informational;

        return ApiResponse<CompanionPresenceResponse>.Success(new CompanionPresenceResponse
        {
            Running = true,
            AppId = "Ra3MapUtils",
            DisplayName = "地编伴侣",
            Version = version,
        });
    }
}

public class CompanionPresenceResponse
{
    public bool Running { get; set; }

    public string AppId { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;
}
