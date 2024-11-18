using SharedFunctionLib.DAO;

namespace SharedFunctionLib.Business;

public static class UpdateBusiness
{
    
    public static bool IsAutoUpdateEnabled
    {
        get
        {
            var isAutoUpdateEnabled = SettingsDAO.GetSetting("Update_IsAutoUpdateEnabled");
            if(isAutoUpdateEnabled == null)
            {
                IsAutoUpdateEnabled = true;
                return true;
            }
            return bool.Parse(isAutoUpdateEnabled);
        }
        set => SettingsDAO.SetSetting("Update_IsAutoUpdateEnabled", value.ToString());
    }
}