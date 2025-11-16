using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Dreamness.Ra3.Map.Facade.Core;
using Dreamness.Ra3.Map.Facade.Util;
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

    [HttpPost("run/file")]
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

public class CSharpScriptAPIRunRequest
{
    [Required]
    public string Code { get; set; }
    
    public string? WorkingDirectory { get; set; }
}

public class CSharpScriptAPIRunCodeRequest
{
    [Required]
    public string FilePath { get; set; }
    
    public string? WorkingDirectory { get; set; }
}