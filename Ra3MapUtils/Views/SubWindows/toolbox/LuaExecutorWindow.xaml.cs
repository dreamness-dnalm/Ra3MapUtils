using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.toolbox;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows.toolbox;

public partial class LuaExecutorWindow : FluentWindow
{
    public LuaExecutorWindowViewModel _luaExecutorWindowViewModel
        => (LuaExecutorWindowViewModel)DataContext;

    public LuaExecutorWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<LuaExecutorWindowViewModel>();
        InitializeComponent();
    }
}
