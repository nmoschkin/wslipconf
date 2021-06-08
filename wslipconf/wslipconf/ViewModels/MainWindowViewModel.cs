﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

using WSLIPConf.Helpers;
using WSLIPConf.Models;

namespace WSLIPConf.ViewModels
{
    public class MainWindowViewModel : ObservableBase
    {

        private IPAddress addr;
        private bool runOnStartup;

        private WSLMapping selItem;
        private WSLConfig config;

        private List<WSLMapping> selItems = new List<WSLMapping>();


        public ICommand AddRuleCommand { get; private set; }

        public ICommand EditRuleCommand { get; private set; }

        public ICommand RemoveSelectedRulesCommand { get; private set; }

        public ICommand GetRulesCommand { get; private set; }


        public WSLMapping SelectedItem
        {
            get => selItem;
            set
            {
                SetProperty(ref selItem, value);
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

        public MainWindowViewModel()
        {
            config = WSLConfig.Load();
            runOnStartup = TaskTool.GetIsEnabled();
            addr = WSLTool.GetWslIpAddress();

            GetRulesCommand = new SimpleCommand((o) =>
            {
                var items = NetshTool.GetPortProxies();

                foreach (var i in items)
                {
                    if (!Config.Mappings.Contains(i)) Config.Mappings.Add(i);
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

    }
}
