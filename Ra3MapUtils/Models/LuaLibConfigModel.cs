using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.FileIO;
using Ra3MapUtils.Services.Interface;
using SharedFunctionLib.Models;

namespace Ra3MapUtils.Models;

public partial class LuaLibConfigModel: ObservableObject
{
    [ObservableProperty] private string _mapName;
    [ObservableProperty] private string _showingName;
    [ObservableProperty] private string _libPath;
    [ObservableProperty] private int _orderNum;

    private readonly ILuaImportService _luaImportService = App.Current.Services.GetRequiredService<ILuaImportService>();
    
    public LuaLibConfigModel(string mapName, string showingName, string libPath, int orderNum)
    {
        _mapName = mapName;
        _showingName = showingName;
        _libPath = libPath;
        _orderNum= orderNum;
    }

    public void Delete()
    {
        _luaImportService.DeleteMapLuaLibConfig(this._mapName, this._showingName);
    }
    
    public void Rename(string newShowingName)
    {
        if (_showingName == newShowingName)
        {
            return;
        }
        _luaImportService.RenameMapLuaLibConfig(_mapName, _showingName, newShowingName);
        ShowingName = newShowingName;
    }
    
    public void Upsert()
    {
        _luaImportService.SaveMapLuaLibConfig(this);
    }

    public static LuaLibConfigModel FromSimple(SimpleLuaLibConfigModel model)
    {
        return new LuaLibConfigModel(model.MapName, model.ShowingName, model.LibPath, model.OrderNum);
    }
    
}