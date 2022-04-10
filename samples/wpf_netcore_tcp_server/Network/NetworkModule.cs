using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using AppSettings;
using NetCoreServer;
using Network;
using wpf_netcore_tcp_server.Network.Core;

namespace wpf_netcore_tcp_server.Network
{
    public enum ProtocolType
    {
        TCP,
        UDP,
        HTTP
    }

    public class NetworkModule : Consumer<NetworkSettingsController, NetworkSettings>
    {
        public event PacketEventHandler ReceivedMessage;
        private NetworkSettings _settings;
        private BaseTcpServer _tcpServer;
        private UdpServer _udpServer;
        private HttpServer _httpServer;

        private Dictionary<int, BaseTcpClient> _tcpClients;

        public NetworkModule() : base()
        {
            _tcpClients = new Dictionary<int, BaseTcpClient>();
            RefreshSessions();
        }

        public void ClearSessions()
        {
            foreach (var tcpClient in _tcpClients)
            {
                Log($"TCP Clinet Stopped... {tcpClient.Key}");
                tcpClient.Value.DisconnectAndStop();
                tcpClient.Value.Dispose();
            }
            _tcpClients.Clear();

            if (_tcpServer != null)
            {
                _tcpServer.ReceivedMessage -= RaiseReceivedMessage;
                _tcpServer.Stop();
                Log("TCP Server Stopped...");
                _tcpServer.Dispose();
            }
            if (_udpServer != null)
            {
                _udpServer.Stop();
                Log("UDP Server Stopped...");
                _udpServer.Dispose();
            }
            if (_httpServer != null)
            {
                _httpServer.Stop();
                Log("HTTP Server Stopped...");
                _httpServer.Dispose();
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
                _tcpServer.ReceivedMessage += RaiseReceivedMessage;
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

        public void Send(int destinationId, string message, ProtocolType protocolType = ProtocolType.TCP)
        {
            switch (protocolType)
            {
                case ProtocolType.TCP:
                    SendTCP(destinationId, message);
                    break;
                default:
                    break;
            }
        }

        private void RaiseReceivedMessage(object sender, PacketEventArgs eventArgs)
        {
            Log($"Received: from [{eventArgs.Id}] message [{eventArgs.Message}]");
            ReceivedMessage?.Invoke(sender, eventArgs);
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
