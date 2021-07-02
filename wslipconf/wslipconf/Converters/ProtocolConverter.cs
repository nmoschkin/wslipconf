using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using WSLIPConf.Helpers;

namespace WSLIPConf.Converters
{
    public class ProtocolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProxyProtocol pp)
            {
                return pp.ToString();
            }
            else
            {
                return value?.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                if (s.ToLower() == "tcp") return ProxyProtocol.Tcp;
                else if (s.ToLower() == "udp") return ProxyProtocol.Udp;
                else return ProxyProtocol.Tcp;
            }
            else
            {
                return ProxyProtocol.Tcp;
            }
        }
    }
}
