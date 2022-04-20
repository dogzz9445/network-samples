using System;
using System.Net.Sockets;

namespace SettingNetwork
{
    /// <summary>
    /// NetworkManagerBase
    /// </summary>
    public class NetworkManagerBase : IDisposable
    {
        public event EventHandler<Message> MessageReceived;
        public event EventHandler<Packet> PacketReceived;

        protected NetworkModule module = null;
        public NetworkModule Module { get => module; set => module = value; }

        public NetworkManagerBase()
        {
            Module = new NetworkModule();
            Module.MessageReceived += OnMessageReceived;
            Module.PacketReceived += PacketReceived;
        }

        public void OnMessageReceived(object sender, Message message)
        {
            MessageReceived?.Invoke(sender, message);
        }

        public void OnPacketReceived(object sender, Packet packet)
        {
            PacketReceived?.Invoke(sender, packet);
        }

        void IDisposable.Dispose()
        {
            if (Module != null)
            {
                Module.MessageReceived -= MessageReceived;
                Module.PacketReceived -= PacketReceived;
            }
        }

        public void Send(int destinationId, string message,
            ProtocolType protocolType = ProtocolType.Tcp)
        {
            Module?.Send(destinationId, message, protocolType);
        }

        public void Send(int destinationId, Message message, ProtocolType protocolType = ProtocolType.Tcp)
        {
            Module?.Send(destinationId, message, protocolType);
        }

        public void Send(int destinationId, Packet packet, ProtocolType protocolType = ProtocolType.Tcp)
        {
            Module?.Send(destinationId, packet, protocolType);
        }

        public void SendFile(int destinationId, string filename)
        {
            Module?.SendFile(destinationId, filename);
        }

    }
}
