using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.toolbox;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows.toolbox;

public partial class MapDataEditorWindow : FluentWindow
{
    public MapDataEditorWindowViewModel _mapDataEditorWindowViewModel => (MapDataEditorWindowViewModel)DataContext;

    public MapDataEditorWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<MapDataEditorWindowViewModel>();
        InitializeComponent();
    }
}
