namespace XL_ID.Wpf.Converter
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Convert from bool to a row height. When value is false, height is 0. Else, the converter parameter is parsed.
    /// Values can include pixels, stars or Auto. sample values : 45 * 22* auto
    /// </summary>
    [ValueConversion(typeof(bool), typeof(GridLength))]
    public class BoolToGridRowHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int unitCount = 1;
            var unitType = GridUnitType.Pixel;

            var stringVal = parameter == null ? string.Empty : parameter.ToString();
            if (stringVal.EndsWith("*") || string.IsNullOrEmpty(stringVal))
            {
                unitType = GridUnitType.Star;
                stringVal = stringVal.TrimEnd('*');
            } 
            else if (stringVal.ToLower() == "auto")
            {
                unitType = GridUnitType.Auto;
            }

            if (unitType != GridUnitType.Auto)
            {
                if (!int.TryParse(stringVal, out unitCount))
                {
                    unitCount = 1;
                }
            }

            return (bool)value ? new GridLength(unitCount, unitType) : new GridLength(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {    // Don't need any convert back
            return null;
        }
    }
}
