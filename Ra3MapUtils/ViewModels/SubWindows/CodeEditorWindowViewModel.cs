using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Ra3MapUtils.ViewModels;

public partial class CodeEditorWindowViewModel: ObservableObject
{
    public TextEditor _textEditor;
    
    [ObservableProperty] private string _windowTitle;

    [ObservableProperty] private string _code;

    [ObservableProperty] private string _filePath;

    private Dictionary<string, Encoding> _encodings = new Dictionary<string, Encoding> 
        {
            {"UTF-8", Encoding.UTF8},
            {"GBK", Encoding.GetEncoding("GB18030")},
            {"GB2312", Encoding.GetEncoding("GB2312")},
            {"UTF-16", Encoding.Unicode},
            {"UTF-32", Encoding.UTF32},
            {"ASCII", Encoding.ASCII},
            {"BigEndianUnicode", Encoding.BigEndianUnicode},
            {"UTF-7", Encoding.UTF7},
            {"UTF-8 BOM", new UTF8Encoding(true)},
            {"UTF-16 BOM", new UnicodeEncoding(false, true)},
            {"UTF-32 BOM", new UTF32Encoding(false, true)},
            {"ASCII BOM", new ASCIIEncoding()}
        };

    [ObservableProperty] private ObservableCollection<string> _encodingNames = new ObservableCollection<string>();

    [ObservableProperty] private string _selectedEncoding = "UTF-8";

    partial void OnSelectedEncodingChanged(string value)
    {
        ReloadContent();
    }
    
    partial void OnFilePathChanged(string value)
    {
        WindowTitle = $"代码预览 - {value}";
        
        if (EncodingNames.Count == 0)
        {
            foreach (var (encodeName, _) in _encodings)
            {
                EncodingNames.Add(encodeName);
            }
            
            using (FileStream fs = File.OpenRead("lua4.xshd"))
            {
                using (XmlReader reader = new XmlTextReader(fs))
                {
                    _textEditor.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
        
        ReloadContent();
    }

    [RelayCommand]
    private void ReloadContent()
    {
        try
        {
            Code = File.ReadAllText(_filePath, _encodings[SelectedEncoding]);
        }
        catch (Exception e)
        {
            Code = "文件读取失败, because: " + e.Message;
        }

        _textEditor.Text = Code;
    }
}