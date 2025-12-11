using System.IO;
using System.Windows.Forms;
using Dreamness.Ra3.Map.Facade.Util;

namespace Ra3MapUtils.ScriptViews;

public class MapFileSelectorDialog: BaseDialog
{
    public static Form Create(string title="选择地图文件")
    {
        var dialog = _create(title, 500, 180, true);
        
        // 标题标签
        Label titleLabel = new Label();
        titleLabel.Text = "请选择地图文件";
        titleLabel.Location = new System.Drawing.Point(20, 20);
        titleLabel.Size = new System.Drawing.Size(460, 25);
        titleLabel.Font = new System.Drawing.Font("Microsoft YaHei", 11F, System.Drawing.FontStyle.Bold);
        dialog.Controls.Add(titleLabel);

        // 文件路径文本框（只读，只能通过文件选择器选择）
        TextBox filePathTextBox = new TextBox();
        filePathTextBox.Location = new System.Drawing.Point(20, 55);
        filePathTextBox.Size = new System.Drawing.Size(380, 25);
        filePathTextBox.ReadOnly = true;
        filePathTextBox.BackColor = System.Drawing.SystemColors.Control;
        filePathTextBox.Name = "filePathTextBox";
        filePathTextBox.PlaceholderText = "请点击浏览按钮选择 .map 文件...";
        dialog.Controls.Add(filePathTextBox);

        // 确定按钮（先创建，因为浏览按钮需要引用它）
        Button okButton = new Button();
        okButton.Text = "确定";
        okButton.Location = new System.Drawing.Point(320, 100);
        okButton.Size = new System.Drawing.Size(75, 30);
        okButton.DialogResult = DialogResult.OK;
        okButton.Enabled = false; // 初始状态禁用，直到选择了文件
        okButton.Click += (sender, e) =>
        {
            if (string.IsNullOrWhiteSpace(filePathTextBox.Text))
            {
                MessageBox.Show(dialog, "请先选择地图文件！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dialog.DialogResult = DialogResult.None;
                return;
            }
            
            if (!File.Exists(filePathTextBox.Text))
            {
                MessageBox.Show(dialog, "选择的文件不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                dialog.DialogResult = DialogResult.None;
                return;
            }
            
            dialog.DialogResult = DialogResult.OK;
            dialog.Tag = filePathTextBox.Text;
        };

        // 浏览按钮 - 使用封装的函数创建
        Button browseButton = EasyControls.CreateBrowseButton(
            dialog: dialog,
            filePathTextBox: filePathTextBox,
            initialDirectory: Ra3PathUtil.RA3MapFolder,
            onFileSelected: (selectedFile) =>
            {
                // 当文件被选择时，启用确定按钮
                okButton.Enabled = true;
            },
            filter: "地图文件 (*.map)\0*.map\0",
            dialogTitle: "选择地图文件",
            location: new System.Drawing.Point(410, 54),
            size: new System.Drawing.Size(70, 27)
        );
        dialog.Controls.Add(browseButton);

        dialog.Controls.Add(okButton);

        // 取消按钮
        Button cancelButton = new Button();
        cancelButton.Text = "取消";
        cancelButton.Location = new System.Drawing.Point(405, 100);
        cancelButton.Size = new System.Drawing.Size(75, 30);
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Click += (sender, e) =>
        {
            dialog.DialogResult = DialogResult.Cancel;
        };
        dialog.Controls.Add(cancelButton);

        // 设置默认按钮
        dialog.AcceptButton = okButton;
        dialog.CancelButton = cancelButton;

        // 文本框双击事件：也可以打开文件选择器
        filePathTextBox.DoubleClick += (sender, e) =>
        {
            browseButton.PerformClick();
        };
        
        return dialog;
    }
}