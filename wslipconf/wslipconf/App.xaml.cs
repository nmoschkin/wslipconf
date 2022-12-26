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

        public Settings Settings { get; private set; } = new Settings();

        public IPAddress WSLAddress { get; set; } = WSLTool.GetWslIpAddress();

        public IPAddress WSLV6Address { get; set; } = WSLTool.GetWslIpV6Address();

        public bool SilentMode { get; private set; }

        public App() : base()
        {
            var test = $"This is an interpolated string \"with\" more stuff like {DateTime.Now.ToString(@"\abc") + $"{1} singing"}";

            // Just for testing
            //CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            //AppResources.Culture = CultureInfo.CurrentCulture;

            InitializeComponent();
        }

        public App(bool silent) : this()
        {
            SilentMode = silent;
        }
    }
}