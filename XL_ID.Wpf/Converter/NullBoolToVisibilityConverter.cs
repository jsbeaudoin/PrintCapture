namespace XL_ID.Wpf.Converter
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    public class NullBoolToVisibilityConverter : IValueConverter
    {
        public Visibility TrueVisibility { get; set; }

        public Visibility FalseVisibility { get; set; }

        public Visibility NullVisibility { get; set; }

        public NullBoolToVisibilityConverter()
        {
            this.TrueVisibility = Visibility.Visible;
            this.FalseVisibility = Visibility.Collapsed;
            this.NullVisibility = Visibility.Hidden;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool invertValue = false;
            if (parameter != null)
            {
                var str = parameter.ToString();
                if (str == "invert" || str == "!" || str == "1" || str == "not")
                {
                    invertValue = true;
                }                
            }
                        
            // 1st --> Process non nullable bool
            if (value != null && Nullable.GetUnderlyingType(value.GetType()) == null)
            {
                var simpleBool = (bool)value;
                if (invertValue)
                {
                    simpleBool = !simpleBool;
                }
                return simpleBool ? this.TrueVisibility : this.FalseVisibility;
            }

            // Else, process nullable bool
            var boolValue = (bool?)value;

            if (invertValue)
            {
                boolValue = !(boolValue.HasValue && boolValue.Value);
            }

            if (boolValue == null)
            {
                return this.NullVisibility;
            }

            return boolValue.Value ? this.TrueVisibility : this.FalseVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}