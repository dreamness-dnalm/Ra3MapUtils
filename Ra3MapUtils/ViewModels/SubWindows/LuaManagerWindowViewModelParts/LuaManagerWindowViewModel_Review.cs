using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Views.SubWindows;

namespace Ra3MapUtils.ViewModels;

public partial class LuaManagerWindowViewModel
{
    // [ObservableProperty] private LibFileModel _selectPreviewLibFileModel;
[ObservableProperty] private ObservableCollection<LibFileModel> _selectPreviewLibFileModels = new();
    [ObservableProperty] private LibFileModel _selectedLibFileModel = new();

    [ObservableProperty] private Visibility _previewSelectedScriptRunOnceVisibility = Visibility.Collapsed;

    [ObservableProperty] private Visibility _previewSelectedScriptVisibility = Visibility.Collapsed;
    
    [RelayCommand]
    private void ReloadLibPreview()
    {
        if(SelectedLuaLibConfig == null && SelectedLuaLibConfig.LibPath != "")
        {
            PreviewSelectedScriptVisibility = Visibility.Collapsed;
            return;
        }
        else
        {
            PreviewSelectedScriptVisibility = Visibility.Visible;
        }
        SelectPreviewLibFileModels.Clear();
        try
        {
            SelectPreviewLibFileModels.Add(LibFileModel.Load(SelectedLuaLibConfig.LibPath));
            // SelectPreviewLibFileModel = LibFileModel.Load(SelectedLuaLibConfig.LibPath);
        }
        catch (Exception e)
        {
            // SelectPreviewLibFileModel = new LibFileModel();
        }
        
    }
    
    
    partial void OnSelectedLibFileModelChanged(LibFileModel value)
    {
        if (value == null)
        {
            return;
        }
        if (value.FileType == "lua")
        {
            PreviewSelectedScriptRunOnceVisibility = Visibility.Visible;
        }
        else
        {
            PreviewSelectedScriptRunOnceVisibility = Visibility.Collapsed;
        }
        
    }

    partial void OnSelectPreviewLibFileModelsChanged(ObservableCollection<LibFileModel> value)
    {
        if (value == null || value.Count == 0)
        {
            return;
        }

        LibFileModel.SaveMeta(SelectedLuaLibConfig.LibPath , value[0]);
    }

    [RelayCommand]
    private void LibPreviewItemSelectedChanged(RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedLibFileModel = e.NewValue as LibFileModel;
    }

    [RelayCommand]
    private void PreviewChanged()
    {
        Save();
    }

    private void Save()
    {
        if(_selectPreviewLibFileModels.Count == 0)
        {
            return;
        }
        LibFileModel.SaveMeta(SelectedLuaLibConfig.LibPath , _selectPreviewLibFileModels[0]);
    }


    [RelayCommand]
    private void PreviewAddLua()
    {
        if(SelectPreviewLibFileModels.Count == 0)
        {
            return;
        }
    }

    [RelayCommand]
    private void PreviewAddFolder()
    {
        if(SelectPreviewLibFileModels.Count == 0)
        {
            return;
        }
    }

    [RelayCommand]
    private void PreviewRename()
    {
        if(SelectPreviewLibFileModels.Count == 0)
        {
            return;
        }
    }
    
    [RelayCommand]
    private void PreviewDelete()
    {
        if(SelectPreviewLibFileModels.Count == 0)
        {
            return;
        }
        if(SelectedLibFileModel == null && SelectedLibFileModel.FilePath != "")
        {
            return;
        }
        SelectedLibFileModel.Delete();
        Save();
    }
    
    [RelayCommand]
    private void PreviewUpPosition()
    {
        if(SelectPreviewLibFileModels.Count == 0)
        {
            return;
        }
        var tmp = _selectedLibFileModel;
        _selectedLibFileModel.UpPos();
        SelectedLibFileModel = tmp;
        Save();
    }
    [RelayCommand]
    private void PreviewDownPosition()
    {
        if(SelectPreviewLibFileModels.Count == 0)
        {
            return;
        }
        var tmp = _selectedLibFileModel;
        _selectedLibFileModel.DownPos();
        SelectedLibFileModel = tmp;
        Save();
    }

    [RelayCommand]
    private void PreviewShowCode()
    {
        var filePath = "??";
        try
        {
            filePath = new DirectoryInfo(_selectedLuaLibConfig.LibPath).Parent.FullName + "\\" + _selectedLibFileModel.FilePath;
        }
        catch (Exception e)
        {
        }
        
        var codeEditorWindow = App.Current.Services.GetRequiredService<CodeEditorWindow>();
        
        codeEditorWindow._codeEditorWindowViewModel.FilePath = filePath;
        codeEditorWindow.Show();
    }
    
    [RelayCommand]
    private void PreviewEditComment()
    {
        if(SelectPreviewLibFileModels.Count == 0)
        {
            return;
        }
        if(SelectedLibFileModel == null && SelectedLibFileModel.FilePath != "")
        {
            return;
        }
        
        var inputDialog = new Ookii.Dialogs.WinForms.InputDialog();
        inputDialog.MainInstruction = "请输入备注";
        inputDialog.Content = "请输入备注";
        inputDialog.WindowTitle = "备注";
        inputDialog.Input = SelectedLibFileModel.Comment;
        
        if (inputDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SelectedLibFileModel.Comment = inputDialog.Input;
            Save();
        }
    }
}