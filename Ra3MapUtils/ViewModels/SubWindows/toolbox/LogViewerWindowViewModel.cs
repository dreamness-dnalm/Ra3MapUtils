using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Ra3MapUtils.Models;
using Ra3MapUtils.ViewModels.MainWindowPages;
using Ra3MapUtils.Views.SubWindows.toolbox;
using SharedFunctionLib.Business;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Ra3MapUtils.ViewModels.toolbox;

public partial class LogViewerWindowViewModel: ObservableObject
{
    [ObservableProperty] private int _logLevelIndex = 1;

    [ObservableProperty] private string _logFilePath = "";

    [ObservableProperty] private string _logFileName = "";

    [ObservableProperty] private string _parseStatus = "已停止";
    
    [ObservableProperty] private Brush _parseStatusColor = Brushes.PaleVioletRed;

    [ObservableProperty] private bool _isHideParseErrorLogs = false;
    
    [ObservableProperty] private bool _isParseLogTime = false;

    [ObservableProperty] private bool _isParseFrameIndex = true;

    [ObservableProperty] private bool _isParseLogLeve = true;
    
    [ObservableProperty] private bool _isParseLogPosition = true;

    // [ObservableProperty] private int _showingCount = 2000;
    
    // [ObservableProperty] 
    // public ObservableCollection<RichTextLine> TextLines { get; } = new ObservableCollection<RichTextLine>();
    
    private SettingPageViewModel _settingPageViewModel = App.Current.Services.GetRequiredService<SettingPageViewModel>();
    
    public LogViewerWindow _logViewerWindow;

    private string logLevel = "DEBUG";

