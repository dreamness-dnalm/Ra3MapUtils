using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class ScriptListPageViewModel: ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _scriptNameList = new();

    [RelayCommand]
    private void RunOrStopScript()
    {
        
    }
    
    [RelayCommand]
    private void RefreshScriptList()
    {
        
    }
    
    [RelayCommand]
    private void AddScript()
    {
        
    }
    
    [RelayCommand]
    private void DeleteScript()
    {
        
    }

    [RelayCommand]
    private void RenameScript()
    {
        
    }

    [RelayCommand]
    private void CloneScript()
    {
        
    }

    [RelayCommand]
    private void OpenInExplorer()
    {
        
    }

    [RelayCommand]
    private void SaveScript()
    {
        
    }
    
    
}