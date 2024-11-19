using CommunityToolkit.Mvvm.ComponentModel;

namespace Ra3MapUtils.Models;

public partial class NewWorldBuilderModel: ObservableObject
{
    [ObservableProperty] private string _newWorldBuilderPath;
    
    [ObservableProperty] private bool _newWorldBuilderAvailable;
    
    
}