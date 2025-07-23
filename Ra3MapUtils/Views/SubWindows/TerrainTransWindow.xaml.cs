using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views;

public partial class TerrainTransWindow : FluentWindow
{
    public TerrainTransWindowViewModel _terrainTransWindowViewModel { get => (TerrainTransWindowViewModel)DataContext; }
    
    public TerrainTransWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<TerrainTransWindowViewModel>();
        InitializeComponent();
    }
}