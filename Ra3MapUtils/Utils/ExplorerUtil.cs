namespace Ra3MapUtils.Utils;

public static class ExplorerUtil
{
    public static void OpenExplorer(string path, bool select = false)
    {
        if (select)
        {
            path = "/select," + path;
        }
        else
        {
            path = " " + path;
        }
        System.Diagnostics.Process.Start("explorer.exe", path);
    }
}