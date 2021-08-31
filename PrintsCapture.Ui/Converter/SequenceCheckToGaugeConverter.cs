namespace PrintsCapture.Ui.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class SequenceCheckToGaugeConverter : IValueConverter
    {
        public static int SequenceThreshold { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (SequenceThreshold == 0)
            {
                SequenceThreshold = 50;
            }
            
            var threshold = SequenceThreshold; // (int)parameter;

            var intValue = (int)value;
            int zone = 5;

            if (intValue > threshold)
            {
                if (intValue > threshold + 50)
                {
                    zone = 1;
                }
                else if (intValue > threshold + 25)
                {
                    zone = 2;
                }
                else
                {
                    zone = 3;
                }
            }
            else
            {
                if (intValue == 0 || intValue < threshold * 0.5)
                {
                    zone = 5;
                }
                else
                {
                    zone = 4;
                }
            }
            
            return string.Format(
                "pack://application:,,,/PrintsCapture.Ui;component/Images/Gauge{0}On5_trans.png",zone);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
