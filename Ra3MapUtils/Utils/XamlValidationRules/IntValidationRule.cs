using System.Globalization;
using System.Windows.Controls;

namespace Ra3MapUtils.Utils.XamlValidationRules;

public class IntValidationRule: ValidationRule
{
    public int MinValue { get; set; } = int.MinValue;
    public int MaxValue { get; set; } = int.MaxValue;
    public string ErrorMsg { get; set; } = "非法输入!";
    
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (value is string inputStr)
        {
            int v;
            if (int.TryParse(inputStr, out v))
            {
                if (v >= MinValue && v <= MaxValue)
                {
                    return ValidationResult.ValidResult;
                }
                else
                {
                    return new ValidationResult(false, ErrorMsg);
                }
            }
            else
            {
                return new ValidationResult(false, ErrorMsg);
            }
        }else if (value is int inputInt)
        {
            if (inputInt >= MinValue && inputInt <= MaxValue)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, ErrorMsg);
            }
        }
        else
        {
            return new ValidationResult(false, ErrorMsg);
        }
    }
}