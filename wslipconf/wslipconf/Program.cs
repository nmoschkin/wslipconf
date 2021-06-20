using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using WSLIPConf.Views;

namespace WSLIPConf
{
    public static class Program
    {
        static App app;

        [STAThread]
        public static void Main(string[] args)
        {
            bool sm = false;

            foreach (var s in args)
            {
                var st = s.ToLower().Trim();

                if (st == "/silent" || st == "/sm")
                {
                    sm = true;
                    break;
                }
            }

            app = new App(sm);
            app.StartupUri = new Uri("Views\\MainWindow.xaml", System.UriKind.Relative);
            app.Run();
        }
    }
}
