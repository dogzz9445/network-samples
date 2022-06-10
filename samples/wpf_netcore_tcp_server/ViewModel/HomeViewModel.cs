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
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using FireXR;

namespace wpf_netcore_tcp_server.ViewModel
{
    public class HomeViewModel : BindableBase
    {
        private static Func<Action, Task> callOnUiThread = async (handler) =>
            await Application.Current.Dispatcher.InvokeAsync(handler);

        private static HomeViewModel _instance = null;
        public static HomeViewModel Instance { get => _instance; }

        #region Property
        private NetworkManager _networkManager;
        public NetworkManager NetworkManager { get => _networkManager; set => _networkManager = value; }
        private ObservableCollection<Message> _messages;
        public ObservableCollection<Message> Messages { get => _messages; set => SetObservableProperty(ref _messages, value); }

        private ObservableCollection<Packet> _packets;
        public ObservableCollection<Packet> Packets { get => _packets; set => SetObservableProperty(ref _packets, value); }

        private ObservableCollection<string> _logs;
        public ObservableCollection<string> Logs { get => _logs; set => SetProperty(ref _logs, value); }

        public ConcurrentQueue<string> datas = new ConcurrentQueue<string>();

        private int _targetId;
        public int TargetId { get => _targetId; set => SetProperty(ref _targetId, value); }

        private string _selectedFilename = @".\test.txt";
        public string SelectedFilename { get => _selectedFilename; set => SetProperty(ref _selectedFilename, value); }

        private bool _isSendingPeriod;
        public bool IsSendingPeriod { get => _isSendingPeriod; set => SetProperty(ref _isSendingPeriod, value); }
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
                    NetworkManager.Module.SendFile(TargetId, SelectedFilename);
                });
        }

        int i = 0;
        private DelegateCommand _sendTest1Command;
        public DelegateCommand SendTest1Command
        {
            get => _sendTest1Command ??= new DelegateCommand(() =>
            {
                i++;
                NetworkManager.Module.Send(TargetId, $"Test1: {i}");
            });
        }

        private DelegateCommand _sendTest2Command;
        public DelegateCommand SendTest2Command
        {
            get => _sendTest2Command ??= new DelegateCommand(async () =>
            {
                await Task.Yield();
                for (int i = 0; i < 1000; i++)
                {
                    NetworkManager.Send(TargetId, $"hihihii {i}");
                }
            });
        }

        private DelegateCommand _sendPacket1Command;
        public DelegateCommand SendPacket1Command
        {
            get => _sendPacket1Command ??= new DelegateCommand(() =>
            {
                NetworkManager.Module.Send(TargetId, new Packet() { Description = "packet" });
            });
        }

        private DelegateCommand _sendPacket2Command;
        public DelegateCommand SendPacket2Command
        {
            get => _sendPacket2Command ??= new DelegateCommand(() =>
            {
                NetworkManager.Module.Send(TargetId, new Hardware_Status());
            });
        }

        private DelegateCommand _sendPacket3Command;
        public DelegateCommand SendPacket3Command
        {
            get => _sendPacket3Command ??= new DelegateCommand(() =>
            {
                NetworkManager.Module.Send(TargetId, new Scenario_Info() { ScenarioID = "123", ScenarioName = "test" });
            });
        }

        private DelegateCommand _sendPacket4Command;
        public DelegateCommand SendPacket4Command
        {
            get => _sendPacket4Command ??= new DelegateCommand(() =>
            {
                NetworkManager.Module.Send(TargetId, new Instructor_Message() { InstructorMessage = "123123123" });
            });
        }


        private DelegateCommand _sendPacketInitCommand;
        public DelegateCommand SendPacketInitCommand
        {
            get => _sendPacketInitCommand ??= new DelegateCommand(() =>
            {
                NetworkManager.Module.Send(TargetId, new Command_Control() { TrnControlCommand = E_COMMAND_CONTROL_TYPE.ControlType_Init });
            });
        }

        private DelegateCommand _sendPacketStartCommand;
        public DelegateCommand SendPacketStartCommand
        {
            get => _sendPacketStartCommand ??= new DelegateCommand(() =>
            {
                NetworkManager.Module.Send(TargetId, new Command_Control() { TrnControlCommand = E_COMMAND_CONTROL_TYPE.ControlType_Start });
            });
        }

        private DelegateCommand _sendPacketStopCommand;
        public DelegateCommand SendPacketStopCommand
        {
            get => _sendPacketStopCommand ??= new DelegateCommand(() =>
            {
                NetworkManager.Module.Send(TargetId, new Command_Control() { TrnControlCommand = E_COMMAND_CONTROL_TYPE.ControlType_Stop });
            });
        }

        private DelegateCommand _sendPacketShutdownCommand;
        public DelegateCommand SendPacketShutdownCommand
        {
            get => _sendPacketShutdownCommand ??= new DelegateCommand(() =>
            {
                NetworkManager.Module.Send(TargetId, new Command_Control() { TrnControlCommand = E_COMMAND_CONTROL_TYPE.ControlType_Shutdown });
            });
        }


        private DelegateCommand _selectFileCommand;
        public DelegateCommand SelectFileCommand
        {
            get => _selectFileCommand ??= new DelegateCommand(() =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    SelectedFilename = openFileDialog.FileName;
                }
            });
        }


        #endregion

        public HomeViewModel()
        {
            _instance = this;
            NetworkManager = new NetworkManager();
            Messages = new ObservableCollection<Message>();
            Packets = new ObservableCollection<Packet>();
            Logs = new ObservableCollection<string>();
            NetworkManager.MessageReceived += OnMessageReceived;
            NetworkManager.PacketReceived += OnPacketReceived;
            PropertyChanged += OnPropertyChanged;

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
                                Logs.Insert(0, $"{buffer}");
                            }
                        }
                        if (Logs.Count > 500)
                        {
                            Logs.Clear();
                        }
                    });
                    Thread.Sleep(100);
                }
            });

            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Update();
                    Thread.Sleep(1000);
                }
            });
        }

        private void OnMessageReceived(object sender, Message message)
        {
            datas.Enqueue(JsonConvert.SerializeObject(message));
        }
        
        private void OnPacketReceived(object sender, Packet packet)
        {
            datas.Enqueue(JsonConvert.SerializeObject(packet));
        }

        private void Update()
        {
            if (IsSendingPeriod)
            {
                NetworkManager?.Module?.Send(TargetId, new Packet() { Description = "packet" });
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "TargetId" || e.PropertyName == "_targetId")
            {
                IsSendingPeriod = false;
            }
        }
    }
}
