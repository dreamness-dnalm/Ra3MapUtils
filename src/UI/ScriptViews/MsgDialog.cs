using System.Windows.Forms;

namespace Ra3MapUtils.ScriptViews;

public class MsgDialog: BaseDialog
{
    public static Form Create(string title = "消息", string msg = "", int width = 400, int height = 250, bool topMost = true)
    {
        var form = _create(title, width, height, topMost);
        
        var label = new Label
        {
            Text = msg,
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            AutoSize = false
        };
        form.Controls.Add(label);
        
        Button okButton = new Button
        {
            Text = "确定",
            DialogResult = DialogResult.OK,
            Anchor = AnchorStyles.Bottom,
            Width = 80,
            Height = 30,
            Top = form.ClientSize.Height - 50,
            Left = (form.ClientSize.Width - 80) / 2
        };
        form.Controls.Add(okButton);
        form.AcceptButton = okButton;
        
        return form;
    }
}