using Ra3MapUtils.ScriptViews;
using System.Windows.Forms;
using System.Threading;
using System;
using System.IO;
using System.Drawing;
using Dreamness.Ra3.Map.Facade.Core;
using Dreamness.RA3.Map.Parser.Asset.Impl.MissionObjective;


enum EnhanceModeState
{
    None,
    ForceEnable,
    ForceDisable
}

string DescribeState(EnhanceModeState state) => state switch
{
    EnhanceModeState.None => "无",
    EnhanceModeState.ForceEnable => "强制开启拓展模式",
    _ => "未知"
};

EnhanceModeState GetCurrentState(IEnumerable<MissionObjective> objectives)
{
    var hasForceEnable = objectives.Any(o => o.Id == "ForceCoronaPVE=1");

    if (hasForceEnable)
    {
        return EnhanceModeState.ForceEnable;
    }
    

    return EnhanceModeState.None;
}

void ApplyState(Ra3MapFacade ra3map, EnhanceModeState targetState)
{
    // 每次操作前获取最新的目标对象，避免使用旧引用
    var objectives = ra3map.GetMissionObjectives();
    var enableObj = objectives.FirstOrDefault(o => o.Id == "ForceCoronaPVE=1");

    if (enableObj != null)
    {
        ra3map.Remove(enableObj);
    }

    if (targetState == EnhanceModeState.ForceEnable)
    {
        ra3map.AddMissionObjective("ForceCoronaPVE=1", "", "");
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
var settingDialog = EasyDialog.Create("拓展模式设置", 420, 260, true);
settingDialog.StartPosition = FormStartPosition.CenterScreen;

var titleLabel = new Label
{
    Text = "请选择拓展模式逻辑：",
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
    Text = "强制开启拓展模式",
    AutoSize = true,
    Location = new Point(20, 135)
};


switch (currentState)
{
    case EnhanceModeState.ForceEnable:
        radioForceEnable.Checked = true;
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
settingDialog.Controls.Add(confirmButton);

var dialogResult = settingDialog.ShowDialog();
if (dialogResult != DialogResult.OK)
{
    // 用户取消或关闭窗口，不做任何修改
    return;
}

var targetState = radioForceEnable.Checked
    ? EnhanceModeState.ForceEnable
    : EnhanceModeState.None;

ApplyState(ra3map, targetState);

MessageBox.Show($"已将拓展模式状态设置为：{DescribeState(targetState)}", "完成");