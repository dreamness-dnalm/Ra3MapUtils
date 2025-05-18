
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ra3MapUtils.Utils.XamlConverters.MapBorder;

public class IndexToDisplayNameConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var item = value;
        var itemsControl = parameter as ItemsControl;
        if (itemsControl == null || item == null)
        {
            return null;
        }
        
        var index = itemsControl.Items.IndexOf(item);
        
        int colorIndex = index % 8;
        int cycleIndex = index / 8;
        string color = colorIndex switch
        {
            0 => "Orange 橙色",
            1 => "Green 绿色",
            2 => "Blue 蓝色",
            3 => "Cyan 青色",
            4 => "Magenta 品红",
            5 => "Yellow 黄色",
            6 => "Purple 紫色",
            7 => "Pink 粉色",
            _ => "Unknown Color 未知颜色"
        };
        return $"{index + 1} ({cycleIndex}) {color}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}