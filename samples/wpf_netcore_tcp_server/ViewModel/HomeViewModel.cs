using Common;
using Network;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using wpf_netcore_tcp_server.Network.Core;

namespace wpf_netcore_tcp_server.ViewModel
{
    public class HomeViewModel : BindableBase
    {
        private static Func<Action, Task> callOnUiThread = async (handler) =>
            await Application.Current.Dispatcher.InvokeAsync(handler);

        private NetworkManager _networkManager;
        private ObservableCollection<string> _messages;
        public ObservableCollection<string> Messages { get => _messages; set => _messages = value; }

        private DelegateCommand _addHostCommand;
        public DelegateCommand AddHostCommand
        {
            get => _addHostCommand ??= new DelegateCommand(() => 
            {
                NetworkSettingsController.Global.Context.Add(new ComputerInfo() { });
            });
        }

        private DelegateCommand _sendFileCommand;
        public DelegateCommand SendFileCommand
        {
            get => _sendFileCommand ??= new DelegateCommand(() =>
                {
                    _networkManager.Module.SendFile(0, "test.txt");
                });
        }

        public HomeViewModel()
        {
            _networkManager = new NetworkManager();
            Messages = new ObservableCollection<string>();
            _networkManager.ReceivedMessage += OnMessageReceived;
        }

        private async void OnMessageReceived(object sender, MessageEventArgs e)
        {
            await callOnUiThread(() => Messages.Add(e.Message));
        }
    }
}
