using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LinqToDB.Tools;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.Views.SubWindows.toolbox;
using MessageBox = System.Windows.Forms.MessageBox;


namespace Ra3MapUtils.ViewModels.toolbox;

public partial class ChatLuaHelperViewModel: ObservableObject
{
    [ObservableProperty] public ChatLuaHelperModel _chatLuaHelperModel = new ChatLuaHelperModel();
    
    [ObservableProperty] private string _fileName = "";
    
    // [ObservableProperty] private string _parsedContent = "";

    [ObservableProperty] private bool _isTopmost = false;
    
    public ChatLuaHelperViewWindow _chatLuaHelperViewWindow;

    // [ObservableProperty] private bool _extremeCompress = true;
    
    // partial void OnExtremeCompressChanged(bool value)
    // {
    //     ReloadFile();
    // }

    public void AddSnippet(string comment, string snippet)
    {
        _chatLuaHelperViewWindow.ParsedCodeTextBox.Dispatcher.Invoke(() =>
        {
            var pComment = new Paragraph();
            pComment.Margin = new Thickness(0, 0, 0, 0); 
            pComment.Inlines.Add(new Run(comment) { Foreground = Brushes.Green });
            _chatLuaHelperViewWindow.ParsedCodeTextBox.Document.Blocks.Add(pComment);

            if (snippet == "/")
            {
                
            }
            else
            {
                var pSnippet = new Paragraph();
                pSnippet.Margin = new Thickness(0, 0, 0, 0); 
                pSnippet.Inlines.Add(new Run(snippet) { Foreground = Brushes.Black });
                _chatLuaHelperViewWindow.ParsedCodeTextBox.Document.Blocks.Add(pSnippet);
            }
        });
    }
    
    public void CleanSnippet()
    {
        _chatLuaHelperViewWindow.ParsedCodeTextBox.Dispatcher.Invoke(() =>
        {
            _chatLuaHelperViewWindow.ParsedCodeTextBox.Document.Blocks.Clear();
        });
    }
    
    public void OnLoad()
    {
        _chatLuaHelperModel.Reload();
        FileName = Path.GetFileName(_chatLuaHelperModel.ChatLuaHelperFilePath);
    }
    
    [RelayCommand]
    private void Closed()
    {
        GlobalVarsModel.ChatLuaHelperWindowOpened = false;
    }
    
    [RelayCommand]
    private void PickLuaFile()
    {
           OpenFileDialog openFileDialog = new OpenFileDialog();
           openFileDialog.Filter = "Lua文件|*.lua";
              if (openFileDialog.ShowDialog() == DialogResult.OK)
              {
                    ChatLuaHelperModel.ChatLuaHelperFilePath = openFileDialog.FileName;
                    FileName = openFileDialog.SafeFileName;
                    
                    ReloadFile();
              }
    }
    
    [RelayCommand]
    public void ReloadFile()
    {
        if (!File.Exists(ChatLuaHelperModel.ChatLuaHelperFilePath))
        {
            MessageBox.Show("lua文件不存在, 请重新选择");
            return;
        }
        CleanSnippet();
        var originText = File.ReadAllText(ChatLuaHelperModel.ChatLuaHelperFilePath);

        var commentConent = "";
        var snippetContent = "";
        var originLines = originText.Replace("\r\n", "\n").Split('\n').ToList();
        
        var cacheLines = new List<string>();
        bool ignoreFlag = false;
        for (int i = 0; i < originLines.Count; i++)
        {
            var currLine = originLines[i];
            if (currLine.StartsWith("--["))
            {
                ignoreFlag = true;
                continue;
            }

            if (currLine.Trim().EndsWith("--]"))
            {
                ignoreFlag = false;
                continue;
            }

            if (ignoreFlag)
            {
                continue;
            }

            if (currLine.StartsWith("--;"))
            {
                AddSnippet(commentConent, mergeAndSplitLines(cacheLines));
                
                commentConent = currLine;
                cacheLines.Clear();
            }
            else
            {
                cacheLines.Add(currLine);
            }
        }

        if (cacheLines.Count > 0)
        {
            AddSnippet(commentConent, mergeAndSplitLines(cacheLines));
        }
        


        // var originLines = list
        //     .Where(s => s != "" && (!s.StartsWith("--")))
        //     .Select(s => s.Trim() + " ")
        //     .ToList();
        //
        //     var line = "";
        //     originLines.ForEach(l =>
        //     {
        //         line += l + " ";
        //     });
        //     splitLine(line).ForEach(subLine => {
        //         resultStr += "/" + subLine + "\n";
        //     });
        
            
        

        
        // ParsedContent = resultStr;
    }

    private string mergeAndSplitLines(List<string> lines)
    {
        var filteredLines = lines
            .Where(s => s != "" && (!s.StartsWith("--")))
            .Select(s => s.Trim() + " ")
            .ToList();
        var line = "";
        filteredLines.ForEach(l =>
        {
            line += l;
        });
        var resultStr = "";
        splitLine(line).ForEach(subLine =>
        {
            resultStr += "/" + subLine + "\n";
        });
        resultStr = resultStr.Substring(0, resultStr.Length - 1);
        return resultStr;
    }

    private List<string> splitLine(string line)
    {
        var ret = new List<string>();
        // line = line.Trim();
        if (line.Length > 35)
        {
            var thisLine = line.Substring(0, 34) + "\\";
            ret.Add(thisLine);
            var nextLine = line.Substring(34);
            splitLine(nextLine).ForEach(ret.Add);
        }
        else
        {
            ret.Add(line);
        }
        return ret;
    }
}