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
var inputFilePath = Path.Combine(Path.GetDirectoryName(mapFilePath), "TeamsData.json");

if(!File.Exists(inputFilePath))
{
    throw new Exception($"未找到输入文件: {inputFilePath}，程序终止。");
}


var ra3map = Ra3MapFacade.Open(mapFilePath);

var jsonStrInput = File.ReadAllText(inputFilePath, Encoding.UTF8);

ra3map.ImportTeamsFromJsonStr(jsonStrInput);
var backupPath = ra3map.Backup();
ra3map.Save();
MsgDialog.Create("导入玩家数据成功", $"已成功导入数据并保存地图文件。\n地图备份文件位置: {backupPath}").ShowDialog();