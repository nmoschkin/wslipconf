using DataTools.MessageBoxEx;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Input;

using WSLIPConf.Helpers;
using WSLIPConf.Localization;
using WSLIPConf.Models;

namespace WSLIPConf.ViewModels
{
    public class ProxySelector
    {
        public string Description { get; set; }
        public ProxyType ProxyType { get; set; }
    }

    public class RuleEditViewModel : ObservableBase
    {
        public event EventHandler AlertClose;

        private static readonly IReadOnlyList<ProxySelector> selectors;
        private bool autoDest;
        private string destAddr;
        private bool destAddrErr;
        private string destAddrErrStr;
        private string destPort;
        private bool destPortErr;
        private string destPortErrStr;
        private List<WSLItem> distributions;
        private WSLItem seldist;
        private string name;
        private bool nameErr;
        private string nameErrStr;
        private WSLMapping oldItem;
        private ProxyProtocol prot;
        private ProxyType prox = ProxyType.V4ToV4;
        private bool result;
        private WSLMapping selItem;
        private ProxySelector selproxy;

        private string srcAddr;
        private bool srcAddrErr;
        private string srcAddrErrStr;
        private string srcPort;
        private bool srcPortErr;
        private string srcPortErrStr;
        private bool suppressValidate;

        static RuleEditViewModel()
        {
            var sel = new List<ProxySelector>
            {
                new ProxySelector()
                {
                    Description = string.Format(AppResources.X_To_X, "IPv4", "IPv4"),
                    ProxyType = ProxyType.V4ToV4,
                },

                new ProxySelector()
                {
                    Description = string.Format(AppResources.X_To_X, "IPv6", "IPv6"),
                    ProxyType = ProxyType.V6ToV6,
                },

                new ProxySelector()
                {
                    Description = string.Format(AppResources.X_To_X, "IPv4", "IPv6"),
                    ProxyType = ProxyType.V4ToV6,
                },

                new ProxySelector()
                {
                    Description = string.Format(AppResources.X_To_X, "IPv6", "IPv4"),
                    ProxyType = ProxyType.V6ToV4,
                }
            };

            selectors = sel.ToArray();
        }

        public RuleEditViewModel(WSLMapping currentItem) : this()
        {
            oldItem = currentItem;
            selItem = oldItem.Clone();

            AnswersToScreen();
            selItem.PropertyChanged += SelItem_PropertyChanged;
        }

        private RuleEditViewModel()
        {
            var defname = WSLDistribution.SessionDefault.Name;

            distributions = new List<WSLItem>()
            {
                new WSLItem()
                {
                    Name = $"{AppResources.Default_Distribution} ({defname})",
                    Distribution = null
                }
            };

            foreach (var dist in WSLDistribution.Distributions)
            {
                distributions.Add(new WSLItem()
                {
                    Name = dist.Name,
                    Distribution = dist
                });
            }

            seldist = distributions[0];

            OKCommand = new SimpleCommand((o) =>
            {
                if (!Validate())
                {
                    MessageBoxEx.Show(AppResources.ErrorCannotSave, AppResources.Error, MessageBoxExType.OK, MessageBoxExIcons.Error);
                    return;
                }

                ApplyChanges();

                Result = true;
                AlertClose?.Invoke(this, new EventArgs());
            });

            CancelCommand = new SimpleCommand((o) =>
            {
                Result = false;
                AlertClose?.Invoke(this, new EventArgs());
            });
        }

        public bool AutoDestination
        {
            get => autoDest;
            set
            {
                if (SetProperty(ref autoDest, value))
                {
                    Validate();
                }
            }
        }

        public ICommand CancelCommand { get; private set; }

        public string DestinationAddress
        {
            get => destAddr;
            set
            {
                if (SetProperty(ref destAddr, value))
                {
                    Validate();
                }
            }
        }

        public string DestinationPort
        {
            get => destPort;
            set
            {
                if (SetProperty(ref destPort, value))
                {
                    Validate();
                }
            }
        }

        public List<WSLItem> Distributions
        {
            get => distributions;
            set
            {
                SetProperty(ref distributions, value);
            }
        }

        public WSLItem SelectedDistribution
        {
            get => seldist;
            set
            {
                if (SetProperty(ref seldist, value))
                {
                    Validate();
                }
            }
        }

        public string Name
        {
            get => name;
            set
            {
                if (SetProperty(ref name, value))
                {
                    Validate();
                }
            }
        }

        public ICommand OKCommand { get; private set; }

        public ProxyProtocol Protocol
        {
            get => prot;
            set
            {
                SetProperty(ref prot, value);
            }
        }

        public IReadOnlyList<ProxySelector> ProxySelectors => selectors;

        public bool Result
        {
            get => result;
            set
            {
                SetProperty(ref result, value);
            }
        }

        public WSLMapping SelectedItem
        {
            get => selItem;
            set
            {
                SetProperty(ref selItem, value);
            }
        }

