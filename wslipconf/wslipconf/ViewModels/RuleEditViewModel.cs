using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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

        public event EventHandler AlertClose;

        public ICommand OKCommand { get; private set; }

        public ICommand CancelCommand { get; private set; }

        public RuleEditViewModel()
        {
            OKCommand = new SimpleCommand((o) =>
            {
                ApplyChanges();
                AlertClose?.Invoke(this, new EventArgs());
            });


            CancelCommand = new SimpleCommand((o) =>
            {
                AlertClose?.Invoke(this, new EventArgs());
            });

        }

        public string WindowTitle
        {
            get
            {
                if (string.IsNullOrEmpty(selItem.Name)) return AppResources.EditRule;
                return AppResources.EditRule + " - " + selItem.Name;
            }
        }

        public RuleEditViewModel(WSLMapping currentItem) : this()
        {
            oldItem = currentItem;
            selItem = oldItem.Clone();

            selItem.PropertyChanged += SelItem_PropertyChanged;
        }

        private void SelItem_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WSLMapping.Name)) OnPropertyChanged(nameof(WindowTitle));
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

            oldItem.Changed = selItem.Changed = false;
        }

    }

}
