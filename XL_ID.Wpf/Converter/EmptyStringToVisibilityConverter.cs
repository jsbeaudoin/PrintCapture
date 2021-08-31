namespace XL_ID.Wpf.Converter
{
    using System;
    using System.Globalization;
    using System.Reflection.Emit;
    using System.Windows;
    using System.Windows.Data;

    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = (string)value;

            var isNotEmpty = !string.IsNullOrEmpty(text);

            if (parameter != null)
            {
                var str = parameter.ToString().ToLowerInvariant();
                if (str == "1" || str == "invert")
                {
                    isNotEmpty = ! isNotEmpty;
                }
            }

            return isNotEmpty ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}