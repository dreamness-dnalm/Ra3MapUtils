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

var units = ra3map.GetUnitObjects();
var totalUnits = units.Count;
var unassignedCount = 0;

var playerTeams = new Dictionary<string, Dictionary<string, int>>(StringComparer.OrdinalIgnoreCase);

foreach (var unit in units)
{
    var belong = unit.BelongToTeam?.Trim();
    if (string.IsNullOrEmpty(belong))
    {
        unassignedCount++;
        continue;
    }

    var parts = belong.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length < 2)
    {
        unassignedCount++;
        continue;
    }

    var playerName = parts[0].Trim();
    var teamName = parts[1].Trim();

    if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(teamName))
    {
        unassignedCount++;
        continue;
    }

    if (!playerTeams.TryGetValue(playerName, out var teamCounter))
    {
        teamCounter = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        playerTeams[playerName] = teamCounter;
    }

    teamCounter[teamName] = teamCounter.TryGetValue(teamName, out var cnt) ? cnt + 1 : 1;
}

var sb = new StringBuilder();
sb.AppendLine($"地图：{Path.GetFileName(mapFilePath)}");
sb.AppendLine($"单位总数：{totalUnits}");
sb.AppendLine($"无归属单位：{unassignedCount}");
sb.AppendLine();

if (playerTeams.Count == 0)
{
    sb.AppendLine("没有统计到任何归属队伍的单位。");
}
else
{
    foreach (var player in playerTeams.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
    {
        var playerName = player.Key;
        var teamCounter = player.Value;
        var playerTotal = teamCounter.Values.Sum();

        sb.AppendLine($"玩家：{playerName}（{playerTotal}）");
        foreach (var team in teamCounter.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
        {
            sb.AppendLine($"  - {team.Key}：{team.Value}");
        }

        sb.AppendLine();
    }
}

sb.AppendLine("提示：括号内数字为对应玩家的单位总数。");

var dialog = EasyDialog.Create("队伍单位统计", 640, 720, true);
dialog.StartPosition = FormStartPosition.CenterScreen;

var textBox = new TextBox
{
    Multiline = true,
    ReadOnly = true,
    ScrollBars = ScrollBars.Both,
    Dock = DockStyle.Fill,
    Font = new Font(FontFamily.GenericMonospace, 9f),
    Text = sb.ToString()
};

var copyButton = new Button
{
    Text = "复制结果",
    Size = new Size(90, 32),
    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
};

var closeButton = new Button
{
    Text = "关闭",
    Size = new Size(80, 32),
    DialogResult = DialogResult.OK
};

copyButton.Click += (sender, e) =>
{
    var t = new Thread(() =>
    {
        try
        {
            Clipboard.SetText(textBox.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show("复制失败：" + ex.Message, "提示");
        }
    });
    t.SetApartmentState(ApartmentState.STA);
    t.IsBackground = true;
    t.Start();
    t.Join(500);
};

closeButton.Click += (sender, e) => dialog.Close();

var buttonPanel = new FlowLayoutPanel
{
    Dock = DockStyle.Bottom,
    Height = 50,
    FlowDirection = FlowDirection.RightToLeft,
    Padding = new Padding(10, 9, 10, 9)
};

buttonPanel.Controls.Add(closeButton);
buttonPanel.Controls.Add(copyButton);

dialog.Controls.Add(textBox);
dialog.Controls.Add(buttonPanel);

dialog.AcceptButton = closeButton;

dialog.ShowDialog();

