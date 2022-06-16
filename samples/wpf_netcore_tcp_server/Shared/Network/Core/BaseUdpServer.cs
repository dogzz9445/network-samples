using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetCoreServer;
using Newtonsoft.Json;
using SettingNetwork.Util;

namespace SettingNetwork
{
    public class BaseUdpServer : UdpServer
    {
        public event EventHandler<Message> MessageReceived;
        private SlidingStream _stream;
        private byte[] _length = new byte[4];
        private byte[] _buffer = new byte[65536];

        public BaseUdpServer(IPAddress address, int port) : base(address, port)
        {
        }

        protected override void OnStarted()
        {
            // Start receive datagrams
            // ReceiveAsync();
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
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

    }
}
