using System.ComponentModel.DataAnnotations;
using Dreamness.RA3.Map.Lua.SyntaxChecker;
using Microsoft.AspNetCore.Mvc;

namespace Ra3MapUtils.API;

[ApiController]
[Route("api/lua4")]
public class Lua4APIService : ControllerBase
{
    [HttpPost("syntax/code")]
    public async Task<ApiResponse<SyntaxCheckResult>> CheckSyntaxByCode([FromBody] Lua4SyntaxCheckCodeRequest request)
    {
        if (request == null || request.Code == null)
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
    public async Task<ApiResponse<SyntaxCheckResult>> CheckSyntaxByFile([FromBody] Lua4SyntaxCheckFileRequest request)
    {
        if (request == null || request.FilePath == null)
        {
            return ApiResponse<SyntaxCheckResult>.IllegalArgument();
        }

        try
        {
            string? workingDirectory = null;
            if (request.WorkingDirectory != null)
            {
                if (!System.IO.Directory.Exists(request.WorkingDirectory))
                {
                    return ApiResponse<SyntaxCheckResult>.IllegalArgument("工作目录不存在");
                }

                workingDirectory = request.WorkingDirectory;
            }

            var targetFilePath = request.FilePath;
            if (workingDirectory != null)
            {
                targetFilePath = System.IO.Path.Combine(workingDirectory, request.FilePath);
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
    public string Code { get; set; }
}

public class Lua4SyntaxCheckFileRequest
{
    [Required]
    public string FilePath { get; set; }

    public string? WorkingDirectory { get; set; }
}
