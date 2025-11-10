using System.ComponentModel.DataAnnotations;
using Dreamness.ScriptExecutor;
using Microsoft.AspNetCore.Mvc;

namespace Ra3MapUtils.API;

[ApiController]
[Route("api/csharpscript")]
public class CSharpScriptAPIService: ControllerBase
{
    [HttpPost("run/code")]
    public ApiResponse<ScriptExecutionResult> Run([FromBody] CSharpScriptAPIRunRequest request)
    {
        if (request == null || request.Code == null)
        {
            return ApiResponse<ScriptExecutionResult>.IllegalArgument();
        }

        try
        {
            var executor = new ScriptExecutor();
            var result = executor.Execute(request.Code);
            if (result.Success)
            {
                return ApiResponse<ScriptExecutionResult>.Success(result);
            }
            else
            {
                return new ApiResponse<ScriptExecutionResult>(ApiResponseCode.ExecuteCSharpScriptFailed, result.Error, result);
            }
        }
        catch(Exception ex)
        {
            return ApiResponse<ScriptExecutionResult>.UnknownError(ex.Message);
        }
    }

    [HttpPost("run/file")]
    public ApiResponse<ScriptExecutionResult> RunFile([FromBody] CSharpScriptAPIRunCodeRequest request)
    {
        if (request == null || request.FilePath == null)
        {
            return ApiResponse<ScriptExecutionResult>.IllegalArgument();
        }
        
        try
        {
            if (!System.IO.File.Exists(request.FilePath))
            {
                return ApiResponse<ScriptExecutionResult>.IllegalArgument("文件不存在");
            }
            // 读取文件内容
            var code = System.IO.File.ReadAllText(request.FilePath);
            
            var executor = new ScriptExecutor();
            var result = executor.Execute(code);
            if (result.Success)
            {
                return ApiResponse<ScriptExecutionResult>.Success(result);
            }
            else
            {
                return new ApiResponse<ScriptExecutionResult>(ApiResponseCode.ExecuteCSharpScriptFailed, result.Error, result);
            }
        }
        catch(Exception ex)
        {
            return ApiResponse<ScriptExecutionResult>.UnknownError(ex.Message);
        }
    }
    
}

public class CSharpScriptAPIRunRequest
{
    [Required]
    public string Code { get; set; }
}

public class CSharpScriptAPIRunCodeRequest
{
    [Required]
    public string FilePath { get; set; }
}