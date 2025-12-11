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
}