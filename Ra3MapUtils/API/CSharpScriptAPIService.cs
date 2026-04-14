using System.ComponentModel.DataAnnotations;
using Dreamness.ScriptExecutor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ra3MapUtils.API;

/// <summary>
/// C# 脚本执行接口。
/// </summary>
[ApiController]
[Route("api/csharpscript")]
public class CSharpScriptAPIService: ControllerBase
{
    /// <summary>
    /// 执行 C# 脚本源码。
    /// </summary>
    /// <param name="request">
    /// 请求体，包含脚本源码与可选工作目录。
    /// </param>
    /// <returns>
    /// 包装在 <see cref="ApiResponse{T}"/> 中的脚本执行结果。
    /// </returns>
    /// <remarks>
    /// 业务状态码：
    /// 1000：成功。
    /// 1001：未知异常。
    /// 1002：脚本执行失败。
    /// 1003：参数不合法或工作目录不存在。
    /// </remarks>
    [HttpPost("run/code")]
    [ProducesResponseType(typeof(ApiResponse<ScriptExecutionResult>), StatusCodes.Status200OK)]
    public ApiResponse<ScriptExecutionResult> Run([FromBody] CSharpScriptAPIRunRequest request)
    {
        if (request == null || request.Code == null)
        {
            return ApiResponse<ScriptExecutionResult>.IllegalArgument();
        }

        try
        {
            
            // 先创建包含常用包的 ScriptOptions
            var scriptOptions = ScriptExecutor.CreateWithCommonPackages();
        
            // 然后将宿主程序已加载的程序集添加进去
            scriptOptions = ScriptExecutor.WithLoadedAssemblies(
                scriptOptions,
                excludeSystemAssemblies: true,
                excludeDynamicAssemblies: true
            );
        
            var executor = new ScriptExecutor(scriptOptions);
            
            string? workingDirectory = null;

            if (request.WorkingDirectory != null)
            {
                if(!System.IO.Directory.Exists(request.WorkingDirectory))
                {
                    return ApiResponse<ScriptExecutionResult>.IllegalArgument("工作目录不存在");
                }
                workingDirectory = request.WorkingDirectory;
            }
            
            var result = executor.Execute(request.Code, workingDirectory:workingDirectory);
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

    /// <summary>
    /// 执行 C# 脚本文件。
    /// </summary>
    /// <param name="request">
    /// 请求体，包含脚本文件路径与可选工作目录。
    /// 指定 <c>WorkingDirectory</c> 时，<c>FilePath</c> 可为相对路径。
    /// </param>
    /// <returns>
    /// 包装在 <see cref="ApiResponse{T}"/> 中的脚本执行结果。
    /// </returns>
    /// <remarks>
    /// 业务状态码：
    /// 1000：成功。
    /// 1001：未知异常。
    /// 1002：脚本执行失败。
    /// 1003：参数不合法、工作目录不存在或目标文件不存在。
    /// </remarks>
    [HttpPost("run/file")]
    [ProducesResponseType(typeof(ApiResponse<ScriptExecutionResult>), StatusCodes.Status200OK)]
    public ApiResponse<ScriptExecutionResult> RunFile([FromBody] CSharpScriptAPIRunCodeRequest request)
    {
        if (request == null || request.FilePath == null)
        {
            return ApiResponse<ScriptExecutionResult>.IllegalArgument();
        }
        
        try
        {
            string? workingDirectory = null;
            if (request.WorkingDirectory != null)
            {
                if(!System.IO.Directory.Exists(request.WorkingDirectory))
                {
                    return ApiResponse<ScriptExecutionResult>.IllegalArgument("工作目录不存在");
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
                return ApiResponse<ScriptExecutionResult>.IllegalArgument("文件不存在");
            }
            // 读取文件内容
            var code = System.IO.File.ReadAllText(targetFilePath);
            
            // 先创建包含常用包的 ScriptOptions
            var scriptOptions = ScriptExecutor.CreateWithCommonPackages();
        
            // 然后将宿主程序已加载的程序集添加进去
            scriptOptions = ScriptExecutor.WithLoadedAssemblies(
                scriptOptions,
                excludeSystemAssemblies: true,
                excludeDynamicAssemblies: true
            );
        
            var executor = new ScriptExecutor(scriptOptions);
            

            
            var result = executor.Execute(code, workingDirectory:workingDirectory);
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

/// <summary>
/// 执行 C# 源码的请求模型。
/// </summary>
public class CSharpScriptAPIRunRequest
{
    /// <summary>
    /// 待执行的 C# 源码。
    /// </summary>
    [Required]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 可选工作目录。
    /// 传入时必须是已存在的目录。
    /// </summary>
    public string? WorkingDirectory { get; set; }
}

/// <summary>
/// 执行 C# 脚本文件的请求模型。
/// </summary>
public class CSharpScriptAPIRunCodeRequest
{
    /// <summary>
    /// 目标脚本文件路径。可为绝对路径，或相对于 <see cref="WorkingDirectory"/> 的相对路径。
    /// </summary>
    [Required]
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// 用于解析相对路径的可选工作目录。
    /// 传入时必须是已存在的目录。
    /// </summary>
    public string? WorkingDirectory { get; set; }
}
