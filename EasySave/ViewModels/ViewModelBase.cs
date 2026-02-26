using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySave.ViewModels
{
     
    /// Base class for all view models that implements INotifyPropertyChanged
     
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
         
        /// Event raised when a property changes
         
        public event PropertyChangedEventHandler PropertyChanged;

         
        /// Raises the PropertyChanged event
         
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

         
        /// Sets a property value and raises the PropertyChanged event if the value has changed
         
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}