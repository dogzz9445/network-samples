using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetCoreServer;
using SettingNetwork.Util;

namespace SettingNetwork
{
    public class BaseUdpClient : UdpClient
    {
        public event EventHandler<Message> MessageReceived;
        private SlidingStream _stream;
        private byte[] _length = new byte[4];
        private byte[] _buffer = new byte[65536];

        public BaseUdpClient(string address, int port) : base(address, port)
        {
        }
    }
}
