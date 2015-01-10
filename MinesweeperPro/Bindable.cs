using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MinesweeperPro
{
    public class Bindable : INotifyPropertyChanging, INotifyPropertyChanged
    {
        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string caller = "")
        {
            if (Equals(field, value))
                return false;

            if (PropertyChanging != null)
                PropertyChanging(this, new PropertyChangingEventArgs(caller));

            field = value;

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(caller));

            return true;
        }

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
