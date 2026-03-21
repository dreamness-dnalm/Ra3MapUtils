using System.Windows.Forms;
using System.Threading;
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Dreamness.Ra3.Map.Facade.Util;


string? mapFilePath = null;

// Dictionary<string, string> ArgumentDictionary = null;

if (ArgumentDictionary.ContainsKey("MapFilePath") && ArgumentDictionary["MapFilePath"] != null)
{
    mapFilePath = ArgumentDictionary["MapFilePath"];
}

string mapDir;
if (string.IsNullOrWhiteSpace(mapFilePath))
{
    mapDir = Ra3PathUtil.RA3MapFolder;
}
else
{
    mapDir = Path.GetDirectoryName(Path.GetFullPath(mapFilePath)) ?? "";
}

if (string.IsNullOrEmpty(mapDir) || !Directory.Exists(mapDir))
{
    var hint = string.IsNullOrWhiteSpace(mapFilePath) ? mapDir : mapFilePath;
    MessageBox.Show(
        $"无法打开文件夹，目录不存在或无效：\n{hint}",
        "打开地图所在文件夹",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning);
    return;
}

try
{
    Process.Start(new ProcessStartInfo
    {
        FileName = "explorer.exe",
        Arguments = $"\"{mapDir}\"",
        UseShellExecute = true
    });
}
catch (Exception ex)
{
    MessageBox.Show(
        $"打开文件夹失败：{ex.Message}",
        "打开地图所在文件夹",
        MessageBoxButtons.OK,
        MessageBoxIcon.Error);
}
