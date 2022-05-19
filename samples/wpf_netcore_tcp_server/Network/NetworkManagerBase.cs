using NetCoreServer;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SettingNetwork
{
    /// <summary>
    /// NetworkManagerBase
    /// </summary>
    public class NetworkManagerBase : IDisposable
    {
        public event EventHandler<Message> MessageReceived;
        public event EventHandler<Packet> PacketReceived;
        public event EventHandler<NetworkError> ErrorReceived;

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

        public void Dispose()
        {
            if (_module != null)
            {
                Module.MessageReceived -= OnMessageReceived;
                Module.PacketReceived -= OnPacketReceived;
            }
        }

        public void OnMessageReceived(object sender, Message message)
        {
            MessageReceived?.Invoke(sender, message);
        }

        public void OnPacketReceived(object sender, Packet packet)
        {
            PacketReceived?.Invoke(sender, packet);
        }

        public void Send(int destinationId, string data, ProtocolType protocolType = ProtocolType.Tcp)
        {
            Module.Send(destinationId, data, protocolType);
        }

        public void Send(int destinationId, Message message, ProtocolType protocolType = ProtocolType.Tcp)
        {
            Module.Send(destinationId, message, protocolType);
        }

        public void Send(int destinationId, Packet packet, ProtocolType protocolType = ProtocolType.Tcp)
        {
            Module.Send(destinationId, packet, protocolType);
        }

        public void SendFile(int destinationId, string filename)
        {
            Module.SendFile(destinationId, filename);
        }

        public HttpResponse GetRequestAPI(string url)
        {
            return Module.GetRequestAPI(url);
        }

        public async Task<HttpResponse> GetRequestAPIAsync(string url)
        {
            return await Module.GetRequestAPIAsync(url);
        }

        public void GetRequestFile(string url)
        {
            Module.GetRequestFile(url);
        }

        public async Task<HttpResponse> GetRequestFileAsync(string url)
        {
            return await Module.GetRequestFileAsync(url);
        }

        public async Task<HttpResponse> PostRequestAPIAsync(string url, string content)
        {
            return await Module.PutRequestAPIAsync(url, content);
        }
    }
}
