using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Data;

using WSLIPConf.Helpers;
using WSLIPConf.Localization;

namespace WSLIPConf.Converters
{
    public class AutoDestConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length >= 1 && values[0] is IPAddress addr)
            {
                // Auto-Destination
                if (values.Length >= 2 && values[1] is bool b)
                {
                    if (b)
                    {
                        if (values.Length >= 3 && values[2] is ProxyType pt)
                        {
                            if ((pt & ProxyType.DestV6) == ProxyType.DestV6)
                            {
                                return string.Format(AppResources.AutoIPX, App.Current.WSLV6Address);
                            }
                        }

                        return string.Format(AppResources.AutoIPX, App.Current.WSLAddress);
                    }
                    else
                    {
                        return addr?.ToString();
                    }
                }
                else
                {
                    return addr?.ToString();
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}