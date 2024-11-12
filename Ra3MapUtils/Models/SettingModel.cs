using CommunityToolkit.Mvvm.ComponentModel;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.Models;

public partial class SettingModel: ObservableObject
{
    [ObservableProperty]private int _luaRedundancyFactor;

    partial void OnLuaRedundancyFactorChanged(int value)
    {
        LuaImporterBusiness.LuaRedundancyFactor = value;
    }

    public void Reload()
    {
        LuaRedundancyFactor = LuaImporterBusiness.LuaRedundancyFactor;
    }

    public SettingModel()
    {
        Reload();
    }
}