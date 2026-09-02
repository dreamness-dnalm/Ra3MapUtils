using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.NanoPrograms;

namespace UI.Views.Dialogs;

public partial class NanoProgramRunResultDialog : Window
{
    public NanoProgramRunResultDialog(string programName, NanoProgramExecutionResult? result, Exception? ex)
    {
        InitializeComponent();
        DataContext = new NanoProgramRunResultViewModel(programName, result, ex);
    }
}

public partial class NanoProgramRunResultViewModel : ObservableObject
{
    public string ProgramName { get; }
    public bool Success { get; }
    public string StatusText { get; }
    public string ReturnValueText { get; }
    public string OutputText { get; }
    public string ErrorText { get; }
    public string TimeText { get; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    public NanoProgramRunResultViewModel(string programName, NanoProgramExecutionResult? result, Exception? ex)
    {
        ProgramName = programName;
        Success = result?.Success ?? false;
        StatusText = Success ? "成功" : "失败";
        ReturnValueText = result?.ReturnValue?.ToString() ?? string.Empty;
        OutputText = result?.Output ?? string.Empty;

        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(result?.Error))
        {
            parts.Add(result!.Error!);
        }

        if (!string.IsNullOrWhiteSpace(result?.Exception))
        {
            parts.Add(result!.Exception!);
        }

        if (ex != null)
        {
            parts.Add(ex.ToString());
        }

        ErrorText = string.Join(Environment.NewLine, parts);
    }

    [RelayCommand]
    private void Copy(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        try
        {
            Clipboard.SetDataObject(text, true);
        }
        catch
        {
            // ignore clipboard contention
        }
    }
}
