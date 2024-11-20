using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows;

public partial class CodeEditorWindow : FluentWindow
{
    public CodeEditorWindowViewModel _codeEditorWindowViewModel { get => (CodeEditorWindowViewModel)DataContext; }
    
    public CodeEditorWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<CodeEditorWindowViewModel>();
        InitializeComponent();
        _codeEditorWindowViewModel._textEditor = CodeEditor;
        
        CodeEditor.TextArea.Caret.PositionChanged += (sender, args) =>
        {
            _codeEditorWindowViewModel.ReloadCaretInfo();
        };
    }
}