using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NetCoreServer;
using SettingNetwork.Util;
using Newtonsoft.Json;

namespace SettingNetwork
{
    public class BaseTcpClient : NetCoreServer.TcpClient
    {
        public event EventHandler<Message> MessageReceived;
        private SlidingStream _stream;
        private byte[] _length = new byte[4];
        private byte[] _buffer = new byte[65536];

        private bool _stop = false;

        public BaseTcpClient(string address, int port) : base(address, port) 
        {
            _stream = new SlidingStream();
        }

        public void DisconnectAndStop()
        {
            _stop = true;
            DisconnectAsync();
            // 타임아웃 설정
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
            Console.WriteLine($"Chat TCP client caught an error with code {error}");
        }

    }
}
