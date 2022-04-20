using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NetCoreServer;
using Newtonsoft.Json;

namespace SettingNetwork
{
    public class NetworkModule : Consumer<NetworkSettingsController, NetworkSettings>
    {
        public event EventHandler<Message> MessageReceived;
        public event EventHandler<Packet> PacketReceived;

        private NetworkSettings _settings;
        private BaseTcpServer _tcpServer;
        private BaseFileTcpServer _fileServer;
        private UdpServer _udpServer;
        private HttpServer _httpServer;

        private Dictionary<int, BaseTcpClient> _tcpClients;
        private Dictionary<int, BaseFileTcpClient> _fileTcpClients;

        public NetworkModule() : base()
        {
            _tcpClients = new Dictionary<int, BaseTcpClient>();
            _fileTcpClients = new Dictionary<int, BaseFileTcpClient>();
            Initialize();
        }

        public void ClearSessions()
        {
            foreach (var tcpClient in _tcpClients)
            {
                Log($"TCP Clinet Stopped... {tcpClient.Key}");
                tcpClient.Value.DataReceived -= OnDataReceived;
                tcpClient.Value.DisconnectAndStop();
                tcpClient.Value.Dispose();
            }
            _tcpClients.Clear();

            if (_tcpServer != null)
            {
                _tcpServer.MessageReceived -= OnMessageReceived;
                _tcpServer.Stop();
                Log("TCP Server Stopped...");
                _tcpServer.Dispose();
                _tcpServer = null;
            }
            if (_udpServer != null)
            {
                _udpServer.Stop();
                //_ucpServer.MessageReceived -= OnMessageReceived;
                Log("UDP Server Stopped...");
                _udpServer.Dispose();
                _udpServer = null;
            }
            if (_httpServer != null)
            {
                _httpServer.Stop();
                Log("HTTP Server Stopped...");
                _httpServer.Dispose();
                _httpServer = null;
            }
            if (_fileServer != null)
            {
                _fileServer.Stop();
                Log("File Server Stopped...");
                _fileServer.Dispose();
                _fileServer = null;
            }
        }

        public void Initialize()
        {
            _settings = NetworkSettingsController.Global.Context;
            int hostId = _settings.HostId ?? 0;
            ComputerInfo hostComputerInfo = null;
            foreach (var setting in _settings.ComputerInfos)
            {
                if (setting.Id == hostId)
                {
                    hostComputerInfo = setting;
                    break;
                }
            }
            if (hostComputerInfo == null)
            {
                Log("Cannot initialize NetworkManager, there isn't any setting about host computer");
                return;
            }
            if (hostComputerInfo.UseTcpServer ?? false)
            {
                _tcpServer = new BaseTcpServer(IPAddress.Any, hostComputerInfo.TcpPort ?? 0);
                _tcpServer.Start();
                Log("TCP Server Started...");
                _tcpServer.MessageReceived += OnMessageReceived;
            }
            if (hostComputerInfo.UseUdpServer ?? false)
            {
                _udpServer = new UdpServer(IPAddress.Any, hostComputerInfo.UdpPort ?? 0);
                _udpServer.Start();
                Log("UDP Server Started...");
            }
            if (hostComputerInfo.UseHttpServer ?? false)
            {
                _httpServer = new HttpServer(IPAddress.Any, hostComputerInfo.HttpPort ?? 0);
                _httpServer.Start();
                Log("HTTP Server Started...");
            }
            if (hostComputerInfo.UseFileTcpServer ?? false)
            {
                _fileServer = new BaseFileTcpServer(IPAddress.Any, hostComputerInfo.FileTcpPort ?? 0,
                                                    _settings.IsSaveFileAbsolutePath ?? false, _settings.FileSavedDirectory);
                _fileServer.Start();
                Log("File Server Started...");
            }
        }

