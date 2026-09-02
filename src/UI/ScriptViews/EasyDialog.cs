using System.Windows.Forms;

namespace Ra3MapUtils.ScriptViews;

public class EasyDialog: BaseDialog
{
    public static Form Create(string title = "对话框", int width = 400, int height = 250, bool topMost = true)
    {
        return _create(title, width, height, topMost);
    }
}