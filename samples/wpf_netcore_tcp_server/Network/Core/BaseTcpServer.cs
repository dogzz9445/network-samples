﻿using System;
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
            string aaa = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            _bufferedStream.Write(buffer, (int)offset, (int)size);
            _bufferedStream.Position = 0;
            _bufferedStream.Flush();

            int readOffset = 0;
            byte[] length = new byte[4];
            byte[] bufferToRead = new byte[65536];
            if (_bufferedStream.CanRead)
            {
                if (size < readOffset + 4)
                {
                    return;
                }
                int nReadLengthBytes = _bufferedStream.Read(length, 0, 4);
                if (nReadLengthBytes < 4)
                {
                    _bufferedStream.Seek(-nReadLengthBytes, SeekOrigin.Current);
                    return;
                }
                readOffset = readOffset + 4;

                int nExpectBytes = BitConverter.ToInt32(length, 0);
                if (size < readOffset + nExpectBytes)
                {
                    _bufferedStream.Seek(-nReadLengthBytes, SeekOrigin.Current);
                    return;
                }
                int nReadBytes = _bufferedStream.Read(bufferToRead, 4, nExpectBytes);
                if (nReadBytes < nExpectBytes)
                {
                    _bufferedStream.Seek(-(nReadLengthBytes + nReadBytes), SeekOrigin.Current);
                    return;
                }
                readOffset = readOffset + nReadBytes;

                string data = Encoding.UTF8.GetString(bufferToRead, 4, nReadBytes);
                Message message = JsonConvert.DeserializeObject<Message>(data);
                MessageReceived?.Invoke(this, message);
            }

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
