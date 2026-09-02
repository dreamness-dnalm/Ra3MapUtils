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

var mapWidth = ra3map.MapWidth;
var mapHeight = ra3map.MapHeight;
var totalTiles = (long)mapWidth * mapHeight;

if (totalTiles <= 0)
{
    throw new Exception("地图尺寸无效，无法统计纹理。");
}

var textureCounter = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

for (int x = 0; x < mapWidth; x++)
{
    for (int y = 0; y < mapHeight; y++)
    {
        var textureName = ra3map.GetTileTexture(x, y) ?? "Unknown";

        if (textureCounter.TryGetValue(textureName, out var cnt))
        {
            textureCounter[textureName] = cnt + 1;
        }
        else
        {
            textureCounter[textureName] = 1;
        }
    }
}

var ordered = textureCounter
    .OrderByDescending(kv => kv.Value)
    .ThenBy(kv => kv.Key)
    .ToList();

var sb = new StringBuilder();
sb.AppendLine($"地图：{Path.GetFileName(mapFilePath)}");
sb.AppendLine($"尺寸：{mapWidth} x {mapHeight}（含边界）");
sb.AppendLine($"总面积：{totalTiles}");
sb.AppendLine($"纹理种类：{ordered.Count}");
sb.AppendLine();

for (int i = 0; i < ordered.Count; i++)
{
    var (name, count) = ordered[i];
    var percent = count * 100.0 / totalTiles;
    sb.AppendLine($"{i + 1}. {name} - {percent:F2}% ，面积：{count}");
}

sb.AppendLine();
sb.AppendLine("提示：面积=纹理覆盖的网格数量。");

var dialog = EasyDialog.Create("纹理统计结果", 640, 720, true);
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

