using System;
using System.Linq;
using System.Net;
using System.Windows;

using WSLIPConf.Helpers;

namespace WSLIPConf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public new static App Current
        {
            get => (App)Application.Current;
        }

        public WSLDistribution SessionDefault
        {
            get => WSLDistribution.SessionDefault;
            set => WSLDistribution.SessionDefault = value;
        }

        public Settings Settings { get; private set; } = new Settings();

        public IPAddress WSLAddress { get; set; }

        public IPAddress WSLV6Address { get; set; }

        public bool SilentMode { get; private set; }

        public App() : base()
        {
            InitializeComponent();

            WSLDistribution.RefreshDistributions();

            if (Program.RequestedDistribution != null)
            {
                SessionDefault = WSLDistribution.Distributions.Where(x => x.Name.ToLower() == Program.RequestedDistribution)?.FirstOrDefault() ?? SessionDefault;
            }

            WSLAddress = SessionDefault.GetWslIpAddress();
            WSLV6Address = SessionDefault.GetWslIpV6Address();
        }

        public App(bool silent) : this()
        {
            SilentMode = silent;
        }
    }
}