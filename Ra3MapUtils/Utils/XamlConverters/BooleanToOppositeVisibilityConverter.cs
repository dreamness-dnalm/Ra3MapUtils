using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ra3MapUtils.Utils.XamlConverters;

public class BooleanToOppositeVisibilityConverter : IValueConverter
{
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            if (!boolValue)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
        return Visibility.Visible;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            if (!boolValue)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }
        return Visibility.Visible;
    }
}