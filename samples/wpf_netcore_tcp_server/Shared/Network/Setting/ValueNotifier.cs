using System.ComponentModel;
using SettingNetwork.Util;

namespace SettingNetwork
{
    public abstract class ValueNotifier<T> : BindableBase
        where T : INotifyPropertyChanged
    {
        public void Notify()
        {
            RaisePropertyChangedEvent();
        }
    }
}
