using System.Windows.Forms;
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
    private void _undo()
    {
        if (_currentCommandIndex > 0)
        {
            CurrentCommandIndex -= 1;
        }
    }

    [RelayCommand]
    private void _redo()
    {
        if (_currentCommandIndex < _transformCommands.Count - 1)
        {
            CurrentCommandIndex += 1;
        }
    }

    [RelayCommand]
    private void _generageMap()
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
        // todo 生成预览图
        
    }

    public void Reset()
    {
        _transformCommands.Clear();
    }

    [RelayCommand]
    private void Closed()
    {
        GlobalVarsModel.TerrainTransWindowOpened = false;
        Reset();
    }
    
    // ------------------------ rotate -----------------------
    [RelayCommand]
    private void RotateClockwise(int angle)
    {
        var cmd = new RotateTransformCommand(CurrentCommand.DestinationRa3MapFacade, angle);
        applyNewCmd(cmd);
    }
    
    // ------------------------ resize -----------------------
    
    [ObservableProperty] private int _resizeNewWidth = 100;
    
    [ObservableProperty] private int _resizeNewHeight = 100;
    
    [ObservableProperty] private int _resizeNewPositionX = 0;
    
    [ObservableProperty] private int _resizeNewPositionY = 0;
    
    [ObservableProperty] private float _resizeDefaultHeight=200f;
    
    [ObservableProperty] private string _resizeDefaultTexture = "Dirt_Yucatan03";
    
    [RelayCommand]
    private void _resize()
    {
        var cmd = new ResizeTransformCommand(CurrentCommand.DestinationRa3MapFacade, _resizeNewWidth, _resizeNewHeight, _resizeNewPositionX,
            _resizeNewPositionY, _resizeDefaultHeight, _resizeDefaultTexture);
        applyNewCmd(cmd);
    }
    
    // ----------------- symmetry ------------------------
    

    [ObservableProperty] private int _symmetrySelectedDivideType = 0;
    
    
    [ObservableProperty] private int _templateAreaIndex = 1;
    
    [RelayCommand]
    private void _symmetry()
    {
        var cmd = new SymmetryTransformCommand(CurrentCommand.DestinationRa3MapFacade, _symmetrySelectedDivideType + 1, _templateAreaIndex, null);
        applyNewCmd(cmd);
    }
}