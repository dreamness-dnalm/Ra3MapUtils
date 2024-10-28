using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using MapCoreLib.Core.Util;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Utils;
using Ra3MapUtils.Views;
using UtilLib.mapFileHelper;
using MessageBox = System.Windows.MessageBox;

namespace Ra3MapUtils.ViewModels
{
    public partial class MainWindowViewModel: ObservableObject
    {
        
        
        [RelayCommand]
        private void CloseWindow()
        {
            Environment.Exit(0);
        }

    }
}