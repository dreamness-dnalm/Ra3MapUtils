using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ra3MapUtils.Models;

namespace Ra3MapUtils.ViewModels;

public partial class TerrainTransWindowViewModel: ObservableObject
{
    [ObservableProperty] private string _mapName = "";
    
    [ObservableProperty] private string _selectedMapName = "";
    
    [ObservableProperty] private string _windowTitle = "";
    
    partial void OnMapNameChanged(string value)
    {
        WindowTitle = $"地形变换工具 - {value}";
        // _borderModels.Clear();
        //
        // if (value != null)
        // {
        //     ReloadBoards();
        // }
    }

    [RelayCommand]
    private void Closed()
    {
        GlobalVarsModel.TerrainTransWindowOpened = false;
        // others
    }
}