using System.Windows;
using System.Windows.Documents;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.toolbox;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows.toolbox;

public partial class LogViewerWindow : FluentWindow
{
    public LogViewerWindowViewModel _LogViewerWindowViewModel {get => (LogViewerWindowViewModel)DataContext;}
    
    public LogViewerWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<LogViewerWindowViewModel>();
        InitializeComponent();
        _LogViewerWindowViewModel._logViewerWindow = this;
        _LogViewerWindowViewModel.TextLines.CollectionChanged += (s, e) => UpdateRichTextBox();
    }
    
    private void UpdateRichTextBox()
    {
        LogTextBox.Dispatcher.Invoke(() =>
        {
            LogTextBox.Document.Blocks.Clear();
            foreach (var line in (DataContext as LogViewerWindowViewModel)?.TextLines)
            {
                var paragraph = new Paragraph();
                var run = new Run(line.Text) { Foreground = line.Color };
                paragraph.Margin = new Thickness(0, 0, 0, 3); 
                paragraph.Inlines.Add(run);
                LogTextBox.Document.Blocks.Add(paragraph);
            }
        });

    }
}