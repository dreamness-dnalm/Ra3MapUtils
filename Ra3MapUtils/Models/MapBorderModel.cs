using CommunityToolkit.Mvvm.ComponentModel;

namespace Ra3MapUtils.Models;

public partial class MapBorderModel: ObservableObject
{
    [ObservableProperty] private int _x1;
    [ObservableProperty] private int _y1;
    [ObservableProperty] private int _x2;
    [ObservableProperty] private int _y2;
    
    // public MapBorderModel(int x1, int y1, int x2, int y2)
    // {
    //     _x1 = x1;
    //     _y1 = y1;
    //     _x2 = x2;
    //     _y2 = y2;
    // }
}