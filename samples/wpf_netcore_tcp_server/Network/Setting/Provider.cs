using System;
using System.Collections.Generic;
using System.ComponentModel;
using SettingNetwork.Util;

#nullable enable
namespace SettingNetwork.Setting
{
    public abstract class Provider<T> : ValueNotifier<T>
        where T : class, INotifyPropertyChanged, new()
    {
        protected static Provider<T>? _global = null;
        public static Provider<T> Global
        {
            get
            {
                if (_global == null)
                {
                    _global = default;
                }
                return _global!;
            }
            set
            {
                _global = value;
            }
        }

        // TODO: 컨트롤러로 넘기기
        protected T? _context;
        public T Context
        {
            get
            {
                if (_context == null)
                {
                    SetObservableProperty(ref _context, new T());
                }
                return _context!;
            }
            set
            {
                SetObservableProperty(ref _context, value);
            }
        }

        public abstract void Load();
    }
}
