using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using WSLIPConf.Localization;

namespace WSLIPConf.Converters
{
    public class AutoDestConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length >= 1 && values[0] is IPAddress addr)
            {
                if (values.Length >= 2 && values[1] is bool b)
                {
                    if (b)
                    {
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
