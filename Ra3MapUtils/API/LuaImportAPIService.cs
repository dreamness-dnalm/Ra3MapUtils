using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;

namespace Ra3MapUtils.API;

/// <summary>
/// Lua 导入方案接口。
/// </summary>
[ApiController]
[Route("api/lua-import")]
public class LuaImportAPIService : ControllerBase
{
    private readonly ILuaImportService _luaImportService = App.Current.Services.GetRequiredService<ILuaImportService>();

    /// <summary>
    /// 导出指定地图的 Lua 导入方案为 JSON 文件。
    /// </summary>
    [HttpPost("scheme/export")]
    [ProducesResponseType(typeof(ApiResponse<LuaImportSchemeOperationResult>), StatusCodes.Status200OK)]
    public ApiResponse<LuaImportSchemeOperationResult> ExportScheme([FromBody] LuaImportSchemeFileRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Map) || string.IsNullOrWhiteSpace(request.JsonPath))
        {
            return ApiResponse<LuaImportSchemeOperationResult>.IllegalArgument();
        }

        try
        {
            return ApiResponse<LuaImportSchemeOperationResult>.Success(
                _luaImportService.ExportMapLuaImportScheme(request.Map, request.JsonPath));
        }
        catch (ArgumentException ex)
        {
            return ApiResponse<LuaImportSchemeOperationResult>.IllegalArgument(ex.Message);
        }
        catch (Exception ex)
        {
            return ApiResponse<LuaImportSchemeOperationResult>.UnknownError(ex.Message);
        }
    }

    /// <summary>
    /// 按 JSON 导入方案向指定地图导入 Lua。
    /// </summary>
    [HttpPost("scheme/import")]
    [ProducesResponseType(typeof(ApiResponse<LuaImportSchemeOperationResult>), StatusCodes.Status200OK)]
    public ApiResponse<LuaImportSchemeOperationResult> ImportScheme([FromBody] LuaImportSchemeFileRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Map) || string.IsNullOrWhiteSpace(request.JsonPath))
        {
            return ApiResponse<LuaImportSchemeOperationResult>.IllegalArgument();
        }

        try
        {
            return ApiResponse<LuaImportSchemeOperationResult>.Success(
                _luaImportService.ImportLuaBySchemeJson(request.Map, request.JsonPath));
        }
        catch (ArgumentException ex)
        {
            return ApiResponse<LuaImportSchemeOperationResult>.IllegalArgument(ex.Message);
        }
        catch (Exception ex)
        {
            return ApiResponse<LuaImportSchemeOperationResult>.UnknownError(ex.Message);
        }
    }
}

/// <summary>
/// Lua 导入方案文件请求。
/// </summary>
public class LuaImportSchemeFileRequest
{
    /// <summary>
    /// 地图名、地图目录路径或 .map 文件路径。
    /// </summary>
    [Required]
    public string Map { get; set; } = string.Empty;

    /// <summary>
    /// Lua 导入方案 JSON 文件路径。
    /// </summary>
    [Required]
    public string JsonPath { get; set; } = string.Empty;
}
