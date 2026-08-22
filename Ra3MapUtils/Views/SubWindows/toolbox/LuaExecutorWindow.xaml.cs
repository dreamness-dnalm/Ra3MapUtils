using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Ra3MapUtils.Models;
using Ra3MapUtils.Utils;
using Ra3MapUtils.ViewModels.toolbox;
using Wpf.Ui.Controls;
using System.Xml;

namespace Ra3MapUtils.Views.SubWindows.toolbox;

public partial class LuaExecutorWindow : FluentWindow
{
    private CompletionWindow? _completionWindow;
    private System.Windows.Controls.ToolTip? _hoverToolTip;
    private bool _isScriptIdCompletion;
    private char? _scriptIdQuote;
    private bool _updatingEditor;

    public LuaExecutorWindowViewModel _luaExecutorWindowViewModel
        => (LuaExecutorWindowViewModel)DataContext;

    public LuaExecutorWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<LuaExecutorWindowViewModel>();
        InitializeComponent();

        InitializeEditor();
        Loaded += OnLoaded;
        CodeEditor.TextChanged += OnEditorTextChanged;
        CodeEditor.TextArea.TextEntered += OnTextEntered;
        CodeEditor.TextArea.TextEntering += OnTextEntering;
        CodeEditor.PreviewKeyDown += OnEditorPreviewKeyDown;
        CodeEditor.MouseHover += OnEditorMouseHover;
        CodeEditor.MouseHoverStopped += OnEditorMouseHoverStopped;
        _luaExecutorWindowViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    protected override void OnClosed(EventArgs e)
    {
        Loaded -= OnLoaded;
        CodeEditor.TextChanged -= OnEditorTextChanged;
        CodeEditor.TextArea.TextEntered -= OnTextEntered;
        CodeEditor.TextArea.TextEntering -= OnTextEntering;
        CodeEditor.PreviewKeyDown -= OnEditorPreviewKeyDown;
        CodeEditor.MouseHover -= OnEditorMouseHover;
        CodeEditor.MouseHoverStopped -= OnEditorMouseHoverStopped;
        _luaExecutorWindowViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _completionWindow?.Close();
        _completionWindow = null;
        CloseHoverToolTip();
        _luaExecutorWindowViewModel.Dispose();
        base.OnClosed(e);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        SetEditorText(_luaExecutorWindowViewModel.LuaCode);
        await _luaExecutorWindowViewModel.InitializeCompletionAsync();
    }

    private void InitializeEditor()
    {
        using var stream = EmbeddedResourcesUtil.GetEmbeddedResourceStream("Ra3MapUtils.lua4.xshd");
        using var reader = XmlReader.Create(stream);
        CodeEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        CodeEditor.Options.ConvertTabsToSpaces = true;
        CodeEditor.Options.IndentationSize = 4;
    }

