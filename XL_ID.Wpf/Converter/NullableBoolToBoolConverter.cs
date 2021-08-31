// -----------------------------------------------------------------------
// <copyright file="NullableBoolToBoolConverter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace XL_ID.Wpf.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class NullableBoolToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            
            var nullableValue = (bool?)value;
            return nullableValue.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public bool ToBool(bool? value)
        {
            return (bool)this.Convert(value, null, null, null);
        }
    }
}