        public void SendTCP(int destinationId, string message)
        {
            if (_settings == null)
            {
                return;
            }
            if (!_tcpClients.ContainsKey(destinationId))
            {
                ComputerInfo destinationComputerInfo = null;
                foreach (var computerInfo in _settings.ComputerInfos)
                {
                    if (computerInfo.Id == destinationId)
                    {
                        destinationComputerInfo = computerInfo;
                        break;
                    }
                }
                if (destinationComputerInfo == null)
                {
                    return;
                }
                var ipAddress = destinationComputerInfo.IpAddress;
                var port = destinationComputerInfo.TcpPort;
                var tcpClient = new BaseTcpClient(ipAddress, port ?? 5000);
                tcpClient.DataReceived += OnDataReceived;
                tcpClient.ConnectAsync();
                Log($"Create TCP client at ip [{ipAddress}] port [{port}]");
                _tcpClients.Add(destinationId, tcpClient);
            }
            if (_tcpClients[destinationId] == null)
            {
                return;
            }
            _tcpClients[destinationId].SendAsync(message);
            Log($"Sended to [{destinationId}] message [{message}]");
        }

        public void SendUDP(int destinationId, string message)
        {
            // TODO:
        }

        public void Send(int destinationId, string data, ProtocolType protocolType = ProtocolType.Tcp)
        {
            Message message = new Message();
            message.Type = MessageType.None;
            message.Data = data;
            Send(destinationId, message, protocolType);
        }

        public void Send(int destinationId, Message message, ProtocolType protocolType = ProtocolType.Tcp)
        {
            message.Type = MessageType.None;

            switch (protocolType)
            {
                case ProtocolType.Tcp:
                    SendTCP(destinationId, JsonConvert.SerializeObject(message));
                    break;
                case ProtocolType.Udp:
                    SendUDP(destinationId, JsonConvert.SerializeObject(message));
                    break;
                default:
                    break;
            }
        }

        public void Send(int destinationId, Packet packet, ProtocolType protocolType = ProtocolType.Tcp)
        {
            packet.From = _settings.HostId ?? 0;
            packet.To = destinationId;

            Message message = new Message();
            message.Type = MessageType.Packet;
            message.Data = JsonConvert.SerializeObject(packet);

            switch (protocolType)
            {
                case ProtocolType.Tcp:
                    SendTCP(destinationId, JsonConvert.SerializeObject(packet));
                    break;
                case ProtocolType.Udp:
                    SendUDP(destinationId, JsonConvert.SerializeObject(packet));
                    break;
                default:
                    break;
            }
        }

        public void SendFile(int destinationId, string filename)
        {
            if (!_fileTcpClients.ContainsKey(destinationId))
            {
                ComputerInfo destinationComputerInfo = null;
                foreach (var computerInfo in _settings.ComputerInfos)
                {
                    if (computerInfo.Id == destinationId)
                    {
                        destinationComputerInfo = computerInfo;
                        break;
                    }
                }
                if (destinationComputerInfo == null)
                {
                    return;
                }
                var ipAddress = destinationComputerInfo.IpAddress;
                var port = destinationComputerInfo.FileTcpPort;
                var fileTcpClient = new BaseFileTcpClient(ipAddress, port ?? 5002);
                _fileTcpClients.Add(destinationId, fileTcpClient);
            }
            if (_fileTcpClients[destinationId] == null)
            {
                return;
            }
            _fileTcpClients[destinationId].SendFile(filename);
        }

        private void OnMessageReceived(object sender, Message message)
        {
            MessageReceived?.Invoke(sender, message);
        }

        private void OnDataReceived(object sender, string data)
        {
            Message message = JsonConvert.DeserializeObject<Message>(data);
            Log($"Received: message [{message.Data}]");
            if (message.Type == MessageType.None)
            {
                MessageReceived?.Invoke(sender, message);
            }
            else if (message.Type == MessageType.Packet)
            {
                Packet packet = JsonConvert.DeserializeObject<Packet>(message.Data);
                PacketReceived?.Invoke(sender, packet);
            }
        }

        public void RefreshSessions()
        {
            ClearSessions();
            Initialize();
        }

        protected void OnNetworkSettingChanged()
        {
            RefreshSessions();
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnNetworkSettingChanged();
        }


        public void Log(string log)
        {
            if (_settings == null)
            {
                return;
            }
            if (!(_settings.UseDebug ?? false))
            {
                return;
            }

            Console.WriteLine(log);
        }

    }
}
