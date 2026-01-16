using System;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dreamness.ScriptExecutor;

namespace Ra3MapUtils.ViewModels.SubWindows;

public partial class NanoProgramRunResultWindowViewModel : ObservableObject
{
    public string ProgramName { get; }
    public bool Success { get; }
    public string StatusText { get; }
    public string ReturnValueText { get; }
    public string OutputText { get; }
    public string ErrorText { get; }
    public string ExceptionText { get; }
    public string ExceptionOrErrorText => string.IsNullOrWhiteSpace(ExceptionText) ? ErrorText : ExceptionText;
    public string ErrorAndExceptionText =>
        string.Join(Environment.NewLine,
            new[] { ErrorText, ExceptionText }
                .Where(s => !string.IsNullOrWhiteSpace(s)));
    public string TimeText { get; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    public NanoProgramRunResultWindowViewModel(string programName, ScriptExecutionResult? result, Exception? ex = null)
    {
        ProgramName = programName;
        Success = result?.Success ?? false;
        StatusText = Success ? "成功" : "失败";

        ReturnValueText = TryGetString(result, "ReturnValue", "Result", "Value");
        OutputText = TryGetString(result, "Output", "StdOut", "ConsoleOutput", "Logs");
        ErrorText = TryGetString(result, "Error", "Message");
        ExceptionText = TryGetExceptionString(result) ?? ex?.ToString() ?? string.Empty;

        if (ex != null && string.IsNullOrWhiteSpace(ErrorText))
        {
            ErrorText = ex.Message;
        }
    }

    private static string TryGetString(object? obj, params string[] propertyNames)
    {
        if (obj == null)
        {
            return string.Empty;
        }

        foreach (var name in propertyNames)
        {
            var prop = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop?.GetValue(obj) is { } val)
            {
                if (val is string s)
                {
                    return s;
                }

                try
                {
                    return JsonSerializer.Serialize(val, new JsonSerializerOptions { WriteIndented = true });
                }
                catch
                {
                    return val.ToString() ?? string.Empty;
                }
            }
        }

        return string.Empty;
    }

    private static string? TryGetExceptionString(object? obj)
    {
        if (obj == null)
        {
            return null;
        }

        var prop = obj.GetType().GetProperty("Exception", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        var val = prop?.GetValue(obj);
        return val?.ToString();
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
        catch (System.Exception)
        {
            // 某些情况下剪贴板被占用，简单忽略避免崩溃
        }
    }
}

