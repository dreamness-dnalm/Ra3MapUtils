using System.Collections.ObjectModel;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hospital_pc_client.Utils;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Services.Interface;

namespace Ra3MapUtils.ViewModels;

public partial class LuaManagerWindowViewModel: ObservableObject
{
    public string XmlFilePath { get; set; } = "";

    private readonly ILuaImportService _luaImportService = App.Current.Services.GetRequiredService<ILuaImportService>();
    
    [ObservableProperty] private string _mapName = "";

    [ObservableProperty] private string _windowTitle = "";
    
    partial void OnMapNameChanged(string value)
    {
        WindowTitle = $"Lua导入工具 - {value}";
        _luaLibConfigs.Clear();
        _luaImportService.LoadMapLuaLibConfig(value)
            .ForEach(o => _luaLibConfigs.Add(o));
    }

    [RelayCommand]
    private void Closed()
    {
        GlobalVarsModel.SetLuaManagerWindowOpenedMapName(null);
    }
    
    // [RelayCommand]
    // private void Initialize()
    // {
    //     GlobalVarsModel.LuaManagerWindowOpened = true;
    //     MessageBox.Show(MapName);
    // }
}