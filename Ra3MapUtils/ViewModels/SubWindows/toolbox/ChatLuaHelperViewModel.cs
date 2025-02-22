using System.IO;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;


namespace Ra3MapUtils.ViewModels.toolbox;

public partial class ChatLuaHelperViewModel: ObservableObject
{
    [ObservableProperty] public ChatLuaHelperModel _chatLuaHelperModel = new ChatLuaHelperModel();
    
    [ObservableProperty] private string _fileName = "";
    
    [ObservableProperty] private string _parsedContent = "";

    [ObservableProperty] private bool _isTopmost = false;

    // [ObservableProperty] private bool _extremeCompress = true;
    
    // partial void OnExtremeCompressChanged(bool value)
    // {
    //     ReloadFile();
    // }
    
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

        var originText = File.ReadAllText(ChatLuaHelperModel.ChatLuaHelperFilePath);
        
        var resultStr = "";
        var originLines = originText.Replace("\r\n", "\n").Split('\n').ToList()
            .Where(s => s != "" && (!s.StartsWith("--")))
            .Select(s => s.Trim() + " ")
            .ToList();
        
            var line = "";
            originLines.ForEach(l =>
            {
                line += l + " ";
            });
            splitLine(line).ForEach(subLine => {
                resultStr += "/" + subLine + "\n";
            });
        
        
        
        // else
        // {
        //     originLines.ForEach(line =>
        //     {
        //             var originLines = splitLine(line);
        //             
        //             originLines.ForEach(subLine =>
        //             {
        //                 resultStr += "/" + subLine + "\n";
        //             });
        //       
        //     });
        // }
        

        
        ParsedContent = resultStr;
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