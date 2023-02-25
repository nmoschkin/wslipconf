using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

using WSLIPConf.Helpers;
using WSLIPConf.Localization;
using WSLIPConf.Models;
using WSLIPConf.ViewModels;

namespace WSLIPConf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm;
        private bool init = false;

        private double x, y;

        public NotifyIcon IconArea { get; private set; }

        private ToolStripMenuItem appMenu;
        private ToolStripMenuItem suspendMenu;
        private ToolStripMenuItem proxiesMenu;

        private WindowInteropHelper wh;

        public MainWindow()
        {
            InitializeComponent();

            PortProxyTool.SystemMappingChanged += PortProxyTool_SystemMappingChanged;

            DataContext = vm = new MainWindowViewModel();
            vm.PropertyChanged += Vm_PropertyChanged;

            wh = new WindowInteropHelper(this);
            wh.EnsureHandle();

            x = Width;
            y = Height;

            IconArea = new NotifyIcon
            {
                Icon = AppResources.wslip,
                Visible = true,
                BalloonTipText = string.Format(AppResources.IconTip, AppResources.MainTitle, App.Current.WSLAddress),
                BalloonTipTitle = AppResources.MainTitle,
                BalloonTipIcon = ToolTipIcon.Info
            };

            IconArea.MouseClick += IconArea_MouseClick;
            IconArea.BalloonTipClicked += IconArea_BalloonTipClicked;

            BuildIconMenu();
            BuildProxiesMenu(vm.Config.Mappings);

            this.StateChanged += MainWindow_StateChanged;
            this.Closing += MainWindow_Closing;
            this.SizeChanged += MainWindow_SizeChanged;
            this.LocationChanged += MainWindow_LocationChanged;

            BindList.SelectionChanged += vm.SelChange;

            if (!App.Current.SilentMode)
            {
                App.Current.Settings.ApplyWindowSettings(this);
            }

            if (App.Current.SilentMode)
            {
                this.WindowState = WindowState.Minimized;
                vm.ApplyRulesCommand.Execute(null);
            }

            init = !App.Current.SilentMode;
        }

        private void PortProxyTool_SystemMappingChanged(object sender, MappingChangedEventArgs e)
        {
            BuildProxiesMenu(vm.Config.Mappings);
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.AllRulesSuspended) && suspendMenu != null)
            {
                suspendMenu.CheckState = vm.AllRulesSuspended ? CheckState.Checked : CheckState.Unchecked;
            }
        }

        private void BuildProxiesMenu(IEnumerable<WSLMapping> newmappings)
        {
            if (proxiesMenu == null) { return; }

            proxiesMenu.DropDownItems.Clear();

            var items = new List<WSLMapping>(newmappings);
            items.Sort((a, b) => string.Compare(a.Name, b.Name));

            foreach (var mapping in items)
            {
                var proxyMenu = new ToolStripMenuItem()
                {
                    Text = mapping.Name,
                    CheckOnClick = false,
                    Checked = mapping.IsOnSystem,
                    Tag = mapping
                };

                proxyMenu.Click += ProxyMenu_Click;
                proxiesMenu.DropDownItems.Add(proxyMenu);
            }

            proxiesMenu.Enabled = proxiesMenu.HasDropDownItems;
        }

        private void ProxyMenu_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is WSLMapping mapping)
            {
                mapping.IsSuspended = item.Checked;
                PortProxyTool.SetPortProxy(mapping);

                item.Checked = mapping.IsOnSystem;
                vm.OnPropertyChanged(nameof(MainWindowViewModel.AllRulesSuspended));
            }
        }

        private void BuildIconMenu()
        {
            var cm = new ContextMenuStrip();

            var mnu = appMenu = new ToolStripMenuItem()
            {
                Text = AppResources.ShowWindow,
                Checked = !App.Current.SilentMode
            };

            mnu.Click += Mnu_Click;

            cm.Items.Add(mnu);
            cm.Items.Add(new ToolStripSeparator());

            mnu = new ToolStripMenuItem()
            {
                Text = AppResources.CopyIPAddress
            };

            mnu.Click += Mnu_Click;

            cm.Items.Add(mnu);

            mnu = new ToolStripMenuItem()
            {
                Text = AppResources.CopyIPV6Address
            };

            mnu.Enabled = App.Current.WSLV6Address != null;
            mnu.Click += Mnu_Click;

            cm.Items.Add(mnu);

            cm.Items.Add(new ToolStripSeparator());

            mnu = new ToolStripMenuItem()
            {
                Text = AppResources.RefreshWSLIP
            };

            mnu.Click += Mnu_Click;

            cm.Items.Add(mnu);

            mnu = new ToolStripMenuItem()
            {
                Text = AppResources.RefreshAndApply
            };

            mnu.Click += Mnu_Click;

            cm.Items.Add(mnu);
            cm.Items.Add(new ToolStripSeparator());

            mnu = new ToolStripMenuItem()
            {
                Text = AppResources.AutoApply,
                CheckOnClick = true,
                Checked = App.Current.Settings.AutoApply
            };

            mnu.Click += Mnu_Click;

            cm.Items.Add(mnu);

            cm.Items.Add(new ToolStripSeparator());

            mnu = new ToolStripMenuItem()
            {
                Text = AppResources.SuspendAllRules,
                CheckOnClick = false,
                Checked = vm.AllRulesSuspended
            };

            mnu.Click += Mnu_Click;

            suspendMenu = mnu;

            cm.Items.Add(mnu);

            mnu = new ToolStripMenuItem()
            {
                Text = AppResources.SuspendEnableRule,
                CheckOnClick = false,
                Enabled = false
            };

            proxiesMenu = mnu;

            cm.Items.Add(mnu);

            cm.Items.Add(new ToolStripSeparator());

            mnu = new ToolStripMenuItem()
            {
                Text = AppResources.Quit
            };

            mnu.Click += Mnu_Click;

            cm.Items.Add(mnu);

            IconArea.ContextMenuStrip = cm;
        }

        private void Mnu_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item)
            {
                if (item.Text == AppResources.ShowWindow)
                {
                    RestoreWindow();
                }
                else if (item.Text == AppResources.SuspendAllRules)
                {
                    vm.AllRulesSuspended = !vm.AllRulesSuspended;
                    item.Checked = vm.AllRulesSuspended;
                }
                else if (item.Text == AppResources.Quit)
                {
                    QuitBtn_Click(sender, null);
                }
                else if (item.Text == AppResources.CopyIPAddress)
                {
                    CopyIP();
                }
                else if (item.Text == AppResources.CopyIPV6Address)
                {
                    CopyIP(v6: true);
                }
                else if (item.Text == AppResources.AutoApply)
                {
                    App.Current.Settings.AutoApply = item.Checked;
                }
                else if (item.Text == AppResources.RefreshIP)
                {
                    vm.RefreshIP();
                    IconArea.BalloonTipText = string.Format(AppResources.IconTip, AppResources.MainTitle, App.Current.WSLAddress);

                    if (App.Current.Settings.AutoApply)
                    {
                        vm.ApplyRulesCommand.Execute(null);
                    }
                }
                else if (item.Text == AppResources.RefreshAndApply)
                {
                    vm.RefreshIP();
                    IconArea.BalloonTipText = string.Format(AppResources.IconTip, AppResources.MainTitle, App.Current.WSLAddress);

                    vm.ApplyRulesCommand.Execute(null);
                }
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }

            if (ActualWidth == 0) Width = x;
            if (ActualHeight == 0) Height = y;

            appMenu.Checked = ShowInTaskbar;

            vm.RefreshIP();
            IconArea.BalloonTipText = string.Format(AppResources.IconTip, AppResources.MainTitle, App.Current.WSLAddress);

            if (App.Current.Settings.AutoApply)
            {
                vm.ApplyRulesCommand.Execute(null);
            }
        }

        private void IconArea_BalloonTipClicked(object sender, EventArgs e)
        {
            RestoreWindow();
        }

        private void IconArea_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IconArea.ShowBalloonTip(0);
            }
        }

        public void RestoreWindow()
        {
            this.WindowState = WindowState.Normal;

            this.Show();
            this.Focus();
            this.Activate();
            this.BringIntoView();

            if (App.Current.SilentMode && !init)
            {
                init = true;
                App.Current.Settings.ApplyWindowSettings(this);
            }
        }

        private void CopyIP(bool suppressAlert = false, bool v6 = false)
        {
            if (v6)
            {
                System.Windows.Forms.Clipboard.SetText(App.Current.WSLV6Address.ToString());
                if (!suppressAlert) IconArea.ShowBalloonTip(0, AppResources.MainTitle, AppResources.IPV6AddressCopied, ToolTipIcon.Info);
            }
            else
            {
                System.Windows.Forms.Clipboard.SetText(App.Current.WSLAddress.ToString());
                if (!suppressAlert) IconArea.ShowBalloonTip(0, AppResources.MainTitle, AppResources.IPAddressCopied, ToolTipIcon.Info);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            IconArea?.Dispose();
            IconArea = null;

            BindList.SelectionChanged -= vm.SelChange;
        }

        private async void QuitBtn_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    IconArea?.Dispose();
                    IconArea = null;

                    System.Windows.Application.Current.Shutdown();
                });
            });
        }

        private void BindList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            vm.EditRuleCommand.Execute(null);
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            if (!init) return;
            if (WindowState != WindowState.Normal) return;
            App.Current.Settings.SaveWindowSettings(this);
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!init) return;
            if (WindowState != WindowState.Normal) return;
            App.Current.Settings.SaveWindowSettings(this);
        }

        private void IPAddress_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (sender == IPAddress)
                {
                    CopyIP();
                }
                else if (sender == IPV6Address)
                {
                    CopyIP(v6: true);
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedItem != null)
            {
                vm.SelectedItem.IsSuspended = !vm.SelectedItem.IsSuspended;
                PortProxyTool.SetPortProxy(vm.SelectedItem);
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            vm.RefreshIP();
            IconArea.BalloonTipText = string.Format(AppResources.IconTip, AppResources.MainTitle, App.Current.WSLAddress);
        }
    }
}