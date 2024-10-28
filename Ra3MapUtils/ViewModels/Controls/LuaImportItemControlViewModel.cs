using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using hospital_pc_client.Utils;
using Ra3MapUtils.Models;

namespace Ra3MapUtils.ViewModels.Controls;

public partial class LuaImportItemControlViewModel: ObservableObject, INotify
{
    [ObservableProperty] private LuaLibConfigModel _model;

    [RelayCommand]
    private void Rename()
    {
        
    }

    [RelayCommand]
    private void ChangePath()
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("LuaImportItemChangePath", _model));
    }

    [RelayCommand]
    private void Delete()
    {
        _model.Delete();
        ObservableUtil.Notify(this, new NotifyEventArgs("LuaImportItemDelet", _model));
    }

    [RelayCommand]
    private void UpPos()
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("LuaImportItemUpPos", _model));
    }

    [RelayCommand]
    private void DownPos()
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("LuaImportItemDownPos", _model));
    }

    [RelayCommand]
    private void DoImport()
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("LuaImportItemDoImport", _model));
    }

    [RelayCommand]
    private void Preview()
    {
        ObservableUtil.Notify(this, new NotifyEventArgs("LuaImportItemPreview", _model));
    }

    public List<IObserver> _observers { get; set; }
}