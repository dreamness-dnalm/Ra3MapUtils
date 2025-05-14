using SharedFunctionLib.DAO;

namespace SharedFunctionLib.Business;

public class ChatLuaHelperBusiness
{
    public static string ChatLuaHelperFilePath
    {
        get
        {
            var chatLuaHelperFilePath = SettingsDAO.GetSetting("ChatLuaHelper_ChatLuaHelperFilePath");
            if (chatLuaHelperFilePath == null)
            {
                ChatLuaHelperFilePath = "";
                return "";
            }
            return chatLuaHelperFilePath;
        }
        set => SettingsDAO.SetSetting("ChatLuaHelper_ChatLuaHelperFilePath", value);
    }
}