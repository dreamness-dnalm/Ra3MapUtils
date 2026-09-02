using System.ComponentModel.DataAnnotations;
using System.IO;
using Dreamness.ScriptExecutor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UI.API;

[ApiController]
[Route("api/csharpscript")]
public class CSharpScriptController : ControllerBase
{
    [HttpPost("run/code")]
    [ProducesResponseType(typeof(ApiResponse<ScriptExecutionResult>), StatusCodes.Status200OK)]
    public ApiResponse<ScriptExecutionResult> Run([FromBody] CSharpScriptRunCodeRequest request)
    {
        if (request?.Code is null)
        {
            return ApiResponse<ScriptExecutionResult>.IllegalArgument();
        }

        try
        {
            var scriptOptions = ScriptExecutor.CreateWithCommonPackages();
            scriptOptions = ScriptExecutor.WithLoadedAssemblies(
                scriptOptions,
                excludeSystemAssemblies: true,
                excludeDynamicAssemblies: true);

            var executor = new ScriptExecutor(scriptOptions);
            string? workingDirectory = null;
            if (request.WorkingDirectory is not null)
            {
                if (!Directory.Exists(request.WorkingDirectory))
                {
                    return ApiResponse<ScriptExecutionResult>.IllegalArgument("工作目录不存在");
                }

                workingDirectory = request.WorkingDirectory;
            }

            var result = executor.Execute(request.Code, workingDirectory: workingDirectory);
            return result.Success
                ? ApiResponse<ScriptExecutionResult>.Success(result)
                : new ApiResponse<ScriptExecutionResult>(ApiResponseCode.ExecuteCSharpScriptFailed, result.Error, result);
        }
        catch (Exception ex)
        {
            return ApiResponse<ScriptExecutionResult>.UnknownError(ex.Message);
        }
    }

    [HttpPost("run/file")]
    [ProducesResponseType(typeof(ApiResponse<ScriptExecutionResult>), StatusCodes.Status200OK)]
    public ApiResponse<ScriptExecutionResult> RunFile([FromBody] CSharpScriptRunFileRequest request)
    {
        if (request?.FilePath is null)
        {
            return ApiResponse<ScriptExecutionResult>.IllegalArgument();
        }

        try
        {
            string? workingDirectory = null;
            if (request.WorkingDirectory is not null)
            {
                if (!Directory.Exists(request.WorkingDirectory))
                {
                    return ApiResponse<ScriptExecutionResult>.IllegalArgument("工作目录不存在");
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
                return ApiResponse<ScriptExecutionResult>.IllegalArgument("文件不存在");
            }

            var code = System.IO.File.ReadAllText(targetFilePath);
            var scriptOptions = ScriptExecutor.CreateWithCommonPackages();
            scriptOptions = ScriptExecutor.WithLoadedAssemblies(
                scriptOptions,
                excludeSystemAssemblies: true,
                excludeDynamicAssemblies: true);

            var executor = new ScriptExecutor(scriptOptions);
            var result = executor.Execute(code, workingDirectory: workingDirectory);
            return result.Success
                ? ApiResponse<ScriptExecutionResult>.Success(result)
                : new ApiResponse<ScriptExecutionResult>(ApiResponseCode.ExecuteCSharpScriptFailed, result.Error, result);
        }
        catch (Exception ex)
        {
            return ApiResponse<ScriptExecutionResult>.UnknownError(ex.Message);
        }
    }
}

public class CSharpScriptRunCodeRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;

    public string? WorkingDirectory { get; set; }
}

public class CSharpScriptRunFileRequest
{
    [Required]
    public string FilePath { get; set; } = string.Empty;

    public string? WorkingDirectory { get; set; }
}
