using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;

namespace wpf_tcp_server.ViewModel
{
    public class HomeViewModel : BindableBase
    {
        private bool _isListening;
        public bool IsListening { get => _isListening; set => SetProperty(ref _isListening, value); }

        private readonly ObservableCollection<string> _logs = new ObservableCollection<string>();

        public HomeViewModel()
        {
            IsListening = false;

        }

        public void AddLog(string log)
        {
            _logs.Add(log);
        }

    }
}
