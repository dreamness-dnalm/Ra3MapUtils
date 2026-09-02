using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Ra3MapUtils.ScriptViews;

public class EasyControls
{
    // Windows API 声明 - 使用原生文件对话框，避免 Roslyn 嵌套对话框问题
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public IntPtr lpstrFilter;
        public IntPtr lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public IntPtr lpstrFileTitle;
        public int nMaxFileTitle;
        public IntPtr lpstrInitialDir;
        public IntPtr lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public IntPtr lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public IntPtr lpTemplateName;
    }
    
    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    // 创建文件浏览按钮的函数
    public static Button CreateBrowseButton(Form dialog, TextBox filePathTextBox, 
        Action<string> onFileSelected = null,
        string filter="所有文件 (*.*)\0*.*\0", 
        string dialogTitle="选择文件",
        string initialDirectory = null,
        System.Drawing.Point? location = null,
        System.Drawing.Size? size = null)
    {
        Button browseButton = new Button();
        browseButton.Text = "浏览...";
        browseButton.Location = location ?? new System.Drawing.Point(410, 54);
        browseButton.Size = size ?? new System.Drawing.Size(70, 27);
        browseButton.UseVisualStyleBackColor = true;
        
        browseButton.Click += (sender, e) =>
        {
            browseButton.Enabled = false;
            
            try
            {
                // 使用 Windows API 直接调用文件对话框，避免 Roslyn 嵌套对话框问题
                OpenFileName ofn = new OpenFileName();
                ofn.lStructSize = Marshal.SizeOf(typeof(OpenFileName));
                
                // 分配文件名缓冲区
                int bufferSize = 260;
                IntPtr fileNameBuffer = Marshal.AllocHGlobal(bufferSize * 2); // Unicode 需要 2 字节
                Marshal.WriteInt16(fileNameBuffer, 0); // 初始化为空字符串
                
                // 分配过滤器字符串
                IntPtr filterPtr = Marshal.StringToHGlobalAuto(filter);
                
                // 分配标题字符串
                IntPtr titlePtr = Marshal.StringToHGlobalAuto(dialogTitle);
                
                // 分配初始文件夹字符串（如果提供）
                IntPtr initialDirPtr = IntPtr.Zero;
                if (!string.IsNullOrWhiteSpace(initialDirectory))
                {
                    // 确保路径存在且是目录
                    if (Directory.Exists(initialDirectory))
                    {
                        initialDirPtr = Marshal.StringToHGlobalAuto(initialDirectory);
                    }
                }
                
                try
                {
                    ofn.lpstrFile = fileNameBuffer;
                    ofn.nMaxFile = bufferSize;
                    ofn.lpstrFilter = filterPtr;
                    ofn.nFilterIndex = 1;
                    ofn.lpstrTitle = titlePtr;
                    ofn.lpstrInitialDir = initialDirPtr;
                    ofn.Flags = 0x00080000 | 0x00001000 | 0x00000800; // OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST
                    
                    // 确保对话框 Handle 已创建
                    if (dialog.IsHandleCreated)
                    {
                        ofn.hwndOwner = dialog.Handle;
                    }
                    else
                    {
                        ofn.hwndOwner = IntPtr.Zero;
                    }
                    
                    if (GetOpenFileName(ref ofn))
                    {
                        string selectedFile = Marshal.PtrToStringAuto(fileNameBuffer);
                        if (!string.IsNullOrWhiteSpace(selectedFile))
                        {
                            filePathTextBox.Text = selectedFile;
                            filePathTextBox.BackColor = System.Drawing.SystemColors.Window;
                            // 调用回调函数
                            onFileSelected?.Invoke(selectedFile);
                        }
                    }
                }
                finally
                {
                    // 释放分配的内存
                    Marshal.FreeHGlobal(fileNameBuffer);
                    Marshal.FreeHGlobal(filterPtr);
                    Marshal.FreeHGlobal(titlePtr);
                    if (initialDirPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(initialDirPtr);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打开文件对话框时出错: {ex.Message}");
                System.Windows.Forms.MessageBox.Show(dialog, $"打开文件对话框时出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                browseButton.Enabled = true;
            }
        };
        
        return browseButton;
    }
}