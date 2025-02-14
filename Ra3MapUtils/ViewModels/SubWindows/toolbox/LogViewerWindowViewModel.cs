﻿using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.ViewModels.MainWindowPages;
using Ra3MapUtils.Views.SubWindows.toolbox;
using SharedFunctionLib.Business;

namespace Ra3MapUtils.ViewModels.toolbox;

public partial class LogViewerWindowViewModel: ObservableObject
{
    [ObservableProperty] private int _logLevelIndex = 1;

    [ObservableProperty] private string _logFilePath = "";

    [ObservableProperty] private string _logFileName = "";

    [ObservableProperty] private string _parseStatus = "已停止";
    
    [ObservableProperty] private Brush _parseStatusColor = Brushes.Red;

    [ObservableProperty] private bool _isHideParseErrorLogs = false;
    
    [ObservableProperty] private bool _isParseLogTime = false;

    [ObservableProperty] private bool _isParseFrameIndex = true;

    [ObservableProperty] private bool _isParseLogLeve = true;
    
    [ObservableProperty] private bool _isParseLogPosition = true;
    
    // [ObservableProperty] 
    public ObservableCollection<RichTextLine> TextLines { get; } = new ObservableCollection<RichTextLine>();
    
    private SettingPageViewModel _settingPageViewModel = App.Current.Services.GetRequiredService<SettingPageViewModel>();
    
    public LogViewerWindow _logViewerWindow;

    private string logLevel = "DEBUG";

    private long lastParsePos = 0;

    private CancellationTokenSource _cancelTokenSource;
    
    private Dictionary<string, int> logLevelDict = new Dictionary<string, int>
    {
        {"TRACE", 0},
        {"DEBUG", 1},
        {"INFO", 2},
        {"WARN", 3},
        {"ERROR", 4}
    };
    
    private Dictionary<string, Brush> logLevelColorDict = new Dictionary<string, Brush>
    {
        {"TRACE", Brushes.DimGray},
        {"DEBUG", Brushes.DodgerBlue},
        {"INFO", Brushes.LimeGreen},
        {"WARN", Brushes.Gold},
        {"ERROR", Brushes.Crimson}
    };
    

    [ObservableProperty] private bool _isParsing = false;

    public void OnLoad()
    {
        // todo: 初始化状态
    }

    [RelayCommand]
    public void AutoLoadLogFile()
    {
        if (!NewWorldBuilderBusiness.IsNewWorldBuilderPathValid)
        {
            MessageBox.Show("请先在设置中绑定新地编");
            return;
        }
        
        var logDir = Path.Combine(Path.GetDirectoryName(NewWorldBuilderBusiness.NewWorldBuilderPath), "game", "logs");
        if (!Directory.Exists(logDir))
        {
            MessageBox.Show("日志目录不存在: " + logDir);
            return;
        }
        string pattern = @"ra3GameDebug-(\d{4})-(\d{1,2})-(\d{1,2})\.dll\.log";
        var latestLogFileName = Directory.GetFiles(logDir,"*.dll.log")
            .Select(f => new {FileName=f, Match = Regex.Match(Path.GetFileName(f), pattern)})
            .Where(x => x.Match.Success)
            .Select(x => new
            {
                x.FileName,
                Year = int.Parse(x.Match.Groups[1].Value),
                Month = int.Parse(x.Match.Groups[2].Value),
                Day = int.Parse(x.Match.Groups[3].Value)
            }).OrderByDescending(x => new DateTime(x.Year, x.Month, x.Day))
            .Select(x => x.FileName)
            .FirstOrDefault("");
        if (latestLogFileName == "")
        {
            MessageBox.Show("没有找到日志文件");
            return;
        }

        LogFilePath = latestLogFileName;
        LogFileName = Path.GetFileName(LogFilePath);
    }
    
    [RelayCommand]
    private void PickLogFile()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = "日志文件|*.log;*.txt";
        
