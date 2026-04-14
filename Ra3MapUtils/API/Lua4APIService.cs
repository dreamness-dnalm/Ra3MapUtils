using System.ComponentModel.DataAnnotations;
using Dreamness.RA3.Map.Lua.SyntaxChecker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ra3MapUtils.API;

/// <summary>
/// Lua4 语法检查接口。
/// </summary>
[ApiController]
[Route("api/lua4")]
public class Lua4APIService : ControllerBase
{
    /// <summary>
    /// 基于源码文本执行 Lua4 语法检查。
    /// </summary>
    /// <param name="request">包含 Lua 源码的请求体。</param>
    /// <returns>包装在 <see cref="ApiResponse{T}"/> 中的语法检查结果。</returns>
    /// <remarks>
    /// 业务状态码：
    /// 1000：成功。
    /// 1001：未知异常。
    /// 1003：参数不合法。
    /// </remarks>
    [HttpPost("syntax/code")]
    [ProducesResponseType(typeof(ApiResponse<SyntaxCheckResult>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// 基于文件执行 Lua4 语法检查。
    /// </summary>
    /// <param name="request">
    /// 请求体，包含文件路径与可选工作目录。
    /// 指定 <c>WorkingDirectory</c> 时，<c>FilePath</c> 可为相对路径。
    /// </param>
    /// <returns>包装在 <see cref="ApiResponse{T}"/> 中的语法检查结果。</returns>
    /// <remarks>
    /// 业务状态码：
    /// 1000：成功。
    /// 1001：未知异常。
    /// 1003：参数不合法、工作目录不存在或文件不存在。
    /// </remarks>
    [HttpPost("syntax/file")]
    [ProducesResponseType(typeof(ApiResponse<SyntaxCheckResult>), StatusCodes.Status200OK)]
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

/// <summary>
/// Lua 源码语法检查请求模型。
/// </summary>
public class Lua4SyntaxCheckCodeRequest
{
    /// <summary>
    /// 待检查的 Lua4 源码。
    /// </summary>
    [Required]
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// Lua 文件语法检查请求模型。
/// </summary>
public class Lua4SyntaxCheckFileRequest
{
    /// <summary>
    /// 目标文件路径。可为绝对路径，或相对于 <see cref="WorkingDirectory"/> 的相对路径。
    /// </summary>
    [Required]
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// 用于解析相对路径的可选工作目录。
    /// 传入时必须是已存在的目录。
    /// </summary>
    public string? WorkingDirectory { get; set; }
}
