using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

using WSLIPConf.Helpers;

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
            InitializeComponent();
        }

        public App(bool silent) : this()
        {
            SilentMode = silent;
        }
        
    }
}
