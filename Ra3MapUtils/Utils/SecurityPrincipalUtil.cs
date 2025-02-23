using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace Ra3MapUtils.Utils;

public class SecurityPrincipalUtil
{
    
    private static bool? _isRunningAsAdministrator = null;
    public static bool IsRunningAsAdministrator
    {
        get  {
            if (_isRunningAsAdministrator == null)
            {


                // 获取当前用户的身份信息
                WindowsIdentity identity = WindowsIdentity.GetCurrent();

                // 创建WindowsPrincipal对象
                WindowsPrincipal principal = new WindowsPrincipal(identity);

                // 检查是否属于管理员角色
                _isRunningAsAdministrator = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return _isRunningAsAdministrator.Value;
        }
    }

    private static void RequestAdminRuleAndRestart()
    {
        if (!IsRunningAsAdministrator)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas" // 触发UAC提权
            };

            try
            {
                Process.Start(processInfo);
                Application.Exit();
            }
            catch
            {
                MessageBox.Show("提权失败，操作无法完成！");
            }
        }
    }
}