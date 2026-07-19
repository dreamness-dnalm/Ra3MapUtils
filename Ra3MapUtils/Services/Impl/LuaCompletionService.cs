using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Dreamness.RA3.Map.Parser.Asset.ScriptData;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.Services.Impl;

public sealed partial class LuaCompletionService : ILuaCompletionService
{
    private static readonly Regex QualifiedCallableNameRegex = new(
        @"^[A-Za-z_]\w*(?:[.:][A-Za-z_]\w*)*$",
        RegexOptions.Compiled);

    private static readonly string[] Lua4Keywords =
    [
        "and", "break", "do", "else", "elseif", "end", "for", "function", "if", "in",
        "local", "nil", "not", "or", "repeat", "return", "then", "until", "while",
    ];

    private static readonly HashSet<string> IgnoredDirectoryNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git", ".idea", ".vscode", "bin", "obj",
    };

    private static readonly Lazy<ImmutableArray<LuaCompletionItem>> ScriptActionItems = new(() =>
        CreateScriptIdItems(
            ScriptData.ActionDict.Values,
            "ExecuteAction",
            LuaCompletionItemKind.ScriptActionId));

    private static readonly Lazy<ImmutableArray<LuaCompletionItem>> ScriptConditionItems = new(() =>
        CreateScriptIdItems(
            ScriptData.ConditionDict.Values,
            "EvaluateCondition",
            LuaCompletionItemKind.ScriptConditionId));

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly object _watcherLock = new();
    private readonly List<FileSystemWatcher> _watchers = [];
    private Timer? _refreshTimer;
    private ImmutableArray<LuaCompletionItem> _items = CreateKeywordItems();
    private bool _disposed;

    public event EventHandler<LuaCompletionIndexChangedEventArgs>? IndexChanged;

    public LuaCompletionIndexStatus Status { get; private set; } =
        new(false, 0, 0, 0, "补全索引尚未加载");

    public LuaCompletionOptions GetOptions()
    {
        return new LuaCompletionOptions(
            LuaExecutorBusiness.EnableLuaLibrary,
            LuaExecutorBusiness.UserCodeLibraryPath);
    }

    public async Task UpdateOptionsAsync(
        LuaCompletionOptions options,
        CancellationToken cancellationToken = default)
    {
        LuaExecutorBusiness.EnableLuaLibrary = options.EnableLuaLibrary;
        LuaExecutorBusiness.UserCodeLibraryPath = options.UserCodeLibraryPath.Trim();
        await RefreshAsync(cancellationToken);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _refreshLock.WaitAsync(cancellationToken);

        try
        {
            SetStatus(new LuaCompletionIndexStatus(true, _items.Length, 0, 0, "正在构建 Lua 4 补全索引..."));
            var options = GetOptions();
            var luaLibraryRoot = LuaLibBindingBusiness.LuaLibPath;
            var result = await Task.Run(
                () => BuildIndex(options, luaLibraryRoot, cancellationToken),
                cancellationToken);
            _items = result.Items;
            ConfigureWatchers(result.WatchedDirectories);

            var message = result.FailedFileCount == 0
                ? $"Lua 4 补全：{result.Items.Length} 个符号，{result.IndexedFileCount} 个文件"
                : $"Lua 4 补全：{result.Items.Length} 个符号，{result.FailedFileCount} 个文件解析失败";
            SetStatus(new LuaCompletionIndexStatus(
                false,
                result.Items.Length,
                result.IndexedFileCount,
                result.FailedFileCount,
                message));
        }
        catch (OperationCanceledException)
        {
            SetStatus(Status with { IsIndexing = false, Message = "Lua 4 补全索引已取消" });
            throw;
        }
        catch (Exception ex)
        {
            SetStatus(Status with { IsIndexing = false, Message = "Lua 4 补全索引失败：" + ex.Message });
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public IReadOnlyList<LuaCompletionItem> GetCompletions(string code, int caretOffset)
    {
        if (code == null || caretOffset < 0)
        {
            return [];
        }

        caretOffset = Math.Min(caretOffset, code.Length);
        var stringContext = FindStringContext(code, caretOffset);
        if (stringContext != null)
        {
            if (stringContext.ScriptIdKind == null)
            {
                return [];
            }

            return GetScriptIdItems(stringContext.ScriptIdKind.Value)
                .Where(item => item.Name.StartsWith(stringContext.Prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        var currentItems = LuaCompletionParser.Parse(
            code,
            "当前文档",
            LuaCompletionSourceKind.CurrentDocument,
            null);
        var allItems = _items.AddRange(currentItems);
        var context = CompletionContext.Create(code, caretOffset);
        var receiverType = context.Receiver == null
            ? null
            : InferReceiverType(code, caretOffset, context.Receiver, allItems);

        return FilterCandidates(allItems, context.Receiver, receiverType)
            .Where(item => item.Name.StartsWith(context.Prefix, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => item.Name.StartsWith(context.Prefix, StringComparison.Ordinal))
            .ThenByDescending(item => SourcePriority(item.SourceKind))
            .ThenBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .DistinctBy(item => (item.ContainerName?.ToLowerInvariant(), item.Name.ToLowerInvariant(), item.Kind))
            .Take(200)
            .ToArray();
    }

    public LuaCompletionItem? GetDocumentation(string code, int offset)
    {
        if (string.IsNullOrEmpty(code) || offset < 0)
        {
            return null;
        }

        offset = Math.Min(offset, code.Length);
        var stringContext = FindStringContext(code, offset);
        if (stringContext != null)
        {
            if (stringContext.ScriptIdKind == null)
            {
                return null;
            }

            return GetScriptIdItems(stringContext.ScriptIdKind.Value)
                .FirstOrDefault(item => string.Equals(
                    item.Name,
                    stringContext.Value,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (offset == code.Length || !IsIdentifierCharacter(code[offset]))
        {
            if (offset == 0 || !IsIdentifierCharacter(code[offset - 1]))
            {
                return null;
            }

            offset--;
        }

        var identifierStart = offset;
        while (identifierStart > 0 && IsIdentifierCharacter(code[identifierStart - 1]))
        {
            identifierStart--;
        }

        var identifierEnd = offset + 1;
        while (identifierEnd < code.Length && IsIdentifierCharacter(code[identifierEnd]))
        {
            identifierEnd++;
        }

        var identifier = code[identifierStart..identifierEnd];
        var currentItems = LuaCompletionParser.Parse(
            code,
            "当前文档",
            LuaCompletionSourceKind.CurrentDocument,
            null);
        var allItems = _items.AddRange(currentItems);
        var context = CompletionContext.Create(code, identifierEnd);
        var receiverType = context.Receiver == null
            ? null
            : InferReceiverType(code, identifierEnd, context.Receiver, allItems);

        return FilterCandidates(allItems, context.Receiver, receiverType)
            .Where(item => item.Kind != LuaCompletionItemKind.Keyword)
            .Where(item => !string.IsNullOrWhiteSpace(item.Description))
            .Where(item => string.Equals(item.Name, identifier, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(item => SourcePriority(item.SourceKind))
            .FirstOrDefault();
    }

    private static ImmutableArray<LuaCompletionItem> GetScriptIdItems(LuaCompletionItemKind kind) => kind switch
    {
        LuaCompletionItemKind.ScriptActionId => ScriptActionItems.Value,
        LuaCompletionItemKind.ScriptConditionId => ScriptConditionItems.Value,
        _ => [],
    };

    private static ImmutableArray<LuaCompletionItem> CreateScriptIdItems(
        IEnumerable<ScriptDeclareModel> declarations,
        string functionName,
        LuaCompletionItemKind kind)
    {
        return declarations
            .Select(declaration => CreateScriptIdItem(declaration, functionName, kind))
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .ToImmutableArray();
    }

    private static LuaCompletionItem CreateScriptIdItem(
        ScriptDeclareModel declaration,
        string functionName,
        LuaCompletionItemKind kind)
    {
        var parameterTypes = declaration.Arguments
            .Select(argument => argument.RealType?.Trim())
            .Where(type => !string.IsNullOrWhiteSpace(type))
            .ToArray();
        var parameterSuffix = parameterTypes.Length == 0
            ? ""
            : ", " + string.Join(", ", parameterTypes!);
        var descriptionLines = new List<string>();
        var scriptName = declaration.ScriptName?.Trim();

        if (scriptName?.Contains("Unused or Obsolete", StringComparison.OrdinalIgnoreCase) == true)
        {
            descriptionLines.Add("状态：World Builder 标记为 Unused or Obsolete");
        }

        AddDescriptionLine(descriptionLines, "中文：", declaration.ScriptTrans);
        AddDescriptionLine(descriptionLines, "英文：", scriptName);
        AddDescriptionLine(descriptionLines, "说明：", declaration.ScriptDesc);
        AddDescriptionLine(descriptionLines, "参数模板：", declaration.ScriptArg);
        descriptionLines.Add("编辑器编号：" + declaration.EditorNumber);

        return new LuaCompletionItem(
            declaration.CommandWord,
            functionName + "." + declaration.CommandWord,
            kind,
            LuaCompletionSourceKind.NativeApi,
            functionName,
            $"{functionName}(\"{declaration.CommandWord}\"{parameterSuffix})",
            string.Join(Environment.NewLine, descriptionLines),
            SourceFile: "Dreamness.RA3.Map.Parser/ScriptData");
    }

    private static void AddDescriptionLine(List<string> lines, string label, string? value)
    {
        value = value?.Trim();
        if (!string.IsNullOrWhiteSpace(value))
        {
            lines.Add(label + value);
        }
    }

    private static LuaStringContext? FindStringContext(string code, int offset)
    {
        for (var index = 0; index < code.Length; index++)
        {
            if (code[index] == '-' && index + 1 < code.Length && code[index + 1] == '-')
            {
                if (index + 3 < code.Length && code[index + 2] == '[' && code[index + 3] == '[')
                {
                    var commentEnd = code.IndexOf("]]", index + 4, StringComparison.Ordinal);
                    index = commentEnd < 0 ? code.Length : commentEnd + 1;
                }
                else
                {
                    var commentEnd = code.IndexOf('\n', index + 2);
                    index = commentEnd < 0 ? code.Length : commentEnd;
                }

                continue;
            }

            if (code[index] == '[' && index + 1 < code.Length && code[index + 1] == '[')
            {
                var stringEnd = code.IndexOf("]]", index + 2, StringComparison.Ordinal);
                var longStringContentEnd = stringEnd < 0 ? code.Length : stringEnd;
                if (offset >= index + 2 && offset <= longStringContentEnd)
                {
                    return new LuaStringContext(null, "", "");
                }

                index = stringEnd < 0 ? code.Length : stringEnd + 1;
                continue;
            }

            if (code[index] is not ('\'' or '"'))
            {
                continue;
            }

            var openingQuote = index;
            var quote = code[index];
            var escaped = false;
            index++;
            while (index < code.Length)
            {
                var character = code[index];
                if (escaped)
                {
                    escaped = false;
                }
                else if (character == '\\')
                {
                    escaped = true;
                }
                else if (character == quote)
                {
                    break;
                }

                index++;
            }

            var contentStart = openingQuote + 1;
            var contentEnd = Math.Min(index, code.Length);
            if (offset < contentStart || offset > contentEnd)
            {
                continue;
            }

            var scriptIdKind = GetScriptIdKind(code, openingQuote);
            var prefixEnd = Math.Clamp(offset, contentStart, contentEnd);
            return new LuaStringContext(
                scriptIdKind,
                code[contentStart..prefixEnd],
                code[contentStart..contentEnd]);
        }

        return null;
    }

    private static LuaCompletionItemKind? GetScriptIdKind(string code, int openingQuote)
    {
        var index = openingQuote - 1;
        while (index >= 0 && char.IsWhiteSpace(code[index]))
        {
            index--;
        }

        if (index < 0 || code[index] != '(')
        {
            return null;
        }

        index--;
        while (index >= 0 && char.IsWhiteSpace(code[index]))
        {
            index--;
        }

        var functionEnd = index + 1;
        while (index >= 0 && IsIdentifierCharacter(code[index]))
        {
            index--;
        }

        if (index >= 0 && code[index] is '.' or ':')
        {
            return null;
        }

        var functionName = code[(index + 1)..functionEnd];
        return functionName switch
        {
            "ExecuteAction" => LuaCompletionItemKind.ScriptActionId,
            "EvaluateCondition" => LuaCompletionItemKind.ScriptConditionId,
            _ => null,
        };
    }

    private static IEnumerable<LuaCompletionItem> FilterCandidates(
        IEnumerable<LuaCompletionItem> items,
        string? receiver,
        string? receiverType)
    {
        if (receiver == null)
        {
            return items.Where(item => item.ContainerName == null);
        }

        return items.Where(item =>
            string.Equals(item.ContainerName, receiver, StringComparison.OrdinalIgnoreCase) ||
            receiverType != null && string.Equals(item.ContainerName, receiverType, StringComparison.OrdinalIgnoreCase));
    }

    internal static CompletionIndexBuildResult BuildIndex(
        LuaCompletionOptions options,
        string? luaLibraryRoot,
        CancellationToken cancellationToken)
    {
        var items = CreateKeywordItems().ToList();

        var indexedFileCount = 0;
        var failedFileCount = 0;
        var watchedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(luaLibraryRoot) && Directory.Exists(luaLibraryRoot))
        {
            var nativeApiPath = Path.Combine(luaLibraryRoot, "origin_funcs");
            AddDirectory(
                nativeApiPath,
                LuaCompletionSourceKind.NativeApi,
                excludeFutureApi: true,
                items,
                watchedDirectories,
                ref indexedFileCount,
                ref failedFileCount,
                cancellationToken);

            if (options.EnableLuaLibrary)
            {
                AddDirectory(
                    Path.Combine(luaLibraryRoot, "lib"),
                    LuaCompletionSourceKind.LuaLibrary,
                    excludeFutureApi: false,
                    items,
                    watchedDirectories,
                    ref indexedFileCount,
                    ref failedFileCount,
                    cancellationToken);
            }
        }

        if (!string.IsNullOrWhiteSpace(options.UserCodeLibraryPath))
        {
            AddDirectory(
                options.UserCodeLibraryPath,
                LuaCompletionSourceKind.UserLibrary,
                excludeFutureApi: false,
                items,
                watchedDirectories,
                ref indexedFileCount,
                ref failedFileCount,
                cancellationToken);
        }

        var merged = items
            .OrderByDescending(item => SourcePriority(item.SourceKind))
            .DistinctBy(item => (item.QualifiedName.ToLowerInvariant(), item.Kind))
            .ToImmutableArray();
        return new CompletionIndexBuildResult(
            merged,
            indexedFileCount,
            failedFileCount,
            watchedDirectories.ToArray());
    }

    private static ImmutableArray<LuaCompletionItem> CreateKeywordItems()
    {
        return Lua4Keywords.Select(keyword => new LuaCompletionItem(
            keyword,
            keyword,
            LuaCompletionItemKind.Keyword,
            LuaCompletionSourceKind.Keyword,
            Description: "Lua 4 关键字"))
            .ToImmutableArray();
    }

    private static void AddDirectory(
        string directory,
        LuaCompletionSourceKind sourceKind,
        bool excludeFutureApi,
        List<LuaCompletionItem> items,
        HashSet<string> watchedDirectories,
        ref int indexedFileCount,
        ref int failedFileCount,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        directory = Path.GetFullPath(directory);
        watchedDirectories.Add(directory);

        foreach (var file in EnumerateLuaFiles(directory))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (excludeFutureApi && Path.GetFileNameWithoutExtension(file).EndsWith("_future", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                var code = File.ReadAllText(file, Encoding.UTF8);
                var version = sourceKind == LuaCompletionSourceKind.NativeApi
                    ? ExtractVersion(file)
                    : null;
                items.AddRange(LuaCompletionParser.Parse(code, file, sourceKind, version));
                indexedFileCount++;
            }
            catch
            {
                failedFileCount++;
            }
        }
    }

    private static IEnumerable<string> EnumerateLuaFiles(string root)
    {
        var directories = new Stack<string>();
        directories.Push(root);

        while (directories.Count > 0)
        {
            var directory = directories.Pop();
            IEnumerable<string> files;
            IEnumerable<string> children;

            try
            {
                files = Directory.EnumerateFiles(directory, "*.lua", SearchOption.TopDirectoryOnly).ToArray();
                children = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly).ToArray();
            }
            catch
            {
                continue;
            }

            foreach (var file in files)
            {
                yield return file;
            }

            foreach (var child in children)
            {
                var info = new DirectoryInfo(child);
                if (!IgnoredDirectoryNames.Contains(info.Name) &&
                    !info.Attributes.HasFlag(FileAttributes.Hidden) &&
                    !info.Attributes.HasFlag(FileAttributes.ReparsePoint))
                {
                    directories.Push(child);
                }
            }
        }
    }

    private void ConfigureWatchers(IReadOnlyList<string> directories)
    {
        lock (_watcherLock)
        {
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }

            _watchers.Clear();
            if (_disposed)
            {
                return;
            }

            foreach (var directory in directories.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    var watcher = new FileSystemWatcher(directory, "*.lua")
                    {
                        IncludeSubdirectories = true,
                        NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
                    };
                    watcher.Changed += OnSourceChanged;
                    watcher.Created += OnSourceChanged;
                    watcher.Deleted += OnSourceChanged;
                    watcher.Renamed += OnSourceChanged;
                    watcher.EnableRaisingEvents = true;
                    _watchers.Add(watcher);
                }
                catch (IOException)
                {
                    // The index remains usable when a source directory cannot be watched.
                }
                catch (UnauthorizedAccessException)
                {
                    // The index remains usable when a source directory cannot be watched.
                }
            }
        }
    }

    private void OnSourceChanged(object sender, FileSystemEventArgs e)
    {
        lock (_watcherLock)
        {
            _refreshTimer ??= new Timer(_ => _ = RefreshFromWatcherAsync());
            _refreshTimer.Change(TimeSpan.FromMilliseconds(600), Timeout.InfiniteTimeSpan);
        }
    }

    private async Task RefreshFromWatcherAsync()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            await RefreshAsync();
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private void SetStatus(LuaCompletionIndexStatus status)
    {
        Status = status;
        IndexChanged?.Invoke(this, new LuaCompletionIndexChangedEventArgs(status));
    }

    private static string? InferReceiverType(
        string code,
        int caretOffset,
        string receiver,
        IEnumerable<LuaCompletionItem> items)
    {
        var calledName = GetCalledName(receiver);
        if (calledName != null)
        {
            return FindCallableReturnType(calledName, items);
        }

        var codeBeforeCaret = code[..caretOffset];
        var typedVariablePattern = new Regex(
            $@"---\s*@type\s+([\w.]+)\s*\r?\n\s*(?:local\s+)?{Regex.Escape(receiver)}\s*=",
            RegexOptions.Multiline);
        var typedMatches = typedVariablePattern.Matches(codeBeforeCaret);
        if (typedMatches.Count > 0)
        {
            return typedMatches[^1].Groups[1].Value;
        }

        var assignmentPattern = new Regex(
            $@"(?:local\s+)?{Regex.Escape(receiver)}\s*=\s*([A-Za-z_]\w*(?:[.:][A-Za-z_]\w*)*)\s*\(",
            RegexOptions.Multiline);
        var assignments = assignmentPattern.Matches(codeBeforeCaret);
        if (assignments.Count == 0)
        {
            return null;
        }

        calledName = assignments[^1].Groups[1].Value;
        return FindCallableReturnType(calledName, items);
    }

    private static string? FindCallableReturnType(
        string calledName,
        IEnumerable<LuaCompletionItem> items)
    {
        calledName = calledName.Replace(':', '.');
        return items.FirstOrDefault(item =>
            item.IsCallable &&
            string.Equals(item.QualifiedName.Replace(':', '.'), calledName, StringComparison.OrdinalIgnoreCase))?.ReturnType;
    }

    private static string? GetCalledName(string receiver)
    {
        var trimmedReceiver = receiver.TrimEnd();
        if (trimmedReceiver.Length == 0 || trimmedReceiver[^1] != ')')
        {
            return null;
        }

        var openingParenthesis = FindMatchingOpeningParenthesis(trimmedReceiver, trimmedReceiver.Length - 1);
        if (openingParenthesis < 0)
        {
            return null;
        }

        var calledName = trimmedReceiver[..openingParenthesis].TrimEnd();
        return QualifiedCallableNameRegex.IsMatch(calledName) ? calledName : null;
    }

    private static int FindMatchingOpeningParenthesis(string code, int closingParenthesis)
    {
        var openings = new Stack<int>();
        char? quote = null;
        var escaped = false;

        for (var index = 0; index <= closingParenthesis; index++)
        {
            var character = code[index];
            if (quote != null)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (character == '\\')
                {
                    escaped = true;
                }
                else if (character == quote)
                {
                    quote = null;
                }

                continue;
            }

            if (character is '\'' or '"')
            {
                quote = character;
                continue;
            }

            if (character == '-' && index < closingParenthesis && code[index + 1] == '-')
            {
                var newline = code.IndexOf('\n', index + 2);
                if (newline < 0 || newline > closingParenthesis)
                {
                    return -1;
                }

                index = newline;
                continue;
            }

            if (character == '(')
            {
                openings.Push(index);
            }
            else if (character == ')')
            {
                if (openings.Count == 0)
                {
                    return -1;
                }

                var opening = openings.Pop();
                if (index == closingParenthesis)
                {
                    return opening;
                }
            }
        }

        return -1;
    }

    private static int SourcePriority(LuaCompletionSourceKind sourceKind) => sourceKind switch
    {
        LuaCompletionSourceKind.CurrentDocument => 5,
        LuaCompletionSourceKind.UserLibrary => 4,
        LuaCompletionSourceKind.LuaLibrary => 3,
        LuaCompletionSourceKind.NativeApi => 2,
        _ => 1,
    };

    private static bool IsIdentifierCharacter(char character) =>
        char.IsLetterOrDigit(character) || character == '_';

    private static string? ExtractVersion(string file)
    {
        var match = VersionFileRegex().Match(Path.GetFileNameWithoutExtension(file));
        if (!match.Success)
        {
            return null;
        }

        var raw = match.Groups[1].Value;
        return raw.Length == 4
            ? $"{raw[0]}.{raw[1..]}"
            : raw;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        lock (_watcherLock)
        {
            _refreshTimer?.Dispose();
            _refreshTimer = null;
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }

            _watchers.Clear();
        }

        _refreshLock.Dispose();
    }

    [GeneratedRegex(@"_(\d{4})$")]
    private static partial Regex VersionFileRegex();

    internal sealed record CompletionIndexBuildResult(
        ImmutableArray<LuaCompletionItem> Items,
        int IndexedFileCount,
        int FailedFileCount,
        IReadOnlyList<string> WatchedDirectories);

    private sealed record CompletionContext(string Prefix, string? Receiver)
    {
        public static CompletionContext Create(string code, int caretOffset)
        {
            var start = caretOffset;
            while (start > 0 && IsIdentifierCharacter(code[start - 1]))
            {
                start--;
            }

            var prefix = code[start..caretOffset];
            if (start == 0 || code[start - 1] is not ('.' or ':'))
            {
                return new CompletionContext(prefix, null);
            }

            var receiverEnd = start - 1;
            if (receiverEnd > 0 && code[receiverEnd - 1] == ')')
            {
                var openingParenthesis = FindMatchingOpeningParenthesis(code, receiverEnd - 1);
                if (openingParenthesis >= 0)
                {
                    var callableEnd = openingParenthesis;
                    while (callableEnd > 0 && char.IsWhiteSpace(code[callableEnd - 1]))
                    {
                        callableEnd--;
                    }

                    var callableStart = callableEnd;
                    while (callableStart > 0 &&
                           (IsIdentifierCharacter(code[callableStart - 1]) || code[callableStart - 1] is '.' or ':'))
                    {
                        callableStart--;
                    }

                    if (callableStart < callableEnd)
                    {
                        return new CompletionContext(prefix, code[callableStart..receiverEnd]);
                    }
                }
            }

            var receiverStart = receiverEnd;
            while (receiverStart > 0 && IsIdentifierCharacter(code[receiverStart - 1]))
            {
                receiverStart--;
            }

            return new CompletionContext(prefix, code[receiverStart..receiverEnd]);
        }

    }

    private sealed record LuaStringContext(
        LuaCompletionItemKind? ScriptIdKind,
        string Prefix,
        string Value);
}

internal static partial class LuaCompletionParser
{
    private static readonly Regex AnnotationRegex = new(
        @"^\s*---\s*@(?<name>class|field|type|param|return)\s+(?<value>.+?)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex FunctionRegex = new(
        @"^\s*function\s+(?<name>[A-Za-z_]\w*(?:[.:][A-Za-z_]\w*)*)\s*\((?<params>[^)]*)\)",
        RegexOptions.Compiled);

    private static readonly Regex AssignedFunctionRegex = new(
        @"^\s*(?<local>local\s+)?(?<name>[A-Za-z_]\w*(?:\.[A-Za-z_]\w*)*)\s*=\s*function\s*\((?<params>[^)]*)\)",
        RegexOptions.Compiled);

    private static readonly Regex TableAssignmentRegex = new(
        @"^\s*(?:local\s+)?(?<name>[A-Za-z_]\w*(?:\.[A-Za-z_]\w*)*)\s*=\s*\{",
        RegexOptions.Compiled);

    private static readonly Regex NamedFieldRegex = new(
        @"^\s*(?<name>[A-Za-z_]\w*)\s*=",
        RegexOptions.Compiled);

    private static readonly Regex LocalVariableRegex = new(
        @"^\s*local\s+(?<names>[A-Za-z_]\w*(?:\s*,\s*[A-Za-z_]\w*)*)\s*(?:=|;|$)",
        RegexOptions.Compiled);

    public static ImmutableArray<LuaCompletionItem> Parse(
        string code,
        string sourceFile,
        LuaCompletionSourceKind sourceKind,
        string? introducedVersion)
    {
        var lines = code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var items = new List<LuaCompletionItem>();
        var pendingDocumentation = new List<string>();
        string? pendingReturnType = null;
        string? pendingType = null;
        string? currentClass = null;
        var tableStack = new Stack<(string Name, int BraceDepth)>();
        var braceDepth = 0;

        foreach (var line in lines)
        {
            var annotation = AnnotationRegex.Match(line);
            if (annotation.Success)
            {
                var annotationName = annotation.Groups["name"].Value.ToLowerInvariant();
                var value = annotation.Groups["value"].Value.Trim();
                switch (annotationName)
                {
                    case "class":
                        currentClass = FirstToken(value);
                        items.Add(new LuaCompletionItem(
                            LastSegment(currentClass),
                            currentClass,
                            LuaCompletionItemKind.Class,
                            sourceKind,
                            Description: JoinDocumentation(pendingDocumentation),
                            SourceFile: sourceFile,
                            IntroducedVersion: introducedVersion));
                        pendingDocumentation.Clear();
                        break;
                    case "field" when currentClass != null:
                        var fieldParts = value.Split([' ', '\t'], 3, StringSplitOptions.RemoveEmptyEntries);
                        if (fieldParts.Length >= 2)
                        {
                            items.Add(new LuaCompletionItem(
                                fieldParts[0],
                                currentClass + "." + fieldParts[0],
                                LuaCompletionItemKind.Field,
                                sourceKind,
                                currentClass,
                                Description: fieldParts.Length == 3 ? fieldParts[2] : null,
                                ReturnType: fieldParts[1],
                                SourceFile: sourceFile,
                                IntroducedVersion: introducedVersion));
                        }
                        break;
                    case "return":
                        pendingReturnType = FirstToken(value);
                        pendingDocumentation.Add("返回：" + value);
                        break;
                    case "type":
                        pendingType = FirstToken(value);
                        break;
                    default:
                        pendingDocumentation.Add("@" + annotationName + " " + value);
                        break;
                }

                continue;
            }

            if (line.TrimStart().StartsWith("---", StringComparison.Ordinal))
            {
                pendingDocumentation.Add(line.TrimStart().TrimStart('-').Trim());
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                if (pendingDocumentation.Count > 0 && pendingReturnType == null)
                {
                    pendingDocumentation.Clear();
                }
                continue;
            }

            var functionMatch = FunctionRegex.Match(line);
            var isAssignedFunction = false;
            if (!functionMatch.Success)
            {
                functionMatch = AssignedFunctionRegex.Match(line);
                isAssignedFunction = functionMatch.Success;
            }

            if (functionMatch.Success)
            {
                var functionName = functionMatch.Groups["name"].Value;
                if (isAssignedFunction &&
                    !functionMatch.Groups["local"].Success &&
                    !functionName.Contains('.') &&
                    tableStack.Count > 0)
                {
                    functionName = tableStack.Peek().Name + "." + functionName;
                }

                AddFunction(
                    items,
                    functionName,
                    functionMatch.Groups["params"].Value,
                    pendingDocumentation,
                    pendingReturnType,
                    sourceFile,
                    sourceKind,
                    introducedVersion);
                pendingDocumentation.Clear();
                pendingReturnType = null;
                pendingType = null;
            }
            else
            {
                var tableMatch = TableAssignmentRegex.Match(line);
                if (tableMatch.Success)
                {
                    var tableName = tableMatch.Groups["name"].Value;
                    items.Add(new LuaCompletionItem(
                        LastSegment(tableName),
                        tableName,
                        pendingType == null ? LuaCompletionItemKind.Module : LuaCompletionItemKind.Variable,
                        sourceKind,
                        ContainerName(tableName),
                        ReturnType: pendingType,
                        Description: JoinDocumentation(pendingDocumentation),
                        SourceFile: sourceFile,
                        IntroducedVersion: introducedVersion));
                    tableStack.Push((tableName, braceDepth));
                    pendingDocumentation.Clear();
                    pendingType = null;
                    pendingReturnType = null;
                }
                else
                {
                    var localVariableMatch = LocalVariableRegex.Match(line);
                    if (localVariableMatch.Success)
                    {
                        foreach (var variableName in localVariableMatch.Groups["names"].Value.Split(','))
                        {
                            var name = variableName.Trim();
                            items.Add(new LuaCompletionItem(
                                name,
                                name,
                                LuaCompletionItemKind.Variable,
                                sourceKind,
                                ReturnType: pendingType,
                                SourceFile: sourceFile,
                                IntroducedVersion: introducedVersion));
                        }

                        pendingType = null;
                    }

                    if (tableStack.Count > 0)
                    {
                        var fieldMatch = NamedFieldRegex.Match(line);
                        if (fieldMatch.Success)
                        {
                            var container = tableStack.Peek().Name;
                            var fieldName = fieldMatch.Groups["name"].Value;
                            items.Add(new LuaCompletionItem(
                                fieldName,
                                container + "." + fieldName,
                                LuaCompletionItemKind.Field,
                                sourceKind,
                                container,
                                SourceFile: sourceFile,
                                IntroducedVersion: introducedVersion));
                        }
                    }
                }
            }

            braceDepth += CountCharacterOutsideComment(line, '{');
            braceDepth -= CountCharacterOutsideComment(line, '}');
            while (tableStack.Count > 0 && braceDepth <= tableStack.Peek().BraceDepth)
            {
                tableStack.Pop();
            }
        }

        return items.ToImmutableArray();
    }

    private static void AddFunction(
        List<LuaCompletionItem> items,
        string qualifiedName,
        string parameters,
        List<string> documentation,
        string? returnType,
        string sourceFile,
        LuaCompletionSourceKind sourceKind,
        string? introducedVersion)
    {
        var separatorIndex = Math.Max(qualifiedName.LastIndexOf('.'), qualifiedName.LastIndexOf(':'));
        var container = separatorIndex < 0 ? null : qualifiedName[..separatorIndex];
        var name = separatorIndex < 0 ? qualifiedName : qualifiedName[(separatorIndex + 1)..];
        var kind = qualifiedName.Contains(':') ? LuaCompletionItemKind.Method : LuaCompletionItemKind.Function;
        items.Add(new LuaCompletionItem(
            name,
            qualifiedName,
            kind,
            sourceKind,
            container,
            $"{qualifiedName}({parameters.Trim()})",
            JoinDocumentation(documentation),
            returnType,
            sourceFile,
            introducedVersion));
    }

    private static string FirstToken(string value)
    {
        var index = value.IndexOfAny([' ', '\t']);
        return index < 0 ? value : value[..index];
    }

    private static string LastSegment(string value)
    {
        var index = Math.Max(value.LastIndexOf('.'), value.LastIndexOf(':'));
        return index < 0 ? value : value[(index + 1)..];
    }

    private static string? ContainerName(string value)
    {
        var index = Math.Max(value.LastIndexOf('.'), value.LastIndexOf(':'));
        return index < 0 ? null : value[..index];
    }

    private static string? JoinDocumentation(List<string> lines)
    {
        var content = string.Join(Environment.NewLine, lines.Where(line => !string.IsNullOrWhiteSpace(line)));
        return string.IsNullOrWhiteSpace(content) ? null : content;
    }

    private static int CountCharacterOutsideComment(string line, char character)
    {
        var commentIndex = line.IndexOf("--", StringComparison.Ordinal);
        var code = commentIndex < 0 ? line : line[..commentIndex];
        return code.Count(value => value == character);
    }
}
