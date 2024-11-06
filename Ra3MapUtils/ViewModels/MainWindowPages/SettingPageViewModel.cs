using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Ra3MapUtils.ViewModels.MainWindowPages;

public partial class SettingPageViewModel(ISnackbarService snackbarService): ObservableObject
{
    // private readonly ISnackbarService _snackbarService = App.Current.Services.GetRequiredService<ISnackbarService>();
    
    [RelayCommand]
    private void ShowSnackbar()
    {
        // MessageBox.Show("d");
        // var snackbar = new Snackbar(new SnackbarPresenter());
        // snackbar.SetCurrentValue(Snackbar.TitleProperty, "aaaaaaaaaaa");
        // snackbar.Show();
        // snackbarService.SetSnackbarPresenter(new SnackbarPresenter());
        snackbarService.Show("aa", "bb", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5));
    }
}