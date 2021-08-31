namespace PrintsCapture.Ui.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class PrintQualityDescriptor : IValueConverter
    {
        public string FormatText { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var quality = (int)value;
            var descriptor = string.Empty;
            switch (quality)
            {
                case 1:
                    descriptor = Language.Text.PrintQualityExcellent;
                    break;
                case 2:
                    descriptor = Language.Text.PrintQualityVeryGood;
                    break;
                case 3:
                    descriptor = Language.Text.PrintQualityGood;
                    break;
                case 4:
                    descriptor = Language.Text.PrintQualityBad;
                    break;
                case 5:
                    descriptor = Language.Text.PrintQualityVeryBad;
                    break;
            }

            return string.IsNullOrEmpty(this.FormatText) ? descriptor : string.Format(this.FormatText, descriptor);            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