    private LogConsumerStatusModel _logConsumerStatusModel = new LogConsumerStatusModel();

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
        AutoLoadLogFile();
    }
    
    [RelayCommand]
    private void Closed()
    {
        if (IsParsing)
        {
            Stop();
        }
        
        cleanLog();
        GlobalVarsModel.LogViewerWindowOpened = false;
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
            return;
        }

        if (!File.Exists(LogFilePath))
        {
            MessageBox.Show("日志文件不存在: " + LogFilePath);
            return;
        }
        ClearLog();
        _cancelTokenSource = new CancellationTokenSource();
        Task.Run(async () =>
        {
            _logConsumerStatusModel.Pos = 0;
            await StartParse(_cancelTokenSource.Token);
        });
    }

    [RelayCommand]
    private void StartFromLatest()
    {
        if (LogFilePath == "")
        {
            MessageBox.Show("请先选择日志文件");
            return;
        }

        if (!File.Exists(LogFilePath))
        {
            MessageBox.Show("日志文件不存在: " + LogFilePath);
            return;
        }
        _cancelTokenSource = new CancellationTokenSource();
        Task.Run(async () =>
        {
            _logConsumerStatusModel.Pos = -1;
            await StartParse(_cancelTokenSource.Token);
        });
    }

    [RelayCommand]
    private void StartFromLastStopPos()
    {
        if (LogFilePath == "")
        {
            MessageBox.Show("请先选择日志文件");
            return;
        }

        if (!File.Exists(LogFilePath))
        {
            MessageBox.Show("日志文件不存在: " + LogFilePath);
            return;
        }
        _cancelTokenSource = new CancellationTokenSource();
        Task.Run(async () =>
        {
            await StartParse(_cancelTokenSource.Token);
        });
    }

    [RelayCommand]
    private void Stop()
    {
        _cancelTokenSource.CancelAsync();
        ParseStatus = "停止中...";
        ParseStatusColor = Brushes.PaleVioletRed;
        IsParsing = false;
    }
    
    [RelayCommand]
    private void cleanLog()
    {
        _logViewerWindow.LogTextBox.Document.Blocks.Clear();
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
    private string systemLogPattern =  @"\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\]\s\[\w+\]\s\[(?<level>\w+)\]\s(?<detail>.+)";
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
        
        var normalLogMatch = Regex.Match(log, normalLogPattern, RegexOptions.Singleline);
        if (normalLogMatch.Success)
        {
            time = normalLogMatch.Groups["time"].Value;
            frameIndex = normalLogMatch.Groups["frame"].Value;
            logDetail = normalLogMatch.Groups["detail"].Value;

            var detailMatch = Regex.Match(logDetail, detailLogpattern, RegexOptions.Singleline);
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
            var systemLogMatch = Regex.Match(log, systemLogPattern, RegexOptions.Singleline);
            if (systemLogMatch.Success)
            {
                time = systemLogMatch.Groups["time"].Value;
                logContent = systemLogMatch.Groups["detail"].Value;
                logLevel = systemLogMatch.Groups["level"].Value.ToUpper();
                if (logLevel == "WARNING")
                {
                    logLevel = "WARN";
                }
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
        
        _logViewerWindow.LogTextBox.Dispatcher.Invoke(() =>
        {
        var paragraph = new Paragraph();
        var run = new Run(finalLogConent) { Foreground = logLevelColorDict.GetValueOrDefault(logLevel, Brushes.Black) };
        paragraph.Margin = new Thickness(0, 0, 0, 0); 
        paragraph.Inlines.Add(run);
        _logViewerWindow.LogTextBox.Document.Blocks.Add(paragraph);
            });
        
        
    }

    private async Task<bool> StartParse(CancellationToken token)
    {
        ParseStatus = "正在解析";
        ParseStatusColor = Brushes.Green;
        IsParsing = true;

        var newLogPattern = @"^\[\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}\] \[ra3GameDebug\] \[(?<level>\w+)\]";
        
        try
        {
            // 以共享读写方式打开文件，确保其他程序也可以写入
            using (FileStream fs = new FileStream(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fs))
            {
                if (_logConsumerStatusModel.Pos > 0)
                {
                    fs.Seek(_logConsumerStatusModel.Pos, SeekOrigin.Begin);
                }
                else if (_logConsumerStatusModel.Pos == 0)
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

                    var content = _logConsumerStatusModel.Cache + reader.ReadToEnd();
                    var lines = content.Split("\r\n");
                    var linesCnt = lines.Length;
                    _logConsumerStatusModel.Cache = "";
                    for (int i = 0; i < linesCnt - 1; i++)
                    {
                        var line = lines[i];
                        if(Regex.IsMatch(line, newLogPattern))
                        {
                            var tmpLog = _logConsumerStatusModel.Cache;
                            if (tmpLog != "")
                            {
                                AddLog(tmpLog);
                            }
                            _logConsumerStatusModel.Cache = line;
                        }
                        else
                        {
                            if(_logConsumerStatusModel.Cache == "")
                            {
                                _logConsumerStatusModel.Cache = line;
                            }
                            else
                            {
                                _logConsumerStatusModel.Cache += "\n" + line;
                            }
                        }
                    }
                    _logConsumerStatusModel.Cache += lines[linesCnt - 1];


                    // // 读取新追加的行
                    // string line = reader.ReadLine();
                    // if (line != null)
                    // {
                    //     if (Regex.IsMatch(line, newLogPattern))
                    //     {
                    //         var tmpLog = _logConsumerStatusModel.Cache;
                    //         AddLog(tmpLog);
                    //         _logConsumerStatusModel.Cache = line;
                    //     }
                    //     else if(line == "")
                    //     {
                    //         
                    //     }
                    //     else 
                    //     {
                    //         if(_logConsumerStatusModel.Cache == "")
                    //         {
                    //             _logConsumerStatusModel.Cache = line;
                    //         }
                    //         else
                    //         {
                    //             _logConsumerStatusModel.Cache += "\n" + line;
                    //         }
                    //     }
                    //     
                    //     _logConsumerStatusModel.Pos = fs.Position;
                    // }
                    // else
                    // {
                    //     if(_logConsumerStatusModel.Cache != "")
                    //     {
                    //         AddLog(_logConsumerStatusModel.Cache);
                    //         _logConsumerStatusModel.Cache = "";
                    //     }
                    //     Thread.Sleep(500);
                    // }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("发生异常: " + ex.Message);
        }
        ParseStatus = "已停止";
        ParseStatusColor = Brushes.PaleVioletRed;
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

public class LogConsumerStatusModel
{
    public long Pos = 0;
    public string Cache = "";
}
