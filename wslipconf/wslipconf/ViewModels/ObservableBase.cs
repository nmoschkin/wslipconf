using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Threading;

namespace WSLIPConf.ViewModels
{
    /// <summary>
    /// Base class for observable classes.
    /// </summary>
    public abstract class ObservableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Set a property value if the current value is not equal to the new value and raise the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="backingStore">The value to compare and set.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The property name.</param>
        /// <returns></returns>
        protected virtual bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = null)
        {
            bool pass;
            if (typeof(T).IsValueType)
            {
                pass = !backingStore.Equals(value);
            }
            else
            {
                var bso = backingStore is object;
                var vo = value is object;

                if (!bso && !vo)
                {
                    pass = false;
                }
                else if (!bso || !vo)
                {
                    pass = true;
                }
                else
                {
                    pass = !backingStore.Equals(value);
                }
            }
            if (pass)
            {
                backingStore = value;
                OnPropertyChanged(propertyName);
            }
            return pass;
        }

        /// <summary>
        /// Raise <see cref="INotifyPropertyChanged.PropertyChanged"/>.
        /// </summary>
        /// <param name="propertyName"></param>
        internal virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            });
        }
    }
}