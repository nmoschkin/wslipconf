using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

using WSLIPConf.Models;

namespace WSLIPConf.Converters
{
    public class V6ColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color result = Colors.Black;

            if (value is WSLMapping map)
            {
                if (map.AutoDestination)
                {
                    if ((map.ProxyType & Helpers.ProxyType.DestV6) == Helpers.ProxyType.DestV6)
                    {
                        result = Colors.Blue;
                    }
                    else
                    {
                        result = Colors.ForestGreen;
                    }
                }
                else
                {
                    result = Colors.Black;
                }
            }

            return new SolidColorBrush(result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}