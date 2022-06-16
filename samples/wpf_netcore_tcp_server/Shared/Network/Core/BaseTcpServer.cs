using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetCoreServer;
using Newtonsoft.Json;
using SettingNetwork.Util;

namespace SettingNetwork
{
    public class BaseTcpSession : NetCoreServer.TcpSession
    {
        public event EventHandler<Message> MessageReceived;
        public event EventHandler<NetworkError> ErrorReceived;
        private SlidingStream _stream;
        private byte[] _length = new byte[4];
        private byte[] _buffer = new byte[65536];

        public BaseTcpSession(TcpServer server) : base(server)
        {
            _stream = new SlidingStream();
        }

        protected override void OnConnected()
        {
        }

        protected override void OnDisconnected()
        {
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            _stream.Write(buffer, (int)offset, (int)size);

            int readOffset = 0;
            while (_stream.CanRead)
            {
                if (size < readOffset + 4)
                {
                    return;
                }
                int nReadLengthBytes = _stream.Read(_length, 0, 4);
                if (nReadLengthBytes < 4)
                {
                    // _stream.Seek(-nReadLengthBytes, SeekOrigin.Current);
                    return;
                }
                readOffset = readOffset + 4;

                int nExpectBytes = BitConverter.ToInt32(_length, 0);
                if (size < readOffset + nExpectBytes)
                {
                    // _stream.Seek(-nReadLengthBytes, SeekOrigin.Current);
                    return;
                }
                int nReadBytes = _stream.Read(_buffer, 0, nExpectBytes);
                if (nReadBytes < nExpectBytes)
                {
                    // _stream.Seek(-(nReadLengthBytes + nReadBytes), SeekOrigin.Current);
                    return;
                }
                readOffset = readOffset + nReadBytes;

                string data = Encoding.UTF8.GetString(_buffer, 0, nReadBytes);
                Message message = JsonConvert.DeserializeObject<Message>(data);
                MessageReceived?.Invoke(this, message);
            }
        }

        protected override void OnError(SocketError error)
        {
            ErrorReceived?.Invoke(this, new NetworkError() { Error = $"{error}" });
        }
    }

    public class BaseTcpServer : TcpServer
    {
        public event EventHandler<Message> MessageReceived;
        public event EventHandler<NetworkError> ErrorReceived;

        public BaseTcpServer(IPAddress address, int port) : base(address, port)
        {
        }

        protected override TcpSession CreateSession()
        {
            var session = new BaseTcpSession(this);
            session.MessageReceived += RaiseMessageReceived;
            session.ErrorReceived += RaiseErrorReceived;
            return session;
        }

        protected override void Dispose(bool disposingManagedResources)
        {
            base.Dispose(disposingManagedResources);

            foreach (var (guid, session) in Sessions)
            {
                (session as BaseTcpSession).MessageReceived -= RaiseMessageReceived;
                (session as BaseTcpSession).ErrorReceived -= RaiseErrorReceived;
            }
        }

        public void RaiseErrorReceived(object sender, NetworkError error)
        {
            ErrorReceived?.Invoke(this, error);
        }

        public void RaiseMessageReceived(object sender, Message data)
        {
            MessageReceived?.Invoke(sender, data);
        }

        protected override void OnDisconnected(TcpSession session)
        {
            (session as BaseTcpSession).MessageReceived -= RaiseMessageReceived;
            base.OnDisconnected(session);
        }

        protected override void OnError(SocketError error)
        {
            RaiseErrorReceived(this, new NetworkError() { Error = $"{error}" });
        }
    }
}
