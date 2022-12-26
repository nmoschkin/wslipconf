using DataTools.MessageBoxEx;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;

using WSLIPConf.Helpers;
using WSLIPConf.Localization;
using WSLIPConf.Models;
using WSLIPConf.Views;

namespace WSLIPConf.ViewModels
{
    public class MainWindowViewModel : ObservableBase
    {
        private IPAddress addr;
        private IPAddress addr6;
        private bool runOnStartup;

        private WSLMapping selItem;
        private bool changed;
        private WSLConfig config;

        private List<WSLMapping> selItems = new List<WSLMapping>();

        public ICommand ShowAboutCommand { get; private set; }

        public ICommand AddRuleCommand { get; private set; }

        public ICommand EditRuleCommand { get; private set; }

        public ICommand SaveRulesCommand { get; private set; }

        public ICommand ReloadRulesCommand { get; private set; }

        public ICommand RemoveSelectedRulesCommand { get; private set; }

        public ICommand ClearRulesCommand { get; private set; }

        public ICommand GetRulesCommand { get; private set; }

        public ICommand ApplyRulesCommand { get; private set; }

        public ICommand RefreshIPCommand { get; private set; }

        public ICommand SelectDistroCommand { get; private set; }

        public WSLMapping SelectedItem
        {
            get => selItem;
            set
            {
                SetProperty(ref selItem, value);
            }
        }

        public bool Changed
        {
            get => changed;
            set
            {
                SetProperty(ref changed, value);
            }
        }

        public List<WSLMapping> SelectedItems
        {
            get => selItems;
            set
            {
                SetProperty(ref selItems, value);
            }
        }

        public void SelChange(object sender, SelectionChangedEventArgs e)
        {
            var oldSel = new List<WSLMapping>(selItems);

            if (e.RemovedItems != null && e.RemovedItems.Count > 0)
            {
                foreach (WSLMapping item in e.RemovedItems)
                {
                    oldSel.Remove(item);
                }
            }

            if (e.AddedItems != null && e.AddedItems.Count > 0)
            {
                foreach (WSLMapping item in e.AddedItems)
                {
                    oldSel.Add(item);
                }
            }

            SelectedItems = oldSel;
        }

        public string DistroName => $"({Distro.Name}):";

        public WSLDistribution Distro
        {
            get => App.Current.SessionDefault;
            set
            {
                if (App.Current.SessionDefault != value)
                {
                    App.Current.SessionDefault = value;
                    RefreshIP();
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(DistroName));
                }
            }
        }

