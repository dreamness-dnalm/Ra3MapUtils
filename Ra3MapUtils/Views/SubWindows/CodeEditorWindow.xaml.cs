using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels;

namespace Ra3MapUtils.Views.SubWindows;

public partial class CodeEditorWindow : Window
{
    public CodeEditorWindowViewModel _codeEditorWindowViewModel { get => (CodeEditorWindowViewModel)DataContext; }
    
    public CodeEditorWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<CodeEditorWindowViewModel>();
        InitializeComponent();
        _codeEditorWindowViewModel._textEditor = CodeEditor;
    }
}