using DataTools.MessageBoxEx;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WSLIPConf.Localization;
using WSLIPConf.Models;

namespace WSLIPConf.ViewModels
{
    public class RuleEditViewModel : ObservableBase
    {

        private WSLMapping selItem;
        private WSLMapping oldItem;

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

        private bool autoDest;

        public event EventHandler AlertClose;

        public ICommand OKCommand { get; private set; }

        public ICommand CancelCommand { get; private set; }


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
            int ti;
            IPAddress ta;

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
                SrcAddrError = false;
                SrcAddrErrorText = "";
                selItem.SourceAddress = ta;
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
                DestAddrError = false;
                DestAddrErrorText = "";
                selItem.DestinationAddress = ta;
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

        public RuleEditViewModel(WSLMapping currentItem) : this()
        {
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

        public void ApplyChanges()
        {
            oldItem.Name = selItem.Name;
            oldItem.SourceAddress = selItem.SourceAddress;
            oldItem.SourcePort = selItem.SourcePort;
            oldItem.DestinationAddress = selItem.DestinationAddress;
            oldItem.DestinationPort = selItem.DestinationPort;
            oldItem.AutoDestination = selItem.AutoDestination;

            oldItem.Changed = selItem.Changed = false;
        }

        public void AnswersToScreen()
        {
            Name = selItem.Name;
            SourceAddress = selItem.SourceAddress?.ToString();
            DestinationAddress = selItem.DestinationAddress?.ToString();
            SourcePort = selItem.SourcePort.ToString();
            DestinationPort = selItem.DestinationPort.ToString();
            autoDest = selItem.AutoDestination;

            oldItem.Changed = selItem.Changed = false;
            Validate();
        }

    }

}
