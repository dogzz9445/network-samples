using System;
using System.ComponentModel;

namespace SettingNetwork.Setting
{
    public abstract class Consumer<T, S> : INotifyPropertyChanged, IDisposable
        where T : Provider<S>, new()
        where S : class, INotifyPropertyChanged, new()
    {
        public Consumer()
        {
            if (Provider<S>.Global == null)
            {
                T controller = new T();
            }
            Provider<S>.Global.PropertyChanged += PropertyChanged;
            PropertyChanged += OnPropertyChanged;
        }

        public void Dispose()
        {
            Provider<S>.Global.PropertyChanged -= PropertyChanged;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected abstract void OnPropertyChanged(object sender, PropertyChangedEventArgs e);
    }
}
