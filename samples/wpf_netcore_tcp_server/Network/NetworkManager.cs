using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AppSettings;
using NetCoreServer;

namespace Network
{
    public class PacketEventArgs : EventArgs
    {
        public Guid Id;
        public string Message;

        public PacketEventArgs(Guid id, string message)
        {
            Id = id;
            Message = message;
        }
    }

    public delegate void PacketEventHandler(object sender, PacketEventArgs e);


    public class CustomTcpSession : TcpSession
    {
        public CustomTcpSession(TcpServer server) : base(server) { }

        public event PacketEventHandler ReceivedMessage;

        protected override void OnConnected()
        {
            Console.WriteLine($"Chat TCP session with Id {Id} connected!");

            // Send invite message
            string message = "Hello from TCP chat! Please send a message or '!' to disconnect the client!";
            SendAsync(message);
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Chat TCP session with Id {Id} disconnected!");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Console.WriteLine("Incoming: " + message);
            ReceivedMessage?.Invoke(this, new PacketEventArgs(Id, message));

            //// Multicast message to all connected sessions
            //Server.Multicast(message);

            //// If the buffer starts with '!' the disconnect the current session
            //if (message == "!")
            //    Disconnect();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP session caught an error with code {error}");
        }
    }

    public class CustomTcpServer : TcpServer
    {
        public CustomTcpServer(IPAddress address, int port) : base(address, port)
        {
        }

        protected override TcpSession CreateSession()
        {
            var session = new CustomTcpSession(this);
            return new CustomTcpSession(this);
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP server caught an error with code {error}");
        }
    }
    public class NetworkManager : Consumer<NetworkSettingsController, NetworkSettings>
    {
        private NetworkSettings _settings;
        private CustomTcpServer _tcpServer;
        private UdpServer _udpServer;
        private HttpServer _httpServer;

        public NetworkManager() : base()
        {
            RefreshSessions();
        }

        public void ClearSessions()
        {
            if (_tcpServer != null)
            {
                _tcpServer.Dispose();
            }
            if (_udpServer != null)
            {
                _udpServer.Dispose();
            }
            if (_httpServer != null)
            {
                _httpServer.Dispose();
            }
        }

        public void InitializeSession()
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
                Console.WriteLine("Cannot initialize NetworkManager, there isn't any setting about host computer");
                return;
            }
            if (hostComputerInfo.UseTcpServer ?? false)
            {
                _tcpServer = new CustomTcpServer(IPAddress.Any, hostComputerInfo.TcpPort ?? 0);
                _tcpServer.Start();
            }
            if (hostComputerInfo.UseUdpServer ?? false)
            {
                _udpServer = new UdpServer(IPAddress.Any, hostComputerInfo.UdpPort ?? 0);
                _udpServer.Start();
            }
            if (hostComputerInfo.UseHttpServer ?? false)
            {
                _httpServer = new HttpServer(IPAddress.Any, hostComputerInfo.HttpPort ?? 0);
                _httpServer.Start();
            }
        }

        public void RefreshSessions()
        {
            ClearSessions();
            InitializeSession();
        }

        protected void OnNetworkSettingChanged()
        {
            RefreshSessions();
        }

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnNetworkSettingChanged();
        }
    }
}
