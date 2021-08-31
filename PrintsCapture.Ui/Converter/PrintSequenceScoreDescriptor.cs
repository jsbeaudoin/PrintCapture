namespace PrintsCapture.Ui.Converter
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    using PrintsCapture.Ui.Language;

    public class PrintSequenceScoreDescriptor : IValueConverter
    {
        public string FormatText { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var score = (int)value;
            string descriptor;
            if (score < 10)
            {
                descriptor = Text.SequenceScoreNone;
            }
            else if (score < 30)
            {
                descriptor = Text.SequenceScoreVeryLow;
            }
            else if (score < 50)
            {
                descriptor = Text.SequenceScoreLow;
            }
            else if (score < 70)
            {
                descriptor = Text.SequenceScoreGood;
            }
            else if (score < 100)
            {
                descriptor = Text.SequenceScoreVeryGood;
            }
            else
            {
                descriptor = Text.SequenceScorePerfectMatch;
            }

            return string.IsNullOrEmpty(this.FormatText) ? descriptor : string.Format(this.FormatText, descriptor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