        public MainWindowViewModel()
        {
            config = WSLConfig.Load();
            runOnStartup = TaskTool.GetIsEnabled();
            addr = App.Current.WSLAddress;
            addr6 = App.Current.WSLV6Address;

            Distro = WSLDistribution.RefreshDistributions();
            GetRulesCommand = new SimpleCommand((o) =>
            {
                var items = PortProxyTool.GetPortProxies();

                foreach (var i in items)
                {
                    bool b = false;

                    foreach (var curm in config.Mappings)
                    {
                        if (curm.Equals(i))
                        {
                            b = true;
                            break;
                        }
                    }

                    if (!b)
                    {
                        Config.Mappings.Add(i);
                        Changed = true;
                    }
                }
            });

            AddRuleCommand = new SimpleCommand((o) =>
            {
                var rule = RuleEdit.NewRule(addr);

                if (rule != null)
                {
                    Config.Mappings.Add(rule);
                    Changed = true;
                }
            });

            EditRuleCommand = new SimpleCommand((o) =>
            {
                var rule = SelectedItem;
                if (RuleEdit.EditRule(rule) == true)
                {
                    Changed = true;
                }
            });

            ClearRulesCommand = new SimpleCommand((o) =>
            {
                var res = MessageBoxEx.Show(AppResources.ConfirmClearRules, AppResources.ClearRules, MessageBoxExType.YesNo, MessageBoxExIcons.Question);

                if (res == MessageBoxExResult.Yes)
                {
                    config.Mappings.Clear();
                    Changed = true;
                }
            });

            SaveRulesCommand = new SimpleCommand((o) =>
            {
                config.SaveToDisk();
                Changed = false;
            });

            ReloadRulesCommand = new SimpleCommand((o) =>
            {
                var res = MessageBoxEx.Show(AppResources.ConfirmReloadRules, AppResources.Reload, MessageBoxExType.YesNo, MessageBoxExIcons.Question);
                if (res == MessageBoxExResult.Yes)
                {
                    config.ReadFromDisk();
                    Changed = false;
                }
            });

            RemoveSelectedRulesCommand = new SimpleCommand((o) =>
            {
                var res = MessageBoxEx.Show(AppResources.ConfirmRemoveRules, AppResources.DeleteRule, MessageBoxExType.YesNo, MessageBoxExIcons.Question);
                if (res == MessageBoxExResult.Yes)
                {
                    var l = new List<WSLMapping>(SelectedItems);

                    foreach (var item in l)
                    {
                        config.Mappings.Remove(item);
                    }

                    Changed = true;
                }
            });

            ApplyRulesCommand = new SimpleCommand((o) =>
            {
                RefreshIP();
                var b = PortProxyTool.SetPortProxies(config.Mappings);

                if (o is string s && s == "True")
                {
                    if (b)
                    {
                        MessageBoxEx.Show(AppResources.RulesApplied, AppResources.ApplyRules, MessageBoxExType.OK, MessageBoxExIcons.Information);
                    }
                    else
                    {
                        MessageBoxEx.Show(AppResources.ErrorApplying, AppResources.ApplyRules, MessageBoxExType.OK, MessageBoxExIcons.Error);
                    }
                }
            });

            RefreshIPCommand = new SimpleCommand((o) =>
            {
                RefreshIP();
            });

            ShowAboutCommand = new SimpleCommand((o) =>
            {
                var aboutBox = new About();
                aboutBox.ShowDialog();
            });

            SelectDistroCommand = new SimpleCommand((o) =>
            {
                Distro = Views.Distro.PickDistribution();
                if (Distro?.IsDefault ?? true)
                {
                    config.PreferredDistribution = null;
                }
                else
                {
                    config.PreferredDistribution = Distro?.Name;
                }

                config.SaveToDisk();

                foreach (var item in config.Mappings)
                {
                    if (item.UseDefaultDistro)
                    {
                        item.TriggerUpdate();
                    }
                }
            });
        }

        public WSLConfig Config
        {
            get => config;
            set
            {
                SetProperty(ref config, value);
            }
        }

        public bool RunOnStartup
        {
            get => runOnStartup;
            set
            {
                if (SetProperty(ref runOnStartup, value))
                {
                    if (value == true)
                    {
                        TaskTool.EnableOnStartup();
                    }
                    else
                    {
                        TaskTool.DisableOnStartup();
                    }

                    if (TaskTool.GetIsEnabled() != value)
                    {
                        runOnStartup = !value;
                        OnPropertyChanged();

                        return;
                    }
                }
            }
        }

        public IPAddress WSLAddress
        {
            get => addr;
            set
            {
                SetProperty(ref addr, value);
            }
        }

        public bool HasV6Address => addr6 != null && addr6.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;

        public IPAddress WSLV6Address
        {
            get => addr6;
            set
            {
                if (SetProperty(ref addr6, value))
                {
                    OnPropertyChanged(nameof(HasV6Address));
                }
            }
        }

        public void RefreshIP()
        {
            Distro.Refresh();

            WSLAddress = App.Current.WSLAddress = Distro.GetWslIpAddress();
            WSLV6Address = App.Current.WSLV6Address = Distro.GetWslIpV6Address();

            foreach (var item in config.Mappings)
            {
                item.OnPropertyChanged(nameof(WSLMapping.DestinationAddress));
            }
        }
    }
}