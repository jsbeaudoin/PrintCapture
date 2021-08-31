namespace PrintsCapture.Livescan.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class QualityToGaugeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int gaugeValue = (int)value;
            if (gaugeValue < 1 || gaugeValue > 5)
            {
                gaugeValue = 5;
            }
            return string.Format(
                @"pack://application:,,,/PrintsCapture.Livescan;component/Images/Gauge{0}On5_trans.png",
                gaugeValue);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
