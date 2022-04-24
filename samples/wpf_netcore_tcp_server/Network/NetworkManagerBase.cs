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

        protected NetworkModule _module = null;
        public NetworkModule Module 
        { 
            get => _module ??= new NetworkModule();
            set => _module = value; 
        }

        public NetworkManagerBase()
        {
            Module = new NetworkModule();

            Module.MessageReceived += OnMessageReceived;
            Module.PacketReceived += OnPacketReceived;
        }

        public void OnMessageReceived(object sender, Message message)
        {
            MessageReceived?.Invoke(sender, message);
        }

        public void OnPacketReceived(object sender, Packet packet)
        {
            PacketReceived?.Invoke(sender, packet);
        }

        public void Dispose()
        {
            if (_module != null)
            {
                Module.MessageReceived -= OnMessageReceived;
                Module.PacketReceived -= OnPacketReceived;
            }
        }

        public void Send(int destinationId, string data, ProtocolType protocolType = ProtocolType.Tcp)
        {
            Module?.Send(destinationId, data, protocolType);
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
