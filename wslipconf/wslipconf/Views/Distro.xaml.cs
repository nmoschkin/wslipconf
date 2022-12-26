using System;
using System.Linq;
using System.Text;
using System.Windows;

using WSLIPConf.Helpers;
using WSLIPConf.ViewModels;

namespace WSLIPConf.Views
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class Distro : Window
    {
        private DistroPickerViewModel vm;

        public Distro()
        {
            InitializeComponent();
            vm = new DistroPickerViewModel();
            DataContext = vm;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static WSLDistribution PickDistribution()
        {
            var wind = new Distro();

            wind.ShowDialog();
            return wind.vm.SelectedDistribution ?? App.Current.SessionDefault;
        }
    }
}