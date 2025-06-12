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
    
    [ObservableProperty] private bool _isEnableApiService;
    
    [ObservableProperty] private int _apiServicePort;

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
    
    partial void OnIsEnableApiServiceChanged(bool value)
    {
        ApiServiceBusiness.IsApiServiceEnabled = value;
    }
    
    partial void OnApiServicePortChanged(int value)
    {
        ApiServiceBusiness.ApiServicePort = value;
    }

    public void Reload()
    {
        LuaRedundancyFactor = LuaImporterBusiness.LuaRedundancyFactor;
        IsAutoUpdate = UpdateBusiness.IsAutoUpdateEnabled;
        NewWorldBuilderPath = NewWorldBuilderBusiness.NewWorldBuilderPath;
        IsEnableApiService = ApiServiceBusiness.IsApiServiceEnabled;
        ApiServicePort = ApiServiceBusiness.ApiServicePort;
    }

    public SettingModel()
    {
        Reload();
    }

    public List<IObserver> _observers { get; set; }
}