using Ra3MapUtils.ScriptViews;
using System.Windows.Forms;
using System.Threading;
using System;
using System.IO;
using Dreamness.Ra3.Map.Facade.Core;


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

var ra3map = Ra3MapFacade.Open(mapFilePath);

for(int x = 200; x < 300; x++)
{
    for(int y = 200; y < 300; y++)
    {
        ra3map.SetTerrainHeight(x, y, 500);
    }
}

ra3map.Save();