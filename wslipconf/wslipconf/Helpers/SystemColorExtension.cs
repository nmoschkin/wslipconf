using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Media;


namespace WSLIPConf.Extensions
{
    [ContentProperty("AccentColor")]
    public class SystemColorExtension : MarkupExtension
    {


        public SystemColorExtension()
        {

        }

        public bool UseTextColor { get; set; } = false;

        public Color TextColor
        {
            get
            {
                var c = AccentColor;

                int l = c.R + c.B + c.G;
                l /= 3;

                if (l < 127)
                {
                    return Colors.White;
                }
                else
                {
                    return Colors.Black;
                }

            }
        }

        public Color AccentColor
        {
            get
            {

                var (r, g, b, a) = GetAccentColor();

                // color.A, color.R, color.G, color.B are the color channels.
                return new Color()
                {
                    A = a,
                    R = r,
                    G = g,
                    B = b
                };
            }
            set
            {

            }
        }

        private static (Byte r, Byte g, Byte b, Byte a) GetAccentColor()
        {
            const String DWM_KEY = @"Software\Microsoft\Windows\DWM";
            using (RegistryKey dwmKey = Registry.CurrentUser.OpenSubKey(DWM_KEY, RegistryKeyPermissionCheck.ReadSubTree))
            {
                const String KEY_EX_MSG = "The \"HKCU\\" + DWM_KEY + "\" registry key does not exist.";
                if (dwmKey is null) throw new InvalidOperationException(KEY_EX_MSG);

                Object accentColorObj = dwmKey.GetValue("AccentColor");
                if (accentColorObj is int accentColorDword)
                {
                    return ParseDWordColor(accentColorDword);
                }
                else
                {
                    const String VALUE_EX_MSG = "The \"HKCU\\" + DWM_KEY + "\\AccentColor\" registry key value could not be parsed as an ABGR color.";
                    throw new InvalidOperationException(VALUE_EX_MSG);
                }
            }

        }

        private static (Byte r, Byte g, Byte b, Byte a) ParseDWordColor(int color)
        {
            byte a = (byte)((color >> 24) & 0xFF),
                b = (byte)((color >> 16) & 0xFF),
                g = (byte)((color >> 8) & 0xFF),
                r = (byte)((color >> 0) & 0xFF);

            return (r, g, b, a);
        }



        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return UseTextColor ? TextColor : AccentColor;
        }

    }

}
