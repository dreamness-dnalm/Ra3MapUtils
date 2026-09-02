using Ra3MapUtils.ScriptViews;
using System.Windows.Forms;
using System.Threading;
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Dreamness.Ra3.Map.Facade.Core;
using Ra3MapUtils.Utils;


string? mapFilePath = null;

// Dictionary<string, string> ArgumentDictionary = null;

if (ArgumentDictionary.ContainsKey("MapFilePath") && ArgumentDictionary["MapFilePath"] != null)
{
    mapFilePath = ArgumentDictionary["MapFilePath"];
}
else
{
    var dialog = MapFileSelectorDialog.Create("选择地图文件");
    dialog.ShowDialog();
    if (dialog.DialogResult == DialogResult.OK)
    {
        mapFilePath = dialog.Tag as string;
    }
}

if (mapFilePath == null)
{
    throw new Exception("未选择地图文件，程序终止。");
}

MapLuaImporterUtil.ImportLuaWithActiveConfig(mapFilePath);


