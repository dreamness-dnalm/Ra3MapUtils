using SharedFunctionLib.DAO;

namespace SharedFunctionLib.Business;

public class ApiServiceBusiness
{
    public static bool IsApiServiceEnabled
    {
        get
        {
            var isApiServiceEnabled = SettingsDAO.GetSetting("ApiService_IsApiServiceEnabled");
            if (isApiServiceEnabled == null)
            {
                IsApiServiceEnabled = true;
                return true;
            }
            return bool.Parse(isApiServiceEnabled);
        }
        set => SettingsDAO.SetSetting("ApiService_IsApiServiceEnabled", value.ToString());
    }
    
    public static int ApiServicePort
    {
        get
        {
            var apiServicePort = SettingsDAO.GetSetting("ApiService_ApiServicePort");
            if (apiServicePort == null)
            {
                ApiServicePort = 17289; 
                return 17289;
            }
            return int.Parse(apiServicePort);
        }
        set => SettingsDAO.SetSetting("ApiService_ApiServicePort", value.ToString());
    }
}