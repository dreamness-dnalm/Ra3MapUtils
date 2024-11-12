using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class SettingPageViewModel: ObservableObject
{
    [ObservableProperty] private SettingModel _settingModel = new SettingModel();

    [ObservableProperty] private int _luaRedundancyFactor;

    public SettingPageViewModel()
    {
        _luaRedundancyFactor = SettingModel.LuaRedundancyFactor;
    }

    [RelayCommand]
    private void SaveLuaRedundancyFactor()
    {
        if (_luaRedundancyFactor < 100 || _luaRedundancyFactor > 3000)
        {
            MessageBox.Show("Lua冗余系数应在100-3000之间");
        }
        SettingModel.LuaRedundancyFactor = LuaRedundancyFactor;
    }
}