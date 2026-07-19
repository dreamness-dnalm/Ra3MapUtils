namespace Ra3MapUtils.Models;

public enum LuaCompletionItemKind
{
    Keyword,
    Function,
    Method,
    Module,
    Class,
    Field,
    Variable,
    Enum,
    EnumValue,
    ScriptActionId,
    ScriptConditionId,
}

public enum LuaCompletionSourceKind
{
    Keyword,
    NativeApi,
    LuaLibrary,
    UserLibrary,
    CurrentDocument,
}

public sealed record LuaCompletionItem(
    string Name,
    string QualifiedName,
    LuaCompletionItemKind Kind,
    LuaCompletionSourceKind SourceKind,
    string? ContainerName = null,
    string? Signature = null,
    string? Description = null,
    string? ReturnType = null,
    string? SourceFile = null,
    string? IntroducedVersion = null)
{
    public bool IsCallable => Kind is LuaCompletionItemKind.Function or LuaCompletionItemKind.Method;
}

public sealed record LuaCompletionOptions(bool EnableLuaLibrary, string UserCodeLibraryPath);

public sealed record LuaCompletionIndexStatus(
    bool IsIndexing,
    int SymbolCount,
    int IndexedFileCount,
    int FailedFileCount,
    string Message);

public sealed class LuaCompletionIndexChangedEventArgs(LuaCompletionIndexStatus status) : EventArgs
{
    public LuaCompletionIndexStatus Status { get; } = status;
}
