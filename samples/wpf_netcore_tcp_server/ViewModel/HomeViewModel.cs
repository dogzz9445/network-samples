using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SettingNetwork;
using SettingNetwork.Util;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Concurrent;

namespace wpf_netcore_tcp_server.ViewModel
{
    public class HomeViewModel : BindableBase
    {
        private static Func<Action, Task> callOnUiThread = async (handler) =>
            await Application.Current.Dispatcher.InvokeAsync(handler);

        #region Property
        private NetworkManager _networkManager;
        private ObservableCollection<Message> _messages;
        public ObservableCollection<Message> Messages { get => _messages; set => SetObservableProperty(ref _messages, value); }

        private ObservableCollection<Packet> _packets;
        public ObservableCollection<Packet> Packets { get => _packets; set => SetObservableProperty(ref _packets, value); }

        private ObservableCollection<string> _logs;
        public ObservableCollection<string> Logs { get => _logs; set => SetProperty(ref _logs, value); }

        public ConcurrentQueue<string> datas = new ConcurrentQueue<string>();
        #endregion

        #region Commands

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
        #endregion

        public HomeViewModel()
        {
            _networkManager = new NetworkManager();
            Messages = new ObservableCollection<Message>();
            Packets = new ObservableCollection<Packet>();
            Logs = new ObservableCollection<string>();
            _networkManager.MessageReceived += OnMessageReceived;
            _networkManager.PacketReceived += OnPacketReceived;

            Task.Factory.StartNew(async () =>
            {
                string buffer = null;
                while (true)
                {
                    await callOnUiThread(() =>
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            if (datas.TryDequeue(out buffer))
                            {
                                Logs.Add($"{buffer}");
                            }
                        }
                    }
                    );
                    //Logs.Add($"{JsonConvert.SerializeObject(message)}");
                    //for (int i = 0; i < 1000; i++)
                    //{
                    //    _networkManager.Send(0, "hihihii");
                    //}
                    Thread.Sleep(100);
                }
            });

            Task.Factory.StartNew(async () =>
            {
                await Task.Yield();
                Thread.Sleep(5000);
                for (int i = 0; i < 1000; i++)
                {
                    _networkManager.Send(0, $"hihihii {i}");
                    Thread.Sleep(100);
                    //Task.Yield();
                }
            });
        }

        private void OnMessageReceived(object sender, Message message)
        {
            //await callOnUiThread(() => Messages.Add(message));
            //await callOnUiThread(() => 
            datas.Enqueue(JsonConvert.SerializeObject(message));
            //Logs.Add($"{JsonConvert.SerializeObject(message)}");
            //);
        }

        private async void OnPacketReceived(object sender, Packet packet)
        {
            //await callOnUiThread(() => Packets.Add(packet));
            await callOnUiThread(() => Logs.Add($"{JsonConvert.SerializeObject(packet)}"));
        }
    }
}
