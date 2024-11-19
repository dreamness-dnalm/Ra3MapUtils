using CommunityToolkit.Mvvm.ComponentModel;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.Models;

public partial class SettingModel: ObservableObject
{
    [ObservableProperty]private int _luaRedundancyFactor;
    
    [ObservableProperty]private bool _isAutoUpdate;
    
    [ObservableProperty]private string _newWorldBuilderPath;

    partial void OnLuaRedundancyFactorChanged(int value)
    {
        LuaImporterBusiness.LuaRedundancyFactor = value;
    }
    
    partial void OnIsAutoUpdateChanged(bool value)
    {
        UpdateBusiness.IsAutoUpdateEnabled = value;
    }
    
    partial void OnNewWorldBuilderPathChanged(string value)
    {
        NewWorldBuilderBusiness.NewWorldBuilderPath = value;
    }

    public void Reload()
    {
        LuaRedundancyFactor = LuaImporterBusiness.LuaRedundancyFactor;
        IsAutoUpdate = UpdateBusiness.IsAutoUpdateEnabled;
        NewWorldBuilderPath = NewWorldBuilderBusiness.NewWorldBuilderPath;
    }

    public SettingModel()
    {
        Reload();
    }
}