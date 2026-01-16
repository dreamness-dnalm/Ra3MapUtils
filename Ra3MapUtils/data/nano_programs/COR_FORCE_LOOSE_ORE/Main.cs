using Ra3MapUtils.ScriptViews;
using System.Windows.Forms;
using System.Threading;
using System;
using System.IO;
using System.Drawing;
using Dreamness.Ra3.Map.Facade.Core;
using Dreamness.RA3.Map.Parser.Asset.Impl.MissionObjective;


enum LooseOreState
{
    None,
    ForceEnable,
    ForceDisable
}

string DescribeState(LooseOreState state) => state switch
{
    LooseOreState.None => "无",
    LooseOreState.ForceEnable => "强制开启散矿",
    LooseOreState.ForceDisable => "强制禁用散矿",
    _ => "未知"
};

LooseOreState GetCurrentState(IEnumerable<MissionObjective> objectives)
{
    var hasForceEnable = objectives.Any(o => o.Id == "ForceLooseOre1");
    var hasForceDisable = objectives.Any(o => o.Id == "ForceLooseOre0");

    if (hasForceEnable)
    {
        return LooseOreState.ForceEnable;
    }

    if (hasForceDisable)
    {
        return LooseOreState.ForceDisable;
    }

    return LooseOreState.None;
}

void ApplyState(Ra3MapFacade ra3map, LooseOreState targetState)
{
    // 每次操作前获取最新的目标对象，避免使用旧引用
    var objectives = ra3map.GetMissionObjectives();
    var enableObj = objectives.FirstOrDefault(o => o.Id == "ForceLooseOre1");
    var disableObj = objectives.FirstOrDefault(o => o.Id == "ForceLooseOre0");

    if (enableObj != null)
    {
        ra3map.Remove(enableObj);
    }
    if (disableObj != null)
    {
        ra3map.Remove(disableObj);
    }

    if (targetState == LooseOreState.ForceEnable)
    {
        ra3map.AddMissionObjective("ForceLooseOre1", "", "");
    }
    else if (targetState == LooseOreState.ForceDisable)
    {
        ra3map.AddMissionObjective("ForceLooseOre0", "", "");
    }

    ra3map.Save();
}

// Dictionary<string, string> ArgumentDictionary = null;
string? mapFilePath = null;

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

var missionObjectives = ra3map.GetMissionObjectives();
var currentState = GetCurrentState(missionObjectives);

// 构建设置窗口
var settingDialog = EasyDialog.Create("散矿逻辑设置", 420, 260, true);
settingDialog.StartPosition = FormStartPosition.CenterScreen;

var titleLabel = new Label
{
    Text = "请选择散矿逻辑：",
    AutoSize = true,
    Location = new Point(20, 20)
};

var mapLabel = new Label
{
    Text = $"地图：{Path.GetFileName(mapFilePath)}",
    AutoSize = true,
    Location = new Point(20, 45)
};

var currentLabel = new Label
{
    Text = $"当前状态：{DescribeState(currentState)}",
    AutoSize = true,
    Location = new Point(20, 70)
};

var radioNone = new RadioButton
{
    Text = "无",
    AutoSize = true,
    Location = new Point(20, 105)
};

var radioForceEnable = new RadioButton
{
    Text = "强制开启散矿",
    AutoSize = true,
    Location = new Point(20, 135)
};

var radioForceDisable = new RadioButton
{
    Text = "强制禁用散矿",
    AutoSize = true,
    Location = new Point(20, 165)
};

switch (currentState)
{
    case LooseOreState.ForceEnable:
        radioForceEnable.Checked = true;
        break;
    case LooseOreState.ForceDisable:
        radioForceDisable.Checked = true;
        break;
    default:
        radioNone.Checked = true;
        break;
}

var confirmButton = new Button
{
    Text = "确定",
    Size = new Size(80, 32),
    Location = new Point(settingDialog.ClientSize.Width - 100, settingDialog.ClientSize.Height - 60),
    Anchor = AnchorStyles.Bottom | AnchorStyles.Right
};

confirmButton.Click += (sender, e) =>
{
    settingDialog.DialogResult = DialogResult.OK;
    settingDialog.Close();
};

settingDialog.AcceptButton = confirmButton;
settingDialog.Controls.Add(titleLabel);
settingDialog.Controls.Add(mapLabel);
settingDialog.Controls.Add(currentLabel);
settingDialog.Controls.Add(radioNone);
settingDialog.Controls.Add(radioForceEnable);
settingDialog.Controls.Add(radioForceDisable);
settingDialog.Controls.Add(confirmButton);

var dialogResult = settingDialog.ShowDialog();
if (dialogResult != DialogResult.OK)
{
    // 用户取消或关闭窗口，不做任何修改
    return;
}

var targetState = radioForceEnable.Checked
    ? LooseOreState.ForceEnable
    : radioForceDisable.Checked
        ? LooseOreState.ForceDisable
        : LooseOreState.None;

ApplyState(ra3map, targetState);

MessageBox.Show($"已将散矿状态设置为：{DescribeState(targetState)}", "完成");