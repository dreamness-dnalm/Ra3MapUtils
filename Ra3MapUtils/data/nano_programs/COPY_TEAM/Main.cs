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

bool ContainsChinese(string? text)
{
    if (string.IsNullOrEmpty(text))
    {
        return false;
    }

    foreach (var ch in text)
    {
        if (ch >= 0x4e00 && ch <= 0x9fff)
        {
            return true;
        }
    }

    return false;
}

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

var originTeams = ra3map.GetTeams()
    .OrderBy(t => t.FullName, StringComparer.OrdinalIgnoreCase)
    .ToList();

if (originTeams.Count == 0)
{
    throw new Exception("地图中没有可用的队伍。");
}

var playerNames = ra3map.GetPlayers()
    .Select(p => p.Name)
    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
    .ToList();

if (playerNames.Count == 0)
{
    throw new Exception("地图中没有玩家，无法生成队伍。");
}

var dialogWindow = EasyDialog.Create("批量复制队伍", 620, 360, true);
dialogWindow.StartPosition = FormStartPosition.CenterScreen;

var warningLabel = new Label
{
    Dock = DockStyle.Top,
    Height = 44,
    ForeColor = Color.Red,
    Text = "队伍配置中不要包含任何中文, 如队伍名,脚本名等; 否则100%会损坏地图文件!",
    TextAlign = ContentAlignment.MiddleLeft,
    Padding = new Padding(8, 8, 8, 0)
};

var templateCombo = new ComboBox
{
    DropDownStyle = ComboBoxStyle.DropDownList,
    Width = 360,
    DataSource = originTeams,
    DisplayMember = "FullName"
};

var playerCombo = new ComboBox
{
    DropDownStyle = ComboBoxStyle.DropDownList,
    Width = 360,
    DataSource = playerNames
};

var prefixBox = new TextBox
{
    Width = 360,
    PlaceholderText = "新队伍名前缀"
};

var countInput = new NumericUpDown
{
    Minimum = 0,
    Maximum = 9999,
    Width = 120,
    Value = 1
};

var formTable = new TableLayoutPanel
{
    Dock = DockStyle.Top,
    ColumnCount = 2,
    RowCount = 4,
    Padding = new Padding(10, 8, 10, 8),
    AutoSize = true
};
formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
formTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

formTable.Controls.Add(new Label { Text = "队伍模板：", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 0);
formTable.Controls.Add(templateCombo, 1, 0);

formTable.Controls.Add(new Label { Text = "玩家名：", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 1);
formTable.Controls.Add(playerCombo, 1, 1);

formTable.Controls.Add(new Label { Text = "名字前缀：", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 2);
formTable.Controls.Add(prefixBox, 1, 2);

formTable.Controls.Add(new Label { Text = "生成数量：", AutoSize = true, Padding = new Padding(0, 6, 0, 0) }, 0, 3);
formTable.Controls.Add(countInput, 1, 3);

var backupLabel = new Label
{
    Dock = DockStyle.Top,
    Height = 40,
    AutoSize = false,
    AutoEllipsis = true,
    ForeColor = Color.Red,
    Text = "",
    Padding = new Padding(10, 4, 0, 0),
    TextAlign = ContentAlignment.TopLeft
};

var startButton = new Button
{
    Text = "开始生成",
    Size = new Size(100, 32)
};

var closeButton = new Button
{
    Text = "关闭",
    Size = new Size(80, 32),
    DialogResult = DialogResult.Cancel
};

void ShowMessage(string text, string caption = "提示")
{
    MessageBox.Show(dialogWindow, text, caption);
}

startButton.Click += (sender, e) =>
{
    var selectedTemplate = templateCombo.SelectedItem;
    var selectedPlayer = playerCombo.SelectedItem as string;
    var prefix = prefixBox.Text?.Trim() ?? string.Empty;
    var count = (int)countInput.Value;

    if (selectedTemplate == null)
    {
        ShowMessage("请选择队伍模板。");
        return;
    }

    if (string.IsNullOrEmpty(selectedPlayer))
    {
        ShowMessage("请选择玩家名。");
        return;
    }

    if (ContainsChinese(prefix))
    {
        ShowMessage("前缀包含中文，可能会损坏地图文件，请修改。");
        return;
    }

    var existingFullNames = new HashSet<string>(
        originTeams.Select(t => t.FullName),
        StringComparer.OrdinalIgnoreCase);

    var newFullNames = new List<string>();
    for (int i = 1; i <= count; i++)
    {
        var teamName = $"{prefix}{i}";
        if (ContainsChinese(teamName) || ContainsChinese(selectedPlayer))
        {
            ShowMessage("生成的队伍名或玩家名包含中文，可能会损坏地图文件，请修改。");
            return;
        }

        var fullName = $"{selectedPlayer}/{teamName}";
        newFullNames.Add(fullName);
        if (existingFullNames.Contains(fullName))
        {
            ShowMessage($"队伍已存在：{fullName}");
            return;
        }
    }

    try
    {
        var backupPath = ra3map.Backup();
        
        backupLabel.Text = $"已备份到: {backupPath}";
        ShowMessage($"已备份到：\n{backupPath}");
    }
    catch (Exception ex)
    {
        ShowMessage("备份失败：" + ex.Message, "错误");
        return;
    }

    try
    {
        for (int i = 0; i < count; i++)
        {
            dynamic template = selectedTemplate;
            dynamic newTeam = template.Clone();

            var teamName = $"{prefix}{i + 1}";
            newTeam.Name = teamName;
            newTeam.FullName = $"{selectedPlayer}/{teamName}";
            newTeam.OwnerPlayerName = selectedPlayer;

            ra3map.AddTeam(newTeam);
        }

        ra3map.Save();
        ShowMessage($"已生成 {count} 个队伍，并保存完成。", "完成");
    }
    catch (Exception ex)
    {
        ShowMessage("生成失败：" + ex.Message, "错误");
    }
};

var buttonPanel = new FlowLayoutPanel
{
    Dock = DockStyle.Bottom,
    Height = 56,
    FlowDirection = FlowDirection.RightToLeft,
    Padding = new Padding(10, 10, 10, 10)
};

buttonPanel.Controls.Add(closeButton);
buttonPanel.Controls.Add(startButton);

dialogWindow.Controls.Add(buttonPanel);
dialogWindow.Controls.Add(backupLabel);
dialogWindow.Controls.Add(formTable);
dialogWindow.Controls.Add(warningLabel);

dialogWindow.AcceptButton = startButton;
dialogWindow.CancelButton = closeButton;

dialogWindow.ShowDialog();