        if (NewWorldBuilderBusiness.IsNewWorldBuilderPathValid)
        {
            openFileDialog.InitialDirectory = Path.Combine(Path.GetDirectoryName(NewWorldBuilderBusiness.NewWorldBuilderPath), "game", "logs");
        }
        
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            LogFilePath = openFileDialog.FileName;
            LogFileName = Path.GetFileName(LogFilePath);
        }
    }

    [RelayCommand]
    private void StartFromBeginning()
    {
        if (LogFilePath == "")
        {
            MessageBox.Show("请先选择日志文件");
        }

        if (!File.Exists(LogFilePath))
        {
            MessageBox.Show("日志文件不存在: " + LogFilePath);
        }
        ClearLog();
        _cancelTokenSource = new CancellationTokenSource();
        Task.Run(async () =>
        {
            await StartParse(-1, _cancelTokenSource.Token);
        });
    }

    [RelayCommand]
    private void StartFromLatest()
    {
        if (LogFilePath == "")
        {
            MessageBox.Show("请先选择日志文件");
        }

        if (!File.Exists(LogFilePath))
        {
            MessageBox.Show("日志文件不存在: " + LogFilePath);
        }
        _cancelTokenSource = new CancellationTokenSource();
        Task.Run(async () =>
        {
            await StartParse(0, _cancelTokenSource.Token);
        });
    }

    [RelayCommand]
    private void StartFromLastStopPos()
    {
        if (LogFilePath == "")
        {
            MessageBox.Show("请先选择日志文件");
        }

        if (!File.Exists(LogFilePath))
        {
            MessageBox.Show("日志文件不存在: " + LogFilePath);
        }
        _cancelTokenSource = new CancellationTokenSource();
        Task.Run(async () =>
        {
            await StartParse(lastParsePos, _cancelTokenSource.Token);
        });
    }

    [RelayCommand]
    private void Stop()
    {
        _cancelTokenSource.CancelAsync();
        ParseStatus = "停止中...";
        ParseStatusColor = Brushes.Red;
        IsParsing = false;
    }
    
    [RelayCommand]
    private void cleanLog()
    {
        TextLines.Clear();
    }

    [RelayCommand]
    private void SwitchLogFile()
    {
        
    }
    
    [RelayCommand]
    private void SwitchLogLevel()
    {
        MessageBox.Show("SwitchLogLevel: " + _logLevelIndex);
    }
    
    [RelayCommand]
    private void ClearLog()
    {
        _logViewerWindow.LogTextBox.Document.Blocks.Clear();
    }
    
    private string normalLogPattern = @"\[(?<time>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3})\].*?\](?<frame>\d+)\s(?<detail>.+)";
    private string systemLogPattern = @"\[(?<time>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3})\]\s.*?\]\s(?<detail>.+)";
    string detailLogpattern = @"<(?<level>\w+)><pos:(?<position>[^>]+)>(?<content>.+)";
    

    private void AddLog(string log)
    {
        string time = "";
        string frameIndex = "";
        string logDetail = "";
        string logLevel = "";
        string logContent = "";
        string logPos = "";
        bool isParseError = false;
        
        var normalLogMatch = Regex.Match(log, normalLogPattern);
        if (normalLogMatch.Success)
        {
            time = normalLogMatch.Groups["time"].Value;
            frameIndex = normalLogMatch.Groups["frame"].Value;
            logDetail = normalLogMatch.Groups["detail"].Value;

            var detailMatch = Regex.Match(logDetail, detailLogpattern);
            if (detailMatch.Success)
            {
                logLevel = detailMatch.Groups["level"].Value;
                logPos = detailMatch.Groups["position"].Value;
                logContent = detailMatch.Groups["content"].Value;
            }
            else
            {
                logLevel = "TRACE";
                logContent = logDetail;
            }
        }
        else
        {
            var systemLogMatch = Regex.Match(log, systemLogPattern);
            if (systemLogMatch.Success)
            {
                time = systemLogMatch.Groups["time"].Value;
                logContent = systemLogMatch.Groups["detail"].Value;
                logLevel = "INFO";
            }
            else
            {
                logContent = log;
            }
        }
        
        if (logLevelDict.GetValueOrDefault(logLevel, 2) < _logLevelIndex)
        {
            return;
        }

        string finalLogConent = "";

        if (IsParseFrameIndex && frameIndex != "")
        {
            finalLogConent += $"{frameIndex}";
        }

        if (IsParseLogTime && time != "")
        {
            finalLogConent += $"[{time}]";
        }

        if (IsParseLogLeve && logLevel != "")
        {
            finalLogConent += $"[{logLevel}]";
        }
        
        if(IsParseLogPosition && logPos != "")
        {
            finalLogConent += $"[{logPos}]";
        }

        finalLogConent += logContent;
        
        TextLines.Add(new RichTextLine(finalLogConent, logLevelColorDict.GetValueOrDefault(logLevel, Brushes.Black)));
    }

    private async Task<bool> StartParse(long pos, CancellationToken token)
    {
        ParseStatus = "正在解析";
        ParseStatusColor = Brushes.Green;
        IsParsing = true;
        
        try
        {
            // 以共享读写方式打开文件，确保其他程序也可以写入
            using (FileStream fs = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                if (pos > 0)
                {
                    fs.Seek(pos, SeekOrigin.Begin);
                }
                else if (pos == 0)
                {
                    
                }
                else
                {
                    fs.Seek(0, SeekOrigin.End);
                }
               

                while (true)
                {
                    if(token.IsCancellationRequested)
                    {
                        break;
                    }
                    // 读取新追加的行
                    string line = reader.ReadLine();
                    if (line != null)
                    {
                        AddLog(line);
                        lastParsePos = fs.Position;
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("发生异常: " + ex.Message);
        }
        ParseStatus = "已停止";
        ParseStatusColor = Brushes.Red;
        IsParsing = false;
        return true;
    }
    

}

public class RichTextLine
{
    public string Text { get; set; }
    public Brush Color { get; set; }

    public RichTextLine(string text, Brush color)
    {
        Text = text;
        Color = color;
    }
}
