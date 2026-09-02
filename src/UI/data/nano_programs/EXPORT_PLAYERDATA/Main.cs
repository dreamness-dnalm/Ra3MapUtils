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


record NamedUnit(string Name, string Type, double X, double Y);

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

var jsonStr = ra3map.ExportPlayersToJsonStr();

var outputFilePath = Path.Combine(Path.GetDirectoryName(mapFilePath), "PlayersData.json");

File.WriteAllText(outputFilePath, jsonStr, Encoding.UTF8);

System.Diagnostics.Process.Start("explorer.exe", "/select," + outputFilePath);