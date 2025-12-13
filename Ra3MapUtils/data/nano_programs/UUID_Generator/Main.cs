using Ra3MapUtils.ScriptViews;
using System.Windows.Forms;
using System.Threading;
using System;


var dialog = EasyDialog.Create("UUID 生成器", 400, 200, true);

var label = new Label();
dialog.Controls.Add(label);

var generateButton = new Button();
generateButton.Text = "生成并复制 UUID";
generateButton.Size = new System.Drawing.Size(150, 40);
generateButton.Location = new System.Drawing.Point((dialog.ClientSize.Width - generateButton.Width) / 2, (dialog.ClientSize.Height - generateButton.Height) / 2);
generateButton.Click += (sender, e) =>
{
    var uuid = System.Guid.NewGuid().ToString();

    // 在单线程单元(STA)线程上执行剪贴板操作，避免在非-STA 环境（如 Roslyn 脚本）中抛出异常
    var t = new Thread(() =>
    {
        try
        {
            Clipboard.SetText(uuid);
        }
        catch (Exception ex)
        {
            // 若复制失败，把错误显示在 label 上（UI 线程会继续设置 uuid 文本）
            try { label.Text = "复制失败: " + ex.Message; } catch { }
        }
    });
    t.SetApartmentState(ApartmentState.STA);
    t.IsBackground = true;
    t.Start();
    // 等待短时间以提高复制成功概率，但不阻塞太久
    t.Join(500);

    label.Text = uuid;
    label.AutoSize = true;
    label.Location = new System.Drawing.Point((dialog.ClientSize.Width - label.Width) / 2, generateButton.Bottom + 10);
};
dialog.Controls.Add(generateButton);

dialog.ShowDialog();
