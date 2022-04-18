using System;
using System.Net.Sockets;
using SettingNetwork.Core;
using SettingNetwork.Data;

namespace SettingNetwork
{
    /// <summary>
    /// NetworkManagerBase
    /// </summary>
    public class NetworkManagerBase
    {
        public event EventHandler<Packet> PacketReceived;
        public event MessageEventHandler MessageReceived;
        protected NetworkModule module = null;
        public NetworkModule Module { get => module; set => module = value; }

        public NetworkManagerBase()
        {
            Module = new NetworkModule();
            Module.MessageReceived += MessageReceived;
        }

        public void SendNonePackagedMessage(int destinationId, string message,
            ProtocolType protocolType = ProtocolType.Tcp)
        {
            Module?.Send(destinationId, message, protocolType, MessageType.None);
        }

        public void Send(int destinationId, string message, ProtocolType protocolType = ProtocolType.Tcp, MessageType messageType = MessageType.None)
        {
            Module?.Send(destinationId, message, protocolType, messageType);
        }

        public void SendFile(int destinationId, string filename)
        {
            Module?.SendFile(destinationId, filename);
        }
    }
}
