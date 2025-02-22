using CommunityToolkit.Mvvm.ComponentModel;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.Models;

public partial class ChatLuaHelperModel: ObservableObject
{
    [ObservableProperty] private string _chatLuaHelperFilePath;
    
    partial void OnChatLuaHelperFilePathChanged(string value)
    {
        ChatLuaHelperBusiness.ChatLuaHelperFilePath = value;
    }

    public void Reload()
    {
        ChatLuaHelperFilePath = ChatLuaHelperBusiness.ChatLuaHelperFilePath;
    }
}