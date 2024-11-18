using CommunityToolkit.Mvvm.ComponentModel;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.Models;

public partial class SettingModel: ObservableObject
{
    [ObservableProperty]private int _luaRedundancyFactor;
    
    [ObservableProperty]private bool _isAutoUpdate;

    partial void OnLuaRedundancyFactorChanged(int value)
    {
        LuaImporterBusiness.LuaRedundancyFactor = value;
    }
    
    partial void OnIsAutoUpdateChanged(bool value)
    {
        UpdateBusiness.IsAutoUpdateEnabled = value;
    }

    public void Reload()
    {
        LuaRedundancyFactor = LuaImporterBusiness.LuaRedundancyFactor;
        IsAutoUpdate = UpdateBusiness.IsAutoUpdateEnabled;
    }

    public SettingModel()
    {
        Reload();
    }
}