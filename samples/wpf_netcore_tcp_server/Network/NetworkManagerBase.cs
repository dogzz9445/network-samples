using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using wpf_netcore_tcp_server.Network.Core;

namespace wpf_netcore_tcp_server.Network
{
    /// <summary>
    /// 브릿지 패턴
    /// </summary>
    public abstract class NetworkManagerBase
    {
        public event PacketEventHandler ReceivedMessage;
        protected NetworkModule module;
        public NetworkModule Module { get => module; set => module = value; }

        public NetworkManagerBase()
        {
            Module = new NetworkModule();
            Module.ReceivedMessage += ReceivedMessage;
            ReceivedMessage += OnReceivedMessage;
        }

        protected abstract void OnReceivedMessage(object sender, PacketEventArgs eventArgs);

        public void Send(int destinationId, string message, ProtocolType protocolType = ProtocolType.Tcp)
        {
            Module?.Send(destinationId, message, protocolType);
        }

        public void SendFile(int destinationId, string message, ProtocolType protocolType = ProtocolType.Tcp)
        {
            Module?.Send(destinationId, message, protocolType);
        }
    }
}
