using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using WSLIPConf.Helpers;

namespace WSLIPConf.ViewModels
{
    public class DistroPickerViewModel : ObservableBase
    {
        private ObservableCollection<WSLDistribution> dists;
        private WSLDistribution seldist;

        public ObservableCollection<WSLDistribution> Distributions
        {
            get => dists;
            set
            {
                SetProperty(ref dists, value);
            }
        }

        public WSLDistribution SelectedDistribution
        {
            get => seldist;
            set
            {
                SetProperty(ref seldist, value);
            }
        }

        public DistroPickerViewModel()
        {
            Distributions = WSLDistribution.Distributions;
            SelectedDistribution = App.Current.SessionDefault;
        }
    }
}