    private void OnEditorTextChanged(object? sender, EventArgs e)
    {
        CloseHoverToolTip();
        if (!_updatingEditor)
        {
            _luaExecutorWindowViewModel.LuaCode = CodeEditor.Text;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LuaExecutorWindowViewModel.LuaCode) &&
            CodeEditor.Text != _luaExecutorWindowViewModel.LuaCode)
        {
            SetEditorText(_luaExecutorWindowViewModel.LuaCode);
        }
    }

    private void SetEditorText(string text)
    {
        _updatingEditor = true;
        CodeEditor.Text = text;
        _updatingEditor = false;
    }

    private void OnEditorPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if ((e.Key is Key.Oem2 or Key.Divide) && Keyboard.Modifiers == ModifierKeys.Control)
        {
            ToggleLineComments();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Space && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            ShowCompletionWindow(force: true);
            e.Handled = true;
        }
    }

    private void ToggleLineComments()
    {
        var document = CodeEditor.Document;
        var hasSelection = CodeEditor.SelectionLength > 0;
        var selectionStart = hasSelection ? CodeEditor.SelectionStart : CodeEditor.CaretOffset;
        var selectionEnd = hasSelection
            ? CodeEditor.SelectionStart + CodeEditor.SelectionLength
            : CodeEditor.CaretOffset;
        var firstLine = document.GetLineByOffset(selectionStart);
        var lastLine = document.GetLineByOffset(selectionEnd);

        // A selection ending at the start of the next line should not affect that line.
        if (hasSelection && selectionEnd == lastLine.Offset && lastLine.LineNumber > firstLine.LineNumber)
        {
            lastLine = document.GetLineByNumber(lastLine.LineNumber - 1);
        }

        var firstLineNumber = firstLine.LineNumber;
        var lastLineNumber = lastLine.LineNumber;
        var caretLineNumber = document.GetLineByOffset(CodeEditor.CaretOffset).LineNumber;
        var caretColumn = CodeEditor.CaretOffset - document.GetLineByNumber(caretLineNumber).Offset;
        var adjustedCaretColumn = caretColumn;
        var lineTexts = Enumerable.Range(firstLineNumber, lastLineNumber - firstLineNumber + 1)
            .Select(lineNumber => document.GetText(document.GetLineByNumber(lineNumber)))
            .ToArray();
        var nonEmptyLines = lineTexts.Where(text => !string.IsNullOrWhiteSpace(text)).ToArray();
        var shouldUncomment = nonEmptyLines.Length > 0 &&
                              nonEmptyLines.All(text => text[GetIndentationLength(text)..].StartsWith("--"));

        document.BeginUpdate();
        try
        {
            for (var lineNumber = lastLineNumber; lineNumber >= firstLineNumber; lineNumber--)
            {
                var line = document.GetLineByNumber(lineNumber);
                var text = document.GetText(line);
                if (string.IsNullOrWhiteSpace(text) && firstLineNumber != lastLineNumber)
                {
                    continue;
                }

                var indentationLength = GetIndentationLength(text);
                if (shouldUncomment)
                {
                    var uncommentOffset = line.Offset + indentationLength;
                    var removeLength = 2;
                    if (text.Length > indentationLength + 2 && text[indentationLength + 2] == ' ')
                    {
                        removeLength++;
                    }

                    document.Remove(uncommentOffset, removeLength);
                    if (lineNumber == caretLineNumber && caretColumn >= indentationLength)
                    {
                        adjustedCaretColumn = Math.Max(indentationLength, caretColumn - removeLength);
                    }
                }
                else
                {
                    document.Insert(line.Offset + indentationLength, "-- ");
                    if (lineNumber == caretLineNumber && caretColumn >= indentationLength)
                    {
                        adjustedCaretColumn = caretColumn + 3;
                    }
                }
            }
        }
        finally
        {
            document.EndUpdate();
        }

        if (hasSelection)
        {
            firstLine = document.GetLineByNumber(firstLineNumber);
            lastLine = document.GetLineByNumber(lastLineNumber);
            CodeEditor.Select(firstLine.Offset, lastLine.EndOffset - firstLine.Offset);
        }
        else
        {
            var caretLine = document.GetLineByNumber(caretLineNumber);
            CodeEditor.CaretOffset = caretLine.Offset + Math.Min(adjustedCaretColumn, caretLine.Length);
        }

        CodeEditor.Focus();
    }

    private static int GetIndentationLength(string text)
    {
        var indentationLength = 0;
        while (indentationLength < text.Length && text[indentationLength] is ' ' or '\t')
        {
            indentationLength++;
        }

        return indentationLength;
    }

    private void OnTextEntered(object sender, TextCompositionEventArgs e)
    {
        if (e.Text is "." or ":")
        {
            ShowCompletionWindow(force: true);
            return;
        }

        if (e.Text is "'" or "\"")
        {
            ShowCompletionWindow(force: true, scriptIdOnly: true);
            return;
        }

        if (_completionWindow == null &&
            e.Text.Length == 1 && (char.IsLetterOrDigit(e.Text[0]) || e.Text[0] == '_') &&
            GetCurrentPrefix().Length >= 2)
        {
            ShowCompletionWindow(force: false);
        }
    }

    private void OnTextEntering(object sender, TextCompositionEventArgs e)
    {
        if (_completionWindow == null || string.IsNullOrEmpty(e.Text))
        {
            return;
        }

        var character = e.Text[0];
        if (_isScriptIdCompletion && _scriptIdQuote == character)
        {
            _completionWindow.Close();
            return;
        }

        if (!char.IsLetterOrDigit(character) && character != '_')
        {
            _completionWindow.CompletionList.RequestInsertion(e);
        }
    }

    private void ShowCompletionWindow(bool force, bool scriptIdOnly = false)
    {
        var prefixLength = GetCurrentPrefix().Length;
        var items = _luaExecutorWindowViewModel.GetCompletionItems(CodeEditor.Text, CodeEditor.CaretOffset);
        if (scriptIdOnly && !items.Any(item => IsScriptId(item.Kind)))
        {
            return;
        }

        if (items.Count == 0 && !force)
        {
            return;
        }

        _completionWindow?.Close();
        var isScriptIdCompletion = items.Count > 0 && items.All(item => IsScriptId(item.Kind));
        var completionWindow = new CompletionWindow(CodeEditor.TextArea)
        {
            SizeToContent = SizeToContent.Manual,
            Width = isScriptIdCompletion ? 520 : 440,
            Height = 292,
            MinWidth = 380,
            MaxWidth = 560,
            MaxHeight = 320,
            StartOffset = CodeEditor.CaretOffset - prefixLength,
        };
        ConfigureCompletionList(completionWindow);
        _completionWindow = completionWindow;

        foreach (var item in items)
        {
            completionWindow.CompletionList.CompletionData.Add(new LuaCompletionData(item));
        }

        if (completionWindow.CompletionList.CompletionData.Count == 0)
        {
            completionWindow.Close();
            _completionWindow = null;
            ResetCompletionMode();
            return;
        }

        _isScriptIdCompletion = isScriptIdCompletion;
        _scriptIdQuote = null;
        if (_isScriptIdCompletion && completionWindow.StartOffset > 0)
        {
            var quote = CodeEditor.Document.GetCharAt(completionWindow.StartOffset - 1);
            if (quote is '\'' or '"')
            {
                _scriptIdQuote = quote;
            }
        }

        completionWindow.Closed += (_, _) =>
        {
            if (ReferenceEquals(_completionWindow, completionWindow))
            {
                _completionWindow = null;
                ResetCompletionMode();
            }
        };
        completionWindow.Show();
    }

    private static void ConfigureCompletionList(CompletionWindow completionWindow)
    {
        var listBox = completionWindow.CompletionList.ListBox;
        listBox.FontFamily = new FontFamily("Consolas");
        listBox.FontSize = 13;
        listBox.Padding = new Thickness(0, 4, 0, 4);
        System.Windows.Controls.ScrollViewer.SetHorizontalScrollBarVisibility(
            listBox,
            System.Windows.Controls.ScrollBarVisibility.Disabled);

        var itemStyle = new Style(typeof(System.Windows.Controls.ListBoxItem));
        itemStyle.Setters.Add(new Setter(
            System.Windows.Controls.Control.PaddingProperty,
            new Thickness(10, 3, 8, 3)));
        itemStyle.Setters.Add(new Setter(
            System.Windows.Controls.Control.MinHeightProperty,
            27d));
        itemStyle.Setters.Add(new Setter(
            System.Windows.Controls.Control.HorizontalContentAlignmentProperty,
            HorizontalAlignment.Stretch));
        listBox.ItemContainerStyle = itemStyle;
    }

    private void ResetCompletionMode()
    {
        _isScriptIdCompletion = false;
        _scriptIdQuote = null;
    }

    private static bool IsScriptId(LuaCompletionItemKind kind) =>
        kind is LuaCompletionItemKind.ScriptActionId or LuaCompletionItemKind.ScriptConditionId;

    private string GetCurrentPrefix()
    {
        var start = CodeEditor.CaretOffset;
        while (start > 0)
        {
            var character = CodeEditor.Document.GetCharAt(start - 1);
            if (!char.IsLetterOrDigit(character) && character != '_')
            {
                break;
            }

            start--;
        }

        return CodeEditor.Document.GetText(start, CodeEditor.CaretOffset - start);
    }

    private void OnEditorMouseHover(object sender, MouseEventArgs e)
    {
        var position = CodeEditor.GetPositionFromPoint(e.GetPosition(CodeEditor));
        if (position == null)
        {
            CloseHoverToolTip();
            return;
        }

        var offset = CodeEditor.Document.GetOffset(position.Value.Line, position.Value.Column);
        var item = _luaExecutorWindowViewModel.GetDocumentation(CodeEditor.Text, offset);
        if (item == null)
        {
            CloseHoverToolTip();
            return;
        }

        CloseHoverToolTip();
        _hoverToolTip = new System.Windows.Controls.ToolTip
        {
            Content = CreateDescriptionContent(item),
            MaxWidth = 520,
            Placement = PlacementMode.Mouse,
            PlacementTarget = CodeEditor,
            IsOpen = true,
        };
        e.Handled = true;
    }

    private void OnEditorMouseHoverStopped(object sender, MouseEventArgs e)
    {
        CloseHoverToolTip();
    }

    private void CloseHoverToolTip()
    {
        if (_hoverToolTip == null)
        {
            return;
        }

        _hoverToolTip.IsOpen = false;
        _hoverToolTip = null;
    }

    private static object CreateCompletionItemContent(LuaCompletionItem item)
    {
        var grid = new System.Windows.Controls.Grid
        {
            ToolTip = item.Name,
        };
        grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());
        grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition
        {
            Width = GridLength.Auto,
        });

        var name = new System.Windows.Controls.TextBlock
        {
            Text = item.Name,
            FontFamily = new FontFamily("Consolas"),
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center,
        };
        System.Windows.Controls.Grid.SetColumn(name, 0);
        grid.Children.Add(name);

        if (IsScriptId(item.Kind))
        {
            return grid;
        }

        var kind = new System.Windows.Controls.TextBlock
        {
            Text = KindName(item.Kind),
            Margin = new Thickness(12, 0, 0, 0),
            Opacity = 0.68,
            FontFamily = new FontFamily("Microsoft YaHei UI"),
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
        };
        System.Windows.Controls.Grid.SetColumn(kind, 1);
        grid.Children.Add(kind);
        return grid;
    }

    private static object CreateDescriptionContent(LuaCompletionItem item)
    {
        var panel = new System.Windows.Controls.StackPanel
        {
            MaxWidth = 380,
            Margin = new Thickness(12, 10, 12, 12),
        };

        var title = new System.Windows.Controls.TextBlock
        {
            Text = item.Name,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
        };
        title.SetResourceReference(
            System.Windows.Controls.TextBlock.ForegroundProperty,
            "TextFillColorPrimaryBrush");
        panel.Children.Add(title);

        var source = new System.Windows.Controls.TextBlock
        {
            Text = SourceName(item),
            Margin = new Thickness(0, 3, 0, 0),
            FontSize = 11,
        };
        source.SetResourceReference(
            System.Windows.Controls.TextBlock.ForegroundProperty,
            "TextFillColorSecondaryBrush");
        panel.Children.Add(source);

        if (!string.IsNullOrWhiteSpace(item.Signature))
        {
            var signature = new System.Windows.Controls.TextBlock
            {
                Text = item.Signature,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 18,
            };
            signature.SetResourceReference(
                System.Windows.Controls.TextBlock.ForegroundProperty,
                "TextFillColorPrimaryBrush");

            var signatureContainer = new System.Windows.Controls.Border
            {
                Margin = new Thickness(0, 9, 0, 8),
                Padding = new Thickness(8, 6, 8, 6),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Child = signature,
            };
            signatureContainer.SetResourceReference(
                System.Windows.Controls.Border.BackgroundProperty,
                "ControlFillColorDefaultBrush");
            signatureContainer.SetResourceReference(
                System.Windows.Controls.Border.BorderBrushProperty,
                "CardStrokeColorDefaultBrush");
            panel.Children.Add(signatureContainer);
        }

        foreach (var line in SplitDescriptionLines(item.Description))
        {
            panel.Children.Add(CreateDescriptionRow(line));
        }

        if (!string.IsNullOrWhiteSpace(item.ReturnType) &&
            item.Description?.Contains("返回：", StringComparison.Ordinal) != true)
        {
            panel.Children.Add(CreateDescriptionRow("返回：" + item.ReturnType));
        }

        if (!string.IsNullOrWhiteSpace(item.IntroducedVersion))
        {
            panel.Children.Add(CreateDescriptionRow("日冕版本：" + item.IntroducedVersion));
        }

        return panel;
    }

    private static IEnumerable<string> SplitDescriptionLines(string? description) =>
        string.IsNullOrWhiteSpace(description)
            ? []
            : description.Split(
                ["\r\n", "\n"],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static System.Windows.FrameworkElement CreateDescriptionRow(string line)
    {
        var separator = line.IndexOf('：');
        if (separator <= 0)
        {
            var text = CreateDescriptionText(line);
            text.Margin = new Thickness(0, 2, 0, 2);
            return text;
        }

        var grid = new System.Windows.Controls.Grid
        {
            Margin = new Thickness(0, 2, 0, 2),
        };
        grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition
        {
            Width = new GridLength(88),
        });
        grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition());

        var label = CreateDescriptionText(line[..(separator + 1)]);
        label.FontWeight = FontWeights.SemiBold;
        label.SetResourceReference(
            System.Windows.Controls.TextBlock.ForegroundProperty,
            "TextFillColorSecondaryBrush");
        grid.Children.Add(label);

        var value = CreateDescriptionText(line[(separator + 1)..].TrimStart());
        System.Windows.Controls.Grid.SetColumn(value, 1);
        grid.Children.Add(value);
        return grid;
    }

    private static System.Windows.Controls.TextBlock CreateDescriptionText(string text)
    {
        var textBlock = new System.Windows.Controls.TextBlock
        {
            Text = text,
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 18,
        };
        textBlock.SetResourceReference(
            System.Windows.Controls.TextBlock.ForegroundProperty,
            "TextFillColorPrimaryBrush");
        return textBlock;
    }

    private static string KindName(LuaCompletionItemKind kind) => kind switch
    {
        LuaCompletionItemKind.Function => "函数",
        LuaCompletionItemKind.Method => "方法",
        LuaCompletionItemKind.Module => "模块",
        LuaCompletionItemKind.Class => "类型",
        LuaCompletionItemKind.Field => "字段",
        LuaCompletionItemKind.Variable => "变量",
        LuaCompletionItemKind.Enum => "枚举",
        LuaCompletionItemKind.EnumValue => "枚举值",
        LuaCompletionItemKind.ScriptActionId => "动作 ID",
        LuaCompletionItemKind.ScriptConditionId => "条件 ID",
        _ => "关键字",
    };

    private static string SourceName(LuaCompletionItem item) => item.Kind switch
    {
        LuaCompletionItemKind.ScriptActionId => "World Builder 动作",
        LuaCompletionItemKind.ScriptConditionId => "World Builder 条件",
        _ => SourceName(item.SourceKind),
    };

    private static string SourceName(LuaCompletionSourceKind sourceKind) => sourceKind switch
    {
        LuaCompletionSourceKind.CurrentDocument => "当前文档",
        LuaCompletionSourceKind.UserLibrary => "用户代码库",
        LuaCompletionSourceKind.LuaLibrary => "Lua 库",
        LuaCompletionSourceKind.NativeApi => "Lua 4 / 原生 API",
        _ => "Lua 4",
    };

    private sealed class LuaCompletionData(LuaCompletionItem item) : ICompletionData
    {
        public ImageSource? Image => null;

        public string Text => item.Name;

        public object Content => CreateCompletionItemContent(item);

        public object Description => CreateDescriptionContent(item);

        public double Priority => item.SourceKind switch
        {
            LuaCompletionSourceKind.CurrentDocument => 5,
            LuaCompletionSourceKind.UserLibrary => 4,
            LuaCompletionSourceKind.LuaLibrary => 3,
            LuaCompletionSourceKind.NativeApi => 2,
            _ => 1,
        };

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var replacementOffset = completionSegment.Offset;
            var hasOpeningParenthesis = completionSegment.EndOffset < textArea.Document.TextLength &&
                                        textArea.Document.GetCharAt(completionSegment.EndOffset) == '(';
            var isEnteringOpeningParenthesis = insertionRequestEventArgs is TextCompositionEventArgs { Text: "(" };
            var appendParentheses = item.IsCallable && !hasOpeningParenthesis && !isEnteringOpeningParenthesis;
            var insertion = appendParentheses ? item.Name + "()" : item.Name;
            textArea.Document.Replace(completionSegment, insertion);
            if (appendParentheses)
            {
                textArea.Caret.Offset = replacementOffset + item.Name.Length + 1;
            }
        }
    }
}
