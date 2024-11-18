using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using hospital_pc_client.Utils;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Utils;
using Ra3MapUtils.ViewModels.MainWindowPages;
using Ra3MapUtils.Views;
using UtilLib.mapFileHelper;
using MessageBox = System.Windows.MessageBox;

namespace Ra3MapUtils.ViewModels
{
    public partial class MainWindowViewModel: ObservableObject, IObserver
    {
        [ObservableProperty ]public string _windowTitle = $"{GlobalVarsModel.ProgramName} {GlobalVarsModel.VersionStr}";
        
        public SettingPageViewModel _settingPageViewModel = App.Current.Services.GetRequiredService<SettingPageViewModel>();

        [ObservableProperty] private Visibility _settingPageInfoBadgeVisibility = Visibility.Collapsed;
        
        [RelayCommand]
        private void CloseWindow()
        {
            Environment.Exit(0);
        }

        public void OnNotify(object sender, NotifyEventArgs e)
        {
            if (e.EventName == "UpdateModelChanged")
            {
                if (_settingPageViewModel.UpdateModel.IsUpdateAvailable)
                {
                    SettingPageInfoBadgeVisibility = Visibility.Visible;
                }
                else
                {
                    SettingPageInfoBadgeVisibility = Visibility.Collapsed;
                }
            }
        }
    }
}