using Common;
using System.ComponentModel;

namespace AppSettings
{
    public abstract class ValueNotifier<T> : BindableBase
        where T : INotifyPropertyChanged
    {
    }
}
