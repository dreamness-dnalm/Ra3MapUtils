using Dreamness.ScriptExecutor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.ScriptViews;
using Ra3MapUtils.Services.Interface;

namespace Ra3MapUtils.API;

[ApiController]
[Route("api/nanoprogram")]
public class NanoProgramAPIService
{
    private readonly INanoProgramService _nanoProgramService = App.Current.Services.GetRequiredService<INanoProgramService>();

    [HttpGet("list")]
    public ApiResponse<List<NanoProgramModel>> GetNanoPrograms()
    {
        try
        {
            var nanoProgramModels = _nanoProgramService.GetNanoPrograms();
            return ApiResponse<List<NanoProgramModel>>.Success(nanoProgramModels);
        }catch(Exception ex)
        {
            return ApiResponse<List<NanoProgramModel>>.UnknownError(ex.Message);
        }
    }
    
    [HttpGet("wb_visible_list")]
    public ApiResponse<List<NanoProgramModel>> GetNanoProgramsWbVisible()
    {
        try
        {
            var nanoProgramModels = _nanoProgramService.GetNanoPrograms()
                .Where(p => p.IsWbVisible)
                .ToList();
            return ApiResponse<List<NanoProgramModel>>.Success(nanoProgramModels);
        }catch(Exception ex)
        {
            return ApiResponse<List<NanoProgramModel>>.UnknownError(ex.Message);
        }
    }
    
    [HttpPost("run/{id}")]
    public ApiResponse<ScriptExecutionResult> ExecuteNanoProgram([FromRoute] string id, [FromBody] Dictionary<string, string> argumentDict)
    {
        try
        {
            var result = _nanoProgramService.ExecuteNanoProgram(id, argumentDict);
            if (result.Success)
            {
                return ApiResponse<ScriptExecutionResult>.Success(result);
            }
            else
            {
                MsgDialog.Create(title: "Error", msg: result.Exception.ToString(), width:600, height:400).ShowDialog();
                return new ApiResponse<ScriptExecutionResult>(ApiResponseCode.ExecuteCSharpScriptFailed, result.Error, result);
            }
            
        }catch(Exception ex)
        {
            MsgDialog.Create(title: "Error", msg: ex.Message).ShowDialog();
            return ApiResponse<ScriptExecutionResult>.UnknownError(ex.Message);
        }
    }
    
}