        public ProxySelector SelectedProxy
        {
            get => selproxy;
            set
            {
                if (SetProperty(ref selproxy, value))
                {
                    Validate();
                }
            }
        }

        public string SourceAddress
        {
            get => srcAddr;
            set
            {
                if (SetProperty(ref srcAddr, value))
                {
                    Validate();
                }
            }
        }

        public string SourcePort
        {
            get => srcPort;
            set
            {
                if (SetProperty(ref srcPort, value))
                {
                    Validate();

                    if (!srcPortErr && (string.IsNullOrEmpty(DestinationPort) || DestinationPort == "0"))
                    {
                        DestinationPort = SourcePort;
                    }
                }
            }
        }

        public string WindowTitle
        {
            get
            {
                if (string.IsNullOrEmpty(selItem.Name)) return AppResources.EditRule;
                return AppResources.EditRule + " - " + (selItem.Changed ? "*" : "") + selItem.Name;
            }
        }

        public static string FormatProxyType(ProxyType proxyType)
        {
            switch (proxyType)
            {
                case ProxyType.V4ToV4:
                    return selectors[0].Description;

                case ProxyType.V6ToV6:
                    return selectors[1].Description;

                case ProxyType.V4ToV6:
                    return selectors[2].Description;

                case ProxyType.V6ToV4:
                    return selectors[3].Description;
            }

            return proxyType.ToString();
        }

        public bool DestAddrError
        {
            get => destAddrErr;
            set
            {
                SetProperty(ref destAddrErr, value);
            }
        }

        public string DestAddrErrorText
        {
            get => destAddrErrStr;
            set
            {
                SetProperty(ref destAddrErrStr, value);
            }
        }

        public bool DestPortError
        {
            get => destPortErr;
            set
            {
                SetProperty(ref destPortErr, value);
            }
        }

        public string DestPortErrorText
        {
            get => destPortErrStr;
            set
            {
                SetProperty(ref destPortErrStr, value);
            }
        }

        public bool NameError
        {
            get => nameErr;
            set
            {
                SetProperty(ref nameErr, value);
            }
        }

        public string NameErrorText
        {
            get => nameErrStr;
            set
            {
                SetProperty(ref nameErrStr, value);
            }
        }

        public bool SrcAddrError
        {
            get => srcAddrErr;
            set
            {
                SetProperty(ref srcAddrErr, value);
            }
        }

        public string SrcAddrErrorText
        {
            get => srcAddrErrStr;
            set
            {
                SetProperty(ref srcAddrErrStr, value);
            }
        }

        public bool SrcPortError
        {
            get => srcPortErr;
            set
            {
                SetProperty(ref srcPortErr, value);
            }
        }

        public string SrcPortErrorText
        {
            get => srcPortErrStr;
            set
            {
                SetProperty(ref srcPortErrStr, value);
            }
        }

