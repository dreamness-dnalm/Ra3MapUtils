using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ra3MapUtils.Models;

public partial class MapStrFileModel: ObservableObject
{
    [ObservableProperty] private ObservableCollection<MapStrFileRecordModel> _showingRecords = new();
    
    [ObservableProperty] private Dictionary<string, string> _recordDict = new();

    [ObservableProperty] private string _mapShowingName = "";
}

public partial class MapStrFileRecordModel : ObservableObject
{
    
}