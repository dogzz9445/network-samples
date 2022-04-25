using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
        private BaseUdpServer _udpServer;
        private HttpServer _httpServer;

        private Dictionary<int, BaseUdpClient> _udpClients;
        private Dictionary<int, BaseTcpClient> _tcpClients;
        private Dictionary<int, BaseFileTcpClient> _fileTcpClients;

        public NetworkModule() : base()
        {
            _udpClients = new Dictionary<int, BaseUdpClient>();
            _tcpClients = new Dictionary<int, BaseTcpClient>();
            _fileTcpClients = new Dictionary<int, BaseFileTcpClient>();
            Initialize();
        }

        public void ClearSessions()
        {
            foreach (var tcpClient in _tcpClients)
            {
                Log($"TCP Clinet Stopped... {tcpClient.Key}");
                tcpClient.Value.MessageReceived -= OnMessageReceived;
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
                _udpServer.MessageReceived -= OnMessageReceived;
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
                _udpServer = new BaseUdpServer(IPAddress.Any, hostComputerInfo.UdpPort ?? 0);
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
                tcpClient.MessageReceived += OnMessageReceived;
                tcpClient.ConnectAsync();
                Log($"Create TCP client at ip [{ipAddress}] port [{port}]");
                _tcpClients.Add(destinationId, tcpClient);
            }
            if (_tcpClients[destinationId] == null)
            {
                return;
            }
            byte[] bufMessage = Encoding.UTF8.GetBytes(message);
            int length = bufMessage.Length;
            string testMessage = Encoding.UTF8.GetString(bufMessage, 0, length);
            byte[] buffer = new byte[4 + length];
            byte[] bufLength = BitConverter.GetBytes(length);
            Array.Copy(bufLength, 0, buffer, 0, 4);
            Array.Copy(bufMessage, 0, buffer, 4, length);
            _tcpClients[destinationId].SendAsync(buffer);
            Log($"Sended to [{destinationId}] message [{message}]");
        }

        public void SendUDP(int destinationId, string message)
        {
            if (_settings == null)
            {
                return;
            }
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
                    SendTCP(destinationId, JsonConvert.SerializeObject(message));
                    break;
                case ProtocolType.Udp:
                    SendUDP(destinationId, JsonConvert.SerializeObject(message));
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
            if (message.Type == MessageType.Packet)
            {
                PacketReceived?.Invoke(sender, JsonConvert.DeserializeObject<Packet>(message.Data));
            }
            else
            {
                MessageReceived?.Invoke(sender, message);
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

        private void Log(string log)
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