        private bool Validate()
        {
            if (suppressValidate) return true;

            int ti;
            bool cd = false, cs = false;
            IPAddress ta;

            selItem.Distribution = SelectedDistribution?.Distribution?.Name;

            selItem.Protocol = prot;

            var np = selproxy?.ProxyType ?? ProxyType.V4ToV4;
            if (np == 0) np = ProxyType.V4ToV4;

            if (selItem.ProxyType != np)
            {
                selItem.ProxyType = np;
                srcAddr = selItem?.SourceAddress?.ToString();
                destAddr = selItem?.DestinationAddress?.ToString();

                cd = cs = true;
                OnPropertyChanged(nameof(WindowTitle));
            }

            if (AutoDestination)
            {
                if ((selItem.ProxyType & ProxyType.DestV6) != 0 &&
                    selItem.DestinationAddress.ToString() != App.Current.WSLV6Address.ToString()
                    )
                {
                    selItem.DestinationAddress = App.Current.WSLV6Address;
                    destAddr = selItem?.DestinationAddress?.ToString();
                    cd = true;
                }
                else if ((selItem.ProxyType & ProxyType.DestV4) != 0 &&
                    selItem.DestinationAddress.ToString() != App.Current.WSLAddress.ToString()
                    )
                {
                    selItem.DestinationAddress = App.Current.WSLAddress;
                    destAddr = selItem?.DestinationAddress?.ToString();
                    cd = true;
                }

                if (selItem.DestinationPort != selItem.SourcePort)
                {
                    selItem.DestinationPort = selItem.SourcePort;
                    destPort = selItem.DestinationPort.ToString();
                    OnPropertyChanged(nameof(DestinationPort));
                }
            }

            if (cd)
            {
                OnPropertyChanged(nameof(DestinationAddress));
            }

            if (cs)
            {
                OnPropertyChanged(nameof(SourceAddress));
            }

            if (string.IsNullOrEmpty(Name))
            {
                NameError = true;
                NameErrorText = AppResources.ErrorFieldBlank;
                selItem.Name = "";
            }
            else
            {
                NameError = false;
                NameErrorText = "";
                selItem.Name = Name;
            }

            if (string.IsNullOrEmpty(SourcePort))
            {
                SrcPortError = true;
                SrcPortErrorText = AppResources.ErrorFieldBlank;
            }
            else if (int.TryParse(SourcePort, out ti))
            {
                if (ti <= 0)
                {
                    SrcPortError = true;
                    SrcPortErrorText = AppResources.ErrorInvalidValue;
                }
                else
                {
                    SrcPortError = false;
                    SrcPortErrorText = "";
                    selItem.SourcePort = ti;
                }
            }
            else
            {
                SrcPortError = true;
                SrcPortErrorText = AppResources.ErrorInvalidFormat;
            }

            if (string.IsNullOrEmpty(DestinationPort))
            {
                DestPortError = true;
                DestPortErrorText = AppResources.ErrorFieldBlank;
            }
            else if (int.TryParse(DestinationPort, out ti))
            {
                if (ti <= 0)
                {
                    DestPortError = true;
                    DestPortErrorText = AppResources.ErrorInvalidValue;
                }
                else
                {
                    DestPortError = false;
                    DestPortErrorText = "";
                    selItem.DestinationPort = ti;
                }
            }
            else
            {
                DestPortError = true;
                DestPortErrorText = AppResources.ErrorInvalidFormat;
            }

            if (string.IsNullOrEmpty(SourceAddress))
            {
                SrcAddrError = true;
                SrcAddrErrorText = AppResources.ErrorFieldBlank;
            }
            else if (IPAddress.TryParse(SourceAddress, out ta))
            {
                if (ta.AddressFamily != SrcFam(selItem.ProxyType))
                {
                    SrcAddrError = true;
                    SrcAddrErrorText = AppResources.ErrorWrongAddress;
                }
                else
                {
                    SrcAddrError = false;
                    SrcAddrErrorText = "";
                    selItem.SourceAddress = ta;
                }
            }
            else
            {
                SrcAddrError = true;
                SrcAddrErrorText = AppResources.ErrorInvalidFormat;
            }

            if (string.IsNullOrEmpty(DestinationAddress))
            {
                DestAddrError = true;
                DestAddrErrorText = AppResources.ErrorFieldBlank;
            }
            else if (IPAddress.TryParse(DestinationAddress, out ta))
            {
                if (ta.AddressFamily != DestFam(selItem.ProxyType))
                {
                    DestAddrError = true;
                    DestAddrErrorText = AppResources.ErrorWrongAddress;
                }
                else
                {
                    DestAddrError = false;
                    DestAddrErrorText = "";
                    selItem.DestinationAddress = ta;
                }
            }
            else
            {
                DestAddrError = true;
                DestAddrErrorText = AppResources.ErrorInvalidFormat;
            }

            return !NameError && !SrcPortError && !DestPortError && !SrcAddrError && !DestAddrError;
        }

        public void AnswersToScreen()
        {
            suppressValidate = true;

            SelectedDistribution = distributions?.FirstOrDefault(x => x.Name == selItem.Distribution) ?? seldist;

            Name = selItem.Name;
            SourceAddress = selItem.SourceAddress?.ToString();
            DestinationAddress = selItem.DestinationAddress?.ToString();
            SourcePort = selItem.SourcePort.ToString();
            DestinationPort = selItem.DestinationPort.ToString();
            Protocol = selItem.Protocol;
            autoDest = selItem.AutoDestination;
            SelectedProxy = selectors.Where(x => x.ProxyType == selItem.ProxyType).FirstOrDefault();
            oldItem.Changed = selItem.Changed = false;

            suppressValidate = false;

            Validate();
        }

        public void ApplyChanges()
        {
            oldItem.Name = selItem.Name;
            oldItem.SourceAddress = selItem.SourceAddress;
            oldItem.SourcePort = selItem.SourcePort;
            oldItem.DestinationAddress = selItem.DestinationAddress;
            oldItem.DestinationPort = selItem.DestinationPort;
            oldItem.AutoDestination = selItem.AutoDestination;
            oldItem.Protocol = selItem.Protocol;
            oldItem.ProxyType = selItem.ProxyType;
            oldItem.Changed = selItem.Changed = false;
            oldItem.Distribution = selItem.Distribution;
        }

        private AddressFamily DestFam(ProxyType t)
        {
            if ((t & ProxyType.DestV6) == ProxyType.DestV6) return AddressFamily.InterNetworkV6;
            else return AddressFamily.InterNetwork;
        }

        private void SelItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WSLMapping.Name)) OnPropertyChanged(nameof(WindowTitle));
            if (e.PropertyName == nameof(WSLMapping.Changed)) OnPropertyChanged(nameof(WindowTitle));
        }

        private AddressFamily SrcFam(ProxyType t)
        {
            if ((t & ProxyType.SourceV6) == ProxyType.SourceV6) return AddressFamily.InterNetworkV6;
            else return AddressFamily.InterNetwork;
        }
    }

    public class WSLItem
    {
        public WSLDistribution Distribution { get; set; }

        public string Name { get; set; }
    }
}