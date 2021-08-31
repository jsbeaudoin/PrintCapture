using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XL_ID.Wpf.Converter
{
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media.Imaging;

    public class StringUriToBitmapImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringUri = string.Empty;
            if (value == null || string.IsNullOrEmpty((string)value))
            {
                if (parameter == null)
                {
                    return null;
                }
                else
                {
                    stringUri = (string)parameter;
                }
            }
            else
            {
                stringUri = (string)value;
            }

            var uri = new Uri(stringUri);
            return new BitmapImage(uri);

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
