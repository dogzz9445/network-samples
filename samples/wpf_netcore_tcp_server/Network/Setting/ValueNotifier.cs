using System.ComponentModel;
using SettingNetwork.Util;

namespace SettingNetwork.Setting
{
    public abstract class ValueNotifier<T> : BindableBase
        where T : INotifyPropertyChanged
    {
    }
}
