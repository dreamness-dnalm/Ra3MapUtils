
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Ra3MapUtils.Utils.XamlConverters.MapBorder;

public class IndexToDisplayColorConverter: IValueConverter
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
        return colorIndex switch
        {
            0 => Brushes.Orange, // "Orange 橙色",
            1 => Brushes.Green, //"Green 绿色",
            2 => Brushes.Blue, //"Blue 蓝色",
            3 => Brushes.Cyan, //"Cyan 青色",
            4 => Brushes.Magenta, //"Magenta 品红",
            5 => Brushes.Yellow, //"Yellow 黄色",
            6 => Brushes.Purple, //"Purple 紫色",
            7 => Brushes.Pink, //"Pink 粉色",
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}