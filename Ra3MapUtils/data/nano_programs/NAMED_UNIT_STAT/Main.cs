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

var namedUnits = ra3map.GetUnitObjects()
    .Where(o => !string.IsNullOrWhiteSpace(o.ObjName))
    .Select(o =>
    {
        var pos = o.Position;
        return new NamedUnit(
            o.ObjName!.Trim(),
            string.IsNullOrWhiteSpace(o.TypeName) ? "<Unknown>" : o.TypeName!.Trim(),
            pos.X,
            pos.Y);
    })
    .OrderBy(u => u.Name, StringComparer.OrdinalIgnoreCase)
    .ThenBy(u => u.Type, StringComparer.OrdinalIgnoreCase)
    .ToList();

var dialog = EasyDialog.Create("命名单位统计", 860, 760, true);
dialog.StartPosition = FormStartPosition.CenterScreen;

var hintLabel = new Label
{
    Dock = DockStyle.Top,
    Height = 24,
    Text = "命名请使用英文，否则会显示乱码",
    TextAlign = ContentAlignment.MiddleLeft,
    Padding = new Padding(8, 4, 0, 0)
};

var searchBox = new TextBox { Width = 260 };

var filterPanel = new FlowLayoutPanel
{
    Dock = DockStyle.Top,
    Height = 40,
    Padding = new Padding(8, 8, 8, 4),
    FlowDirection = FlowDirection.LeftToRight
};

filterPanel.Controls.Add(new Label { Text = "搜索（命名/类型）：", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
filterPanel.Controls.Add(searchBox);

var listView = new ListView
{
    View = View.Details,
    Dock = DockStyle.Fill,
    FullRowSelect = true,
    GridLines = true
};

listView.Columns.Add("命名", 240);
listView.Columns.Add("类型", 320);
listView.Columns.Add("位置 (x, y)", 200);

var counterLabel = new Label
{
    Dock = DockStyle.Bottom,
    Height = 26,
    TextAlign = ContentAlignment.MiddleLeft,
    Padding = new Padding(8, 0, 0, 0)
};

List<NamedUnit> currentView = namedUnits;

void RefreshList()
{
    var keyword = searchBox.Text?.Trim() ?? string.Empty;

    bool Match(NamedUnit unit)
    {
        if (string.IsNullOrEmpty(keyword))
        {
            return true;
        }

        return unit.Name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0
            || unit.Type.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    currentView = namedUnits.Where(Match).ToList();

    listView.BeginUpdate();
    listView.Items.Clear();
    foreach (var unit in currentView)
    {
        var item = new ListViewItem(unit.Name);
        item.SubItems.Add(unit.Type);
        item.SubItems.Add($"({unit.X:F2}, {unit.Y:F2})");
        listView.Items.Add(item);
    }
    listView.EndUpdate();

    counterLabel.Text = $"命名单位：{namedUnits.Count}，当前显示：{currentView.Count}";
}

searchBox.TextChanged += (sender, e) => RefreshList();

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
            var sb = new StringBuilder();
            sb.AppendLine($"地图：{Path.GetFileName(mapFilePath)}");
            foreach (var unit in currentView)
            {
                sb.AppendLine($"{unit.Name} | {unit.Type} | ({unit.X:F2}, {unit.Y:F2})");
            }

            Clipboard.SetText(sb.ToString());
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

dialog.Controls.Add(listView);
dialog.Controls.Add(filterPanel);
dialog.Controls.Add(hintLabel);
dialog.Controls.Add(counterLabel);
dialog.Controls.Add(buttonPanel);

dialog.AcceptButton = closeButton;

RefreshList();

if (namedUnits.Count == 0)
{
    MessageBox.Show("未找到任何命名单位。", "提示");
}

dialog.ShowDialog();

