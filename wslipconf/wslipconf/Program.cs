using DataTools.MessageBoxEx;

using System;
using System.Linq;
using System.Text;

using WSLIPConf.Helpers;
using WSLIPConf.Localization;

namespace WSLIPConf
{
    public static class Program
    {
        private static App app;
        public static string RequestedDistribution { get; set; }

        [STAThread]
        public static void Main(string[] args)
        {
            bool silentMode = false;
            bool printIP = false;
            string dist = null;

            int i, c = args.Length;

            for (i = 0; i < c; i++)
            {
                var s = args[i];
                var st = s.ToLower().Trim();

                if (st == "/silent" || st == "/sm")
                {
                    silentMode = true;
                    break;
                }
                else if (st == "/printip")
                {
                    printIP = true;
                }
                else if (st == "/dist" || st == "/d")
                {
                    if (i == c - 1)
                    {
                        Console.WriteLine("Must specify a distribution.");
                        Environment.Exit(-1);
                    }
                    dist = args[i + 1];
                }
                else if (st == "/h" || st == "/help")
                {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("");
                    Console.WriteLine("    wslipconf [/silent|/sm] [/printip] [/dist|/d <name>]");
                    Console.WriteLine("");
                    Console.WriteLine("Options:");
                    Console.WriteLine("");
                    Console.WriteLine("    /silent               Start minimized (silent mode)");
                    Console.WriteLine("    /sm");
                    Console.WriteLine("");
                    Console.WriteLine("    /printip              Print IP Address to _wsl.ip.txt and quit.");
                    Console.WriteLine("");
                    Console.WriteLine("    /dist      <name>     Specify default distribution");
                    Console.WriteLine("    /d         <name>");
                    Console.WriteLine("");
                    Environment.Exit(0);
                }
            }

            var wsl = WSLDistribution.RefreshDistributions();

            if (wsl == null)
            {
                Console.WriteLine(AppResources.Failed_Wsl);
                if (!silentMode) MessageBoxEx.Show(AppResources.Error, AppResources.Failed_Wsl, MessageBoxExType.OK, MessageBoxExIcons.Error);

                Environment.Exit(2);
            }

            if (printIP)
            {
                if (dist != null)
                {
                    wsl = WSLDistribution.Distributions.Where(x => x.Name.ToLower() == dist.ToLower())?.FirstOrDefault() ?? wsl;
                }

                System.IO.File.WriteAllText("_wsl.ip.txt", $"{wsl.GetWslIpAddress()}\r\n{wsl.GetWslIpV6Address()}");
                Environment.Exit(0);
            }

            RequestedDistribution = dist;

            app = new App(silentMode);
            app.StartupUri = new Uri("Views\\MainWindow.xaml", System.UriKind.Relative);
            app.Run();
        }
    }
}