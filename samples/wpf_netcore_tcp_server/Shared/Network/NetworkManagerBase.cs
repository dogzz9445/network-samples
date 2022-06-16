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
            _module = new NetworkModule();

            Module.MessageReceived += OnMessageReceived;
            Module.PacketReceived += OnPacketReceived;
            Module.ErrorReceived += OnErrorReceived;
        }

        public void Dispose()
        {
            if (_module != null)
            {
                Module.MessageReceived -= OnMessageReceived;
                Module.PacketReceived -= OnPacketReceived;
                Module.ErrorReceived -= OnErrorReceived;
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
        public void OnErrorReceived(object sender, NetworkError error)
        {
            ErrorReceived?.Invoke(sender, error);
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

        public async Task<HttpResponse> GetRequestFileAsync(string url)
        {
            return await Module.GetRequestFileAsync(url);
        }

        public async Task<HttpResponse> PostRequestFileAsync(string url, string filename)
        {
            return await Module.PostRequestFileAsync(url, filename);
        }

        public async Task<string> GetRequestAPIAsync(string url)
        {
            return await Module.GetRequestAPIAsync(url);
        }

        public async Task<string> PostRequestAPIAsync(string url, string content)
        {
            return await Module.PostRequestAPIAsync(url, content);
        }

        public async Task<string> PutRequestAPIAsync(string url, string content)
        {
            return await Module.PutRequestAPIAsync(url, content);
        }

        public async Task<string> PatchRequestAPIAsync(string url, string content)
        {
            return await Module.PatchRequestAPIAsync(url, content);
        }

        public async Task<string> DeleteRequestAPIAsync(string url, string content)
        {
            return await Module.DeleteRequestAPIAsync(url);
        }

    }
}
