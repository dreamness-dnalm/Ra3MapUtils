using System.IO;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dreamness.Ra3.Map.Facade.Core;
using Dreamness.Ra3.Map.Facade.Util;
using Dreamness.RA3.Map.Transform.Ra3MapTransform.Commands;
using Ra3MapUtils.Models;
using SharedFunctionLib.Utils;

namespace Ra3MapUtils.ViewModels;

public partial class TerrainTransWindowViewModel: ObservableObject
{
    [ObservableProperty] private string _mapName = "";
    
    [ObservableProperty] private string _selectedMapName = "";
    
    [ObservableProperty] private string _windowTitle = "";
    
    private List<BaseTransformCommand> _transformCommands = new List<BaseTransformCommand>();
    
    private int _currentCommandIndex = -1;
    
    private BaseTransformCommand CurrentCommand => _currentCommandIndex == -1 ? null : _transformCommands[_currentCommandIndex];
    
    [ObservableProperty] private ImageSource _mapPreviewImage = new BitmapImage();

    private int CurrentCommandIndex
    {
        get => _currentCommandIndex;
        set
        {
            if (value < -1 || value >= _transformCommands.Count)
            {
                throw new IndexOutOfRangeException();
            }

            if (CurrentCommandIndex != value)
            {
                _currentCommandIndex = value;
            }
            
        }
    }
    
    partial void OnMapNameChanged(string mapName)
    {
        WindowTitle = $"地形变换工具 - {mapName}";
        Reset();
        var ra3Map = Ra3MapFacade.Open(Ra3PathUtil.RA3MapFolder, mapName);

        var cmd = new InitTransformCommand(ra3Map);
        applyNewCmd(cmd);
    }

    [RelayCommand]
    private void Undo()
    {
        if (_currentCommandIndex > 0)
        {
            CurrentCommandIndex -= 1;
        }
    }

    [RelayCommand]
    private void Redo()
    {
        if (_currentCommandIndex < _transformCommands.Count - 1)
        {
            CurrentCommandIndex += 1;
        }
    }

    [RelayCommand]
    private void GenerateMap()
    {
        var cmd = _transformCommands[_currentCommandIndex];
        var ra3Map = cmd.DestinationRa3MapFacade;
        
        var inputDialog = new Ookii.Dialogs.WinForms.InputDialog();
        inputDialog.MainInstruction = "请输入新地图名";
        inputDialog.Content = "请输入新地图名";
        inputDialog.WindowTitle = "另存为";
        inputDialog.Input = "trans_" + _mapName + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        
        if (inputDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        {
            return;
        }
        if (inputDialog.Input.Trim() == "")
        {
            MessageBox.Show("地图名不能为空");
            return;
        }
        
        ra3Map.SaveAs(Ra3PathUtil.RA3MapFolder, inputDialog.Input.Trim());
    }

    private void applyNewCmd(BaseTransformCommand cmd)
    {
        for (int i = CurrentCommandIndex + 1; i < _transformCommands.Count; i++)
        {
            _transformCommands.RemoveAt(i);
        }
        _transformCommands.Add(cmd);
        cmd.Transform();
        CurrentCommandIndex += 1;

        var imageBytes = CurrentCommand.DestinationRa3MapFacade.GetPreviewImage();
        
        var img = new BitmapImage();
        using (var ms = new MemoryStream(imageBytes))
        {
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = ms;
            img.EndInit();
            img.Freeze();
        }
        
        MapPreviewImage = img;
        
        UpdateInfo();
    }

    public void Reset()
    {
        _transformCommands.Clear();
        _currentCommandIndex = -1;
        MapPreviewImage = new BitmapImage();
    }

    [RelayCommand]
    private void Closed()
    {
        GlobalVarsModel.TerrainTransWindowOpened = false;
        Reset();
    }
    
    // ------------------------ info ----------------------
    [ObservableProperty] private string _infoSize = "";

    private void UpdateInfo()
    {
        var ra3map = CurrentCommand.DestinationRa3MapFacade;
        InfoSize = $"{ra3map.MapWidth} x {ra3map.MapHeight}";
    }
    
    
    
    // ------------------------ rotate -----------------------
    [RelayCommand]
    private void RotateClockwise(string angle)
    {
        var cmd = new RotateTransformCommand(CurrentCommand.DestinationRa3MapFacade, Convert.ToInt32(angle));
        applyNewCmd(cmd);
    }
    
    // ------------------------ resize -----------------------
    
    [ObservableProperty] private int _resizeNewWidth = 100;
    
    [ObservableProperty] private int _resizeNewHeight = 100;
    
    [ObservableProperty] private int _resizeNewPositionX = 0;
    
    [ObservableProperty] private int _resizeNewPositionY = 0;
    
    [ObservableProperty] private int _resizeDefaultHeight=200;
    
    [ObservableProperty] private string _resizeDefaultTexture = "Dirt_Yucatan03";
    
    [RelayCommand]
    private void Resize()
    {
        var cmd = new ResizeTransformCommand(CurrentCommand.DestinationRa3MapFacade, _resizeNewWidth, _resizeNewHeight, _resizeNewPositionX,
            _resizeNewPositionY, _resizeDefaultHeight, _resizeDefaultTexture);
        applyNewCmd(cmd);
    }
    
    // ----------------- symmetry ------------------------
    
    public class SymmetryDivideType
    {
        public int Id { get; private set; }
        public ImageSource Source { get; private set; }
        public int AreaCnt { get; private set; }
        public bool Enabled { get; private set; }
        
        public SymmetryDivideType(int id, int areaCnt, bool enabled)
        {
            Id = id;
            AreaCnt = areaCnt;
            Source = new BitmapImage(new Uri($"pack://application:,,,/data/imgs/SymmetryTransform_{id}.png"));
            Enabled = enabled;
        }
    }
    
    
    [ObservableProperty] private List<SymmetryDivideType> _symmetryDivideTypes = new List<SymmetryDivideType>()
    {
        new SymmetryDivideType(1, 2, true),
        // new SymmetryDivideType(2, 2, false),
        new SymmetryDivideType(3, 2, true),
        // new SymmetryDivideType(4, 2, false),
        // new SymmetryDivideType(5, 2, false),
        // new SymmetryDivideType(6, 2, false),
        // new SymmetryDivideType(7, 2, false),
        // new SymmetryDivideType(8, 2, false),
        // new SymmetryDivideType(9, 4, false),
        // new SymmetryDivideType(10, 4, false),
        // new SymmetryDivideType(11, 8, false)
    };


    

    // [ObservableProperty] private int _symmetrySelectedDivideType = 0;
    
    
    [ObservableProperty] private int _templateAreaId = 1;
    
    [ObservableProperty] private SymmetryDivideType _selectedSymmetryDivideType; 
    
    partial void OnSelectedSymmetryDivideTypeChanged(SymmetryDivideType value)
    {
        if (value != null && _templateAreaId > value.AreaCnt)
        {
            TemplateAreaId = 1;
        }
    }
    
    [RelayCommand]
    private void Symmetry()
    {
        if (SelectedSymmetryDivideType == null)
        {
            MessageBox.Show("请先选择对称类型");
            return;
        }
        var cmd = new SymmetryTransformCommand(CurrentCommand.DestinationRa3MapFacade, SelectedSymmetryDivideType.Id, _templateAreaId);
        applyNewCmd(cmd);
    }
}