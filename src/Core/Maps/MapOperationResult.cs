namespace Core.Maps;

public sealed class MapOperationResult
{
    public bool Succeeded { get; init; }

    public string? ErrorMessage { get; init; }

    public string? OutputPath { get; init; }

    public static MapOperationResult Ok(string? outputPath = null) => new()
    {
        Succeeded = true,
        OutputPath = outputPath,
    };

    public static MapOperationResult Fail(string errorMessage) => new()
    {
        Succeeded = false,
        ErrorMessage = errorMessage,
    };
}

public sealed class MapBatchDeleteResult
{
    public int SuccessCount { get; init; }

    public IReadOnlyList<string> Failures { get; init; } = Array.Empty<string>();
}
