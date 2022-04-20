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

namespace SettingNetwork
{
    public class BaseTcpSession : NetCoreServer.TcpSession
    {
        public event EventHandler<Message> MessageReceived;
        private Stream _stream;
        private BufferedStream _bufferedStream;
        private byte[] _buffer = new byte[160000];
        int _offset = 0;

        public BaseTcpSession(TcpServer server) : base(server)
        {
            _stream = new MemoryStream();
            _bufferedStream = new BufferedStream(_stream);
        }

        protected override void OnConnected()
        {
            //Console.WriteLine($"Chat TCP session with Id {Id} connected!");

            //// Send invite message
            //string message = "Hello from TCP chat! Please send a message or '!' to disconnect the client!";
            //SendAsync(message);
        }

        protected override void OnDisconnected()
        {
            //Console.WriteLine($"Chat TCP session with Id {Id} disconnected!");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            _bufferedStream.Write(buffer, _offset + (int)offset, (int)size);
            _bufferedStream.Position = _offset;
            _bufferedStream.Flush();
            _offset = _offset + (int)size;

            byte[] length = new byte[4];
            byte[] bufferToRead = new byte[8096];
            _bufferedStream.Read(length, 0, 4);
            _bufferedStream.Read(bufferToRead, 0, BitConverter.ToInt32(length, 0));

            //_streamWriter.Write(data);
            //_streamWriter.Flush();
            //Console.WriteLine(data);
            //MessageReceived?.Invoke(this, data);

            //// Multicast message to all connected sessions
            //Server.Multicast(message);

            //// If the buffer starts with '!' the disconnect the current session
            //if (message == "!")
            //    Disconnect();
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP session caught an error with code {error}");
        }
    }

    public class BaseTcpServer : TcpServer
    {
        public event EventHandler<Message> MessageReceived;

        public BaseTcpServer(IPAddress address, int port) : base(address, port)
        {
        }

        protected override TcpSession CreateSession()
        {
            var session = new BaseTcpSession(this);
            session.MessageReceived += RaiseMessageReceivedEvent;
            return session;
        }

        public void RaiseMessageReceivedEvent(object sender, Message data)
        {
            MessageReceived?.Invoke(sender, data);
        }

        protected override void OnDisconnected(TcpSession session)
        {
            (session as BaseTcpSession).MessageReceived -= RaiseMessageReceivedEvent;
            base.OnDisconnected(session);
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP server caught an error with code {error}");
        }
    }
}
