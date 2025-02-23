using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.ViewModels.toolbox;
using Wpf.Ui.Controls;

namespace Ra3MapUtils.Views.SubWindows.toolbox;

public partial class ChatLuaHelperViewWindow : FluentWindow
{
    
    public ChatLuaHelperViewModel _chatLuaHelperViewModel => (ChatLuaHelperViewModel)DataContext;
    
    public ChatLuaHelperViewWindow()
    {
        DataContext = App.Current.Services.GetRequiredService<ChatLuaHelperViewModel>();
        InitializeComponent();
        _chatLuaHelperViewModel._chatLuaHelperViewWindow = this;
    }
}