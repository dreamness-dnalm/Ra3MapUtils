using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace Ra3MapUtils.API;

[ApiController]
[Route("api/status")]
public class StatusAPIService
{
    [HttpGet("ping")]
    public ApiResponse<string> Ping()
    {
        return ApiResponse<string>.Success("pong");
    }

    [HttpGet("companion")]
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

public class CompanionPresenceResponse
{
    public bool Running { get; set; }

    public string AppId { get; set; }

    public string DisplayName { get; set; }

    public string Version { get; set; }
}
