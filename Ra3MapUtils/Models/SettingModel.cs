using CommunityToolkit.Mvvm.ComponentModel;
using hospital_pc_client.Utils;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.Models;

public partial class SettingModel: ObservableObject, INotify
{
    [ObservableProperty]private int _luaRedundancyFactor;
    
    [ObservableProperty]private bool _isAutoUpdate;
    
    [ObservableProperty]private string _newWorldBuilderPath;

    [ObservableProperty] private bool _isEnableAutoBackup;

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

    public List<IObserver> _observers { get; set; }
}