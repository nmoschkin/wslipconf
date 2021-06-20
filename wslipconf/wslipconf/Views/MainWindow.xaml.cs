using System;

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Forms;

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
        MainWindowViewModel vm;
        bool init = false;

        double x, y;

        public NotifyIcon IconArea { get; private set; }
        ToolStripMenuItem appMenu;

        public MainWindow()
        {
            InitializeComponent();
            var wh = new WindowInteropHelper(this);
            wh.EnsureHandle();
            
            x = Width;
            y = Height;

            IconArea = new NotifyIcon();
            IconArea.Icon = AppResources.wslip;
            IconArea.Visible = true;
            IconArea.BalloonTipText = string.Format(AppResources.IconTip, AppResources.MainTitle, App.Current.WSLAddress);
            IconArea.BalloonTipTitle = AppResources.MainTitle;
            IconArea.BalloonTipIcon = ToolTipIcon.Info;
            IconArea.MouseClick += IconArea_MouseClick;
            IconArea.BalloonTipClicked += IconArea_BalloonTipClicked;

            BuildIconMenu();

            this.StateChanged += MainWindow_StateChanged;
            this.Closing += MainWindow_Closing;
            this.SizeChanged += MainWindow_SizeChanged;
            this.LocationChanged += MainWindow_LocationChanged;

            DataContext = vm = new MainWindowViewModel();
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

            mnu = new ToolStripMenuItem()
            {
                Text = AppResources.Quit
            };

            mnu.Click += Mnu_Click;

            cm.Items.Add(new ToolStripSeparator());
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
                else if (item.Text == AppResources.Quit)
                {
                    QuitBtn_Click(sender, null);
                }
                else if (item.Text == AppResources.CopyIPAddress)
                {
                    CopyIP();
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

        private void CopyIP(bool suppressAlert = false)
        {
            System.Windows.Forms.Clipboard.SetText(App.Current.WSLAddress.ToString());
            if (!suppressAlert) IconArea.ShowBalloonTip(0, AppResources.MainTitle, AppResources.IPAddressCopied, ToolTipIcon.Info);

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BindList.SelectionChanged -= vm.SelChange;
        }

        private void QuitBtn_Click(object sender, RoutedEventArgs e)
        {
            _ = Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    IconArea.Visible = false;
                });

            }).ContinueWith((t) =>
            {
                System.Windows.Application.Current.Shutdown();
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
                CopyIP();   
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            vm.RefreshIP();
            IconArea.BalloonTipText = string.Format(AppResources.IconTip, AppResources.MainTitle, App.Current.WSLAddress);
        }
    }
}
