using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Utils;
using Ra3MapUtils.Views;
using UtilLib.mapFileHelper;
using MessageBox = System.Windows.MessageBox;

namespace Ra3MapUtils.ViewModels
{
    public partial class MainWindowViewModel: ObservableObject
    {
        [ObservableProperty ]public string _windowTitle = $"{GlobalVarsModel.ProgramName} {GlobalVarsModel.VersionStr}";
        
        [RelayCommand]
        private void CloseWindow()
        {
            Environment.Exit(0);
        }

    }
}