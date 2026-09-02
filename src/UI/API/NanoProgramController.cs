using Core.NanoPrograms;
using Microsoft.AspNetCore.Mvc;
using UI.Services;

namespace UI.API;

[ApiController]
[Route("api/nanoprogram")]
public sealed class NanoProgramController : ControllerBase
{
    private readonly INanoProgramService _nanoProgramService;
    private readonly ILocalizationService _localization;

    public NanoProgramController(INanoProgramService nanoProgramService, ILocalizationService localization)
    {
        _nanoProgramService = nanoProgramService;
        _localization = localization;
    }

    [HttpGet("list")]
    public ApiResponse<List<NanoProgramModel>> GetNanoPrograms()
    {
        try
        {
            return ApiResponse<List<NanoProgramModel>>.Success(MapForApi(_nanoProgramService.GetNanoPrograms()));
        }
        catch (Exception ex)
        {
            return ApiResponse<List<NanoProgramModel>>.UnknownError(ex.Message);
        }
    }

    [HttpGet("wb_visible_list")]
    public ApiResponse<List<NanoProgramModel>> GetNanoProgramsWbVisible()
    {
        try
        {
            var list = _nanoProgramService.GetNanoPrograms()
                .Where(p => p.IsWbVisible);
            return ApiResponse<List<NanoProgramModel>>.Success(MapForApi(list));
        }
        catch (Exception ex)
        {
            return ApiResponse<List<NanoProgramModel>>.UnknownError(ex.Message);
        }
    }

    [HttpPost("run/{id}")]
    public ApiResponse<NanoProgramExecutionResult> ExecuteNanoProgram(
        [FromRoute] string id,
        [FromBody] Dictionary<string, string>? argumentDict)
    {
        try
        {
            var result = _nanoProgramService.ExecuteNanoProgram(id, argumentDict ?? new Dictionary<string, string>());
            if (result.Success)
            {
                return ApiResponse<NanoProgramExecutionResult>.Success(result);
            }

            return new ApiResponse<NanoProgramExecutionResult>(
                ApiResponseCode.ExecuteCSharpScriptFailed,
                result.Error ?? "execute failed",
                result);
        }
        catch (Exception ex)
        {
            return ApiResponse<NanoProgramExecutionResult>.UnknownError(ex.Message);
        }
    }

    private List<NanoProgramModel> MapForApi(IEnumerable<NanoProgramModel> programs)
    {
        var lang = _localization.CurrentLanguage;
        return programs.Select(p => NanoProgramDisplayResolver.WithResolvedDisplay(p, lang)).ToList();
    }
}
