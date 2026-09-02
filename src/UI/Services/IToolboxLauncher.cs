using Core.Toolbox;

namespace UI.Services;

public sealed class ToolboxLaunchResult
{
    public bool Success { get; init; }

    public string? Message { get; init; }

    public static ToolboxLaunchResult Ok() => new() { Success = true };

    public static ToolboxLaunchResult Fail(string message) =>
        new() { Success = false, Message = message };
}

public interface IToolboxLauncher
{
    ToolboxLaunchResult Launch(ToolEntry entry);
}
