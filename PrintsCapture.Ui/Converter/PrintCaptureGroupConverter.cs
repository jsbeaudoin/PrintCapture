namespace PrintsCapture.Ui.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    using PrintsCapture.Prints.Enum;

    public class PrintCaptureGroupConverter : IValueConverter
    {
       
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var paramString = (PrintCaptureGroup)parameter;

            var valueToCheck = (PrintCaptureGroup)parameter;
            
            //Enum.TryParse(paramString, out valueToCheck);
            
            var refValue = (PrintCaptureGroup)value;

            return refValue == valueToCheck;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
