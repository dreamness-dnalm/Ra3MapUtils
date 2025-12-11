using Microsoft.AspNetCore.Mvc;

namespace Ra3MapUtils.API;

[ApiController]
[Route("api/wb/plugin")]
public class WorldBuilderPluginAPIService
{
    [HttpGet("list")]
    public ApiResponse<List<string>> GetPluginList()
    {
        return null;
    }

    // [HttpPost("run")]
    // public ApiResponse<string> RunPlugin([FromBody] WorldBuilderPluginAPIRunRequest request)
    // {
    //     return null;
    // }
}