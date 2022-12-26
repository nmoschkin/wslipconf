using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

using WSLIPConf.Helpers;
using WSLIPConf.ViewModels;

namespace WSLIPConf.Converters
{
    public class ProxyTypeFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProxyType pt)
            {
                return RuleEditViewModel.FormatProxyType(pt);
            }

            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}