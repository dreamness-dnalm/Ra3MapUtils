using System.Windows.Forms;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.ViewModels.MainWindowPages;
using Ra3MapUtils.Views.SubWindows.toolbox;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.ViewModels.toolbox;

public partial class LogViewerWindowViewModel: ObservableObject
{
    [ObservableProperty] private int _logLevelIndex = 1;

    [ObservableProperty] private string _logFileName = "aaa";

    [ObservableProperty] private string _parseStatus = "已停止";
    
    [ObservableProperty] private Brush _parseStatusColor = Brushes.Red;

    private string logLevel = "DEBUG";

    private long pos = 0;

    [RelayCommand]
    private void StartFromBeginning()
    {
        
    }

    [RelayCommand]
    private void StartFromLatest()
    {
        
    }

    [RelayCommand]
    private void StartFromLastStopPos()
    {
        
    }

    [RelayCommand]
    private void Stop()
    {
        
    }
    
    [RelayCommand]
    private void cleanLog()
    {
        
    }

    [RelayCommand]
    private void SwitchLogFile()
    {
        
    }
    
    [RelayCommand]
    private void SwitchLogLevel()
    {
        MessageBox.Show("SwitchLogLevel: " + _logLevelIndex);
    }
}