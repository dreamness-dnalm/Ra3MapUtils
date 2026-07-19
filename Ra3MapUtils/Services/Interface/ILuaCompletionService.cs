using Ra3MapUtils.Models;

namespace Ra3MapUtils.Services.Interface;

public interface ILuaCompletionService : IDisposable
{
    event EventHandler<LuaCompletionIndexChangedEventArgs>? IndexChanged;

    LuaCompletionOptions GetOptions();

    LuaCompletionIndexStatus Status { get; }

    Task UpdateOptionsAsync(LuaCompletionOptions options, CancellationToken cancellationToken = default);

    Task RefreshAsync(CancellationToken cancellationToken = default);

    IReadOnlyList<LuaCompletionItem> GetCompletions(string code, int caretOffset);

    LuaCompletionItem? GetDocumentation(string code, int offset);
}
