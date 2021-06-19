using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

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

        public IPAddress WSLAddress
        {
            get;
            set;
        }


        public App() : base()
        {
            WSLAddress = Helpers.WSLTool.GetWslIpAddress();
        }
    }
}
