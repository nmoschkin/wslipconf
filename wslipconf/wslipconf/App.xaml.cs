using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

using WSLIPConf.Helpers;
using WSLIPConf.Localization;

namespace WSLIPConf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static public new App Current
        {
            get => (App)Application.Current;

        }

        public Settings Settings { get; private set; } = new Settings();

        public IPAddress WSLAddress { get; set; } = WSLTool.GetWslIpAddress();

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
