using System.IO;
using System.Windows;
using MapCoreLib.Core.Util;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.ViewModels;

namespace Ra3MapUtils.Views;

public partial class LuaManagerWindow : Window
{
    public LuaManagerWindowViewModel _luaManagerWindowViewModel { get => (LuaManagerWindowViewModel)DataContext; }
    
    public LuaManagerWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<LuaManagerWindowViewModel>();
        InitializeComponent();
    }
}