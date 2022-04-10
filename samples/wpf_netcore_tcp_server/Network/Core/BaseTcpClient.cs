using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetCoreServer;

namespace wpf_netcore_tcp_server.Network.Core
{
    public class BaseTcpClient : NetCoreServer.TcpClient
    {
        public event PacketEventHandler ReceivedMessage;

        private bool _stop;

        public BaseTcpClient(string address, int port) : base(address, port) { }

        public void DisconnectAndStop()
        {
            _stop = true;
            DisconnectAsync();
            while (IsConnected)
                Thread.Yield();
        }

        protected override void OnConnected()
        {
            Console.WriteLine($"Chat TCP client connected a new session with Id {Id}");
        }

        protected override void OnDisconnected()
        {
            Console.WriteLine($"Chat TCP client disconnected a session with Id {Id}");

            // Wait for a while...
            Thread.Sleep(1000);

            // Try to connect again
            if (!_stop)
                ConnectAsync();
        }

        public void SendFile(string filename)
        {
            Socket.SendFile(filename);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            // Console.WriteLine(Encoding.UTF8.GetString(buffer, (int)offset, (int)size));
            ReceivedMessage?.Invoke(this, new PacketEventArgs(Id, message));
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP client caught an error with code {error}");
        }

    }
}
