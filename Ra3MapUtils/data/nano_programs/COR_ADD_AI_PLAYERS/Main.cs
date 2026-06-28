using Ra3MapUtils.ScriptViews;
using System.Windows.Forms;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Dreamness.Ra3.Map.Facade.Core;
using Ra3MapUtils.Utils;

string? mapFilePath = null;

if (ArgumentDictionary.TryGetValue("MapFilePath", out var argumentMapFilePath)
    && !string.IsNullOrWhiteSpace(argumentMapFilePath))
{
    mapFilePath = argumentMapFilePath;
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

if (string.IsNullOrWhiteSpace(mapFilePath))
{
    throw new Exception("未选择地图文件，程序终止。");
}

var targetPlayerNames = new[]
{
    "AIPlayer01",
    "AIPlayer02",
    "AIPlayer03",
    "AIPlayer04",
    "AIPlayer05",
    "AIPlayer06"
};

var ra3map = Ra3MapFacade.Open(mapFilePath);

var backupPath = ra3map.Backup();

var existingPlayerNames = new HashSet<string>(
    ra3map.GetPlayers().Select(p => p.Name),
    StringComparer.Ordinal);

var addedPlayerNames = new List<string>();
var skippedPlayerNames = new List<string>();

foreach (var playerName in targetPlayerNames)
{
    if (existingPlayerNames.Contains(playerName))
    {
        skippedPlayerNames.Add(playerName);
        continue;
    }

    ra3map.AddPlayer(playerName);
    existingPlayerNames.Add(playerName);
    addedPlayerNames.Add(playerName);
}

if (addedPlayerNames.Count == 0)
{
    MsgDialog.Create(
        "添加AI玩家",
        $"目标玩家均已存在，地图未修改。\n已存在玩家：{string.Join(", ", skippedPlayerNames)}").ShowDialog();
    return;
}


ra3map.Save();

var skippedText = skippedPlayerNames.Count == 0
    ? "无"
    : string.Join(", ", skippedPlayerNames);

MsgDialog.Create(
    "添加AI玩家成功",
    $"已新增 {addedPlayerNames.Count} 个玩家：{string.Join(", ", addedPlayerNames)}\n" +
    $"已跳过 {skippedPlayerNames.Count} 个已存在玩家：{skippedText}\n" +
    $"地图已保存。\n地图备份文件位置: {backupPath}").ShowDialog();
