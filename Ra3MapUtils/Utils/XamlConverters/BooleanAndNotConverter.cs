using System;
using System.Globalization;
using System.Windows.Data;

namespace Ra3MapUtils.Utils.XamlConverters;

/// <summary>
/// Returns firstBool && !secondBool for two boolean inputs.
/// </summary>
public class BooleanAndNotConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is { Length: >= 2 } &&
            values[0] is bool first &&
            values[1] is bool second)
        {
            return first && !second;
        }

        return false;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

