using CommunityToolkit.Mvvm.ComponentModel;
using hospital_pc_client.Utils;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.Models;

public partial class LuaLibBindingModel: ObservableObject, INotify
{
    [ObservableProperty] private string _luaLibPath;

    [ObservableProperty] private bool _isAutoUpdateEnabled;

    [ObservableProperty] private bool _isAutoLoadWhenImport;

    partial void OnLuaLibPathChanged(string value)
    {
        LuaLibBindingBusiness.LuaLibPath = value;
        // ObservableUtil.Notify(this, new NotifyEventArgs("LuaLibBindingModelChanged"));
    }
    
    partial void OnIsAutoUpdateEnabledChanged(bool value)
    {
        LuaLibBindingBusiness.IsAutoUpdateEnabled = value;
        // ObservableUtil.Notify(this, new NotifyEventArgs("LuaLibBindingModelChanged"));
    }
    
    partial void OnIsAutoLoadWhenImportChanged(bool value)
    {
        LuaLibBindingBusiness.IsAutoLoadWhenImport = value;
        // ObservableUtil.Notify(this, new NotifyEventArgs("LuaLibBindingModelChanged"));
    }
    
    public void Reload()
    {
        LuaLibPath = LuaLibBindingBusiness.LuaLibPath;
        IsAutoUpdateEnabled = LuaLibBindingBusiness.IsAutoUpdateEnabled;
        IsAutoLoadWhenImport = LuaLibBindingBusiness.IsAutoLoadWhenImport;
    }
    
    public LuaLibBindingModel()
    {
        Reload();
    }

    public List<IObserver> _observers { get; set; }
}