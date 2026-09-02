using System.ComponentModel.DataAnnotations;
using System.IO;
using Dreamness.RA3.Map.Lua.SyntaxChecker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UI.API;

[ApiController]
[Route("api/lua4")]
public class Lua4Controller : ControllerBase
{
    [HttpPost("syntax/code")]
    [ProducesResponseType(typeof(ApiResponse<SyntaxCheckResult>), StatusCodes.Status200OK)]
    public async Task<ApiResponse<SyntaxCheckResult>> CheckSyntaxByCode([FromBody] Lua4SyntaxCheckCodeRequest request)
    {
        if (request?.Code is null)
        {
            return ApiResponse<SyntaxCheckResult>.IllegalArgument();
        }

        try
        {
            var result = await LuaSyntaxChecker.CheckSyntax(request.Code);
            return ApiResponse<SyntaxCheckResult>.Success(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<SyntaxCheckResult>.UnknownError(ex.Message);
        }
    }

    [HttpPost("syntax/file")]
    [ProducesResponseType(typeof(ApiResponse<SyntaxCheckResult>), StatusCodes.Status200OK)]
    public async Task<ApiResponse<SyntaxCheckResult>> CheckSyntaxByFile([FromBody] Lua4SyntaxCheckFileRequest request)
    {
        if (request?.FilePath is null)
        {
            return ApiResponse<SyntaxCheckResult>.IllegalArgument();
        }

        try
        {
            string? workingDirectory = null;
            if (request.WorkingDirectory is not null)
            {
                if (!Directory.Exists(request.WorkingDirectory))
                {
                    return ApiResponse<SyntaxCheckResult>.IllegalArgument("工作目录不存在");
                }

                workingDirectory = request.WorkingDirectory;
            }

            var targetFilePath = request.FilePath;
            if (workingDirectory is not null)
            {
                targetFilePath = Path.Combine(workingDirectory, request.FilePath);
            }

            if (!System.IO.File.Exists(targetFilePath))
            {
                return ApiResponse<SyntaxCheckResult>.IllegalArgument("文件不存在");
            }

            var result = await LuaSyntaxChecker.CheckFileSyntax(targetFilePath);
            return ApiResponse<SyntaxCheckResult>.Success(result);
        }
        catch (Exception ex)
        {
            return ApiResponse<SyntaxCheckResult>.UnknownError(ex.Message);
        }
    }
}

public class Lua4SyntaxCheckCodeRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;
}

public class Lua4SyntaxCheckFileRequest
{
    [Required]
    public string FilePath { get; set; } = string.Empty;

    public string? WorkingDirectory { get; set; }
}
