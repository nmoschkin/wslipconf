using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

using WSLIPConf.Localization;

namespace WSLIPConf.Converters
{
    internal class ItemEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                if (b)
                {
                    return AppResources.Enable;
                }
                else
                {
                    return AppResources.Disable;
                }
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}