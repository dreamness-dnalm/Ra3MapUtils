using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UI.Models;
using UI.Services;

namespace UI.API;

[ApiController]
[Route("api/lua-import")]
public class LuaImportController : ControllerBase
{
    private readonly ILuaImportService _luaImportService;

    public LuaImportController(ILuaImportService luaImportService)
    {
        _luaImportService = luaImportService;
    }

    [HttpPost("scheme/export")]
    [ProducesResponseType(typeof(ApiResponse<LuaImportSchemeOperationResult>), StatusCodes.Status200OK)]
    public ApiResponse<LuaImportSchemeOperationResult> ExportScheme([FromBody] LuaImportSchemeFileRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Map) || string.IsNullOrWhiteSpace(request.JsonPath))
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

    [HttpPost("scheme/import")]
    [ProducesResponseType(typeof(ApiResponse<LuaImportSchemeOperationResult>), StatusCodes.Status200OK)]
    public ApiResponse<LuaImportSchemeOperationResult> ImportScheme([FromBody] LuaImportSchemeFileRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Map) || string.IsNullOrWhiteSpace(request.JsonPath))
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

public class LuaImportSchemeFileRequest
{
    [Required]
    public string Map { get; set; } = string.Empty;

    [Required]
    public string JsonPath { get; set; } = string.Empty;
}
