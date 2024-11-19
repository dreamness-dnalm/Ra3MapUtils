using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class SettingPageViewModel: ObservableObject
{
    
    
    
    [RelayCommand]
    private void PickNewWorldBuilderPath()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择新地编路径",
            Filter = "All files (*.exe)|*.exe",
            InitialDirectory = @"C:\",
            Multiselect = false
        };

        bool? result = dialog.ShowDialog();
        if (result == true)
        {
            var selectedPath = dialog.FileName;
            if (NewWorldBuilderBusiness.IsPathValidForNewWorldBuilderPath(selectedPath))
            {
                NewWorldBuilderBusiness.NewWorldBuilderPath = selectedPath;
            }
        }
    }
}