using Dreamness.ScriptExecutor;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.ScriptViews;
using Ra3MapUtils.Services.Interface;

namespace Ra3MapUtils.API;

/// <summary>
/// 微程序查询与执行接口。
/// </summary>
[ApiController]
[Route("api/nanoprogram")]
public class NanoProgramAPIService
{
    private readonly INanoProgramService _nanoProgramService = App.Current.Services.GetRequiredService<INanoProgramService>();

    /// <summary>
    /// 获取全部微程序列表。
    /// </summary>
    /// <returns>包装在 <see cref="ApiResponse{T}"/> 中的完整微程序列表。</returns>
    /// <remarks>
    /// 业务状态码：
    /// 1000：成功。
    /// 1001：未知异常。
    /// </remarks>
    [HttpGet("list")]
    [ProducesResponseType(typeof(ApiResponse<List<NanoProgramModel>>), StatusCodes.Status200OK)]
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
    
    /// <summary>
    /// 获取在 World Builder 场景可见的微程序列表。
    /// </summary>
    /// <returns>包装在 <see cref="ApiResponse{T}"/> 中的筛选后微程序列表。</returns>
    /// <remarks>
    /// 业务状态码：
    /// 1000：成功。
    /// 1001：未知异常。
    /// </remarks>
    [HttpGet("wb_visible_list")]
    [ProducesResponseType(typeof(ApiResponse<List<NanoProgramModel>>), StatusCodes.Status200OK)]
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
    
    /// <summary>
    /// 按 ID 执行指定微程序。
    /// </summary>
    /// <param name="id">微程序唯一标识。</param>
    /// <param name="argumentDict">传递给微程序的运行时参数字典。</param>
    /// <returns>包装在 <see cref="ApiResponse{T}"/> 中的脚本执行结果。</returns>
    /// <remarks>
    /// 业务状态码：
    /// 1000：成功。
    /// 1001：未知异常。
    /// 1002：微程序执行失败。
    /// </remarks>
    [HttpPost("run/{id}")]
    [ProducesResponseType(typeof(ApiResponse<ScriptExecutionResult>), StatusCodes.Status200OK)]
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
