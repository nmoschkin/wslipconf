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
        public ProxyType ProxyType { get; set; }

        public string Description { get; set; }
    }

    public class RuleEditViewModel : ObservableBase
    {
        private WSLMapping selItem;
        private WSLMapping oldItem;
        private List<ProxySelector> selectors = new List<ProxySelector>();
        private ProxySelector selproxy;

        private bool suppressValidate;
        private bool result;

        private bool nameErr;
        private bool srcPortErr;
        private bool destPortErr;
        private bool srcAddrErr;
        private bool destAddrErr;

        private string nameErrStr;
        private string srcPortErrStr;
        private string destPortErrStr;
        private string srcAddrErrStr;
        private string destAddrErrStr;

        private string name;
        private string srcPort;
        private string destPort;
        private string srcAddr;
        private string destAddr;

        private ProxyProtocol prot;
        private ProxyType prox = ProxyType.V4ToV4;

        private bool autoDest;

        public event EventHandler AlertClose;

        public ICommand OKCommand { get; private set; }

        public ICommand CancelCommand { get; private set; }

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

        public ProxyProtocol Protocol
        {
            get => prot;
            set
            {
                SetProperty(ref prot, value);
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

        public bool Result
        {
            get => result;
            set
            {
                SetProperty(ref result, value);
            }
        }

        public bool AutoDestination
        {
            get => autoDest;
            set
            {
                if (SetProperty(ref autoDest, value))
                {
                    if (autoDest)
                    {
                        DestinationAddress = App.Current.WSLAddress.ToString();
                    }
                }
            }
        }

        #region Errors

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

        private bool Validate()
        {
            if (suppressValidate) return true;

            int ti;
            bool cd = false, cs = false;
            IPAddress ta;

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

        #endregion Errors

        private RuleEditViewModel()
        {
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

        public string WindowTitle
        {
            get
            {
                if (string.IsNullOrEmpty(selItem.Name)) return AppResources.EditRule;
                return AppResources.EditRule + " - " + (selItem.Changed ? "*" : "") + selItem.Name;
            }
        }

        public List<ProxySelector> ProxySelectors => selectors;

        public RuleEditViewModel(WSLMapping currentItem) : this()
        {
            selectors.Add(new ProxySelector()
            {
                Description = string.Format(AppResources.Mode_X_To_X, "V4", "V4"),
                ProxyType = ProxyType.V4ToV4,
            });

            selectors.Add(new ProxySelector()
            {
                Description = string.Format(AppResources.Mode_X_To_X, "V6", "V6"),
                ProxyType = ProxyType.V6ToV6,
            });

            selectors.Add(new ProxySelector()
            {
                Description = string.Format(AppResources.Mode_X_To_X, "V4", "V6"),
                ProxyType = ProxyType.V4ToV6,
            });

            selectors.Add(new ProxySelector()
            {
                Description = string.Format(AppResources.Mode_X_To_X, "V6", "V4"),
                ProxyType = ProxyType.V6ToV4,
            });

            oldItem = currentItem;
            selItem = oldItem.Clone();

            AnswersToScreen();
            selItem.PropertyChanged += SelItem_PropertyChanged;
        }

        private void SelItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WSLMapping.Name)) OnPropertyChanged(nameof(WindowTitle));
            if (e.PropertyName == nameof(WSLMapping.Changed)) OnPropertyChanged(nameof(WindowTitle));
        }

        public WSLMapping SelectedItem
        {
            get => selItem;
            set
            {
                SetProperty(ref selItem, value);
            }
        }

        private AddressFamily SrcFam(ProxyType t)
        {
            if ((t & ProxyType.SourceV6) == ProxyType.SourceV6) return AddressFamily.InterNetworkV6;
            else return AddressFamily.InterNetwork;
        }

        private AddressFamily DestFam(ProxyType t)
        {
            if ((t & ProxyType.DestV6) == ProxyType.DestV6) return AddressFamily.InterNetworkV6;
            else return AddressFamily.InterNetwork;
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
        }

        public void AnswersToScreen()
        {
            suppressValidate = true;

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
    }
}