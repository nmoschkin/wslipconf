using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;

using WSLIPConf.Helpers;
using WSLIPConf.Models;
using WSLIPConf.ViewModels;

namespace WSLIPConf.Views
{
    /// <summary>
    /// Interaction logic for RuleEdit.xaml
    /// </summary>
    public partial class RuleEdit : Window
    {
        private RuleEditViewModel vm;

        public RuleEdit(WSLMapping rule, Window owner = null)
        {
            InitializeComponent();

            ProtoCombo.ItemsSource = new List<ProxyProtocol>() { ProxyProtocol.Tcp, ProxyProtocol.Udp };
            Owner = owner ?? App.Current.MainWindow;

            DataContext = vm = new RuleEditViewModel(rule);
            vm.AlertClose += Vm_AlertClose;
        }

        private void Vm_AlertClose(object sender, EventArgs e)
        {
            DialogResult = vm.Result;
            Close();
        }

        public static bool? EditRule(WSLMapping rule, bool isnew = false)
        {
            if (rule == null) return false;

            var re = new RuleEdit(rule);
            re.ShowDialog();

            return re.DialogResult;
        }

        public static WSLMapping NewRule(IPAddress defaultAddress = null)
        {
            var nr = new WSLMapping();
            if (defaultAddress != null)
            {
                nr.DestinationAddress = defaultAddress;
            }

            if (EditRule(nr, true) == true)
            {
                return nr;
            }
            else
            {
                return null;
            }
        }
    }
}