using System;
using System.IO;
using System.Linq;
using System.Text;

using WSLIPConf.Helpers;

namespace WSLIPConf
{
    public static class Program
    {
        private static App app;

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
                else if (st == "/printip")
                {
                    File.WriteAllText("_wsl.ip.txt", $"{WSLTool.GetWslIpAddress()}\r\n{WSLTool.GetWslIpV6Address()}");
                    Environment.Exit(0);
                }
            }

            app = new App(sm);
            app.StartupUri = new Uri("Views\\MainWindow.xaml", System.UriKind.Relative);
            app.Run();
        }
    }
}