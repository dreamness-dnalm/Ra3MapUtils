using CommunityToolkit.Mvvm.ComponentModel;

namespace Ra3MapUtils.ViewModels;

public partial class LuaManagerWindowViewModel: ObservableObject
{
    public string XmlFilePath { get; set; } = "";
    
    
    [ObservableProperty] private string _mapName = "";
    
    
}