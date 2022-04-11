using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetCoreServer;

namespace SettingNetwork.Core
{
    public class BaseTcpSession : NetCoreServer.TcpSession
    {
        public event MessageEventHandler MessageReceived;

        public BaseTcpSession(TcpServer server) : base(server) { }

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
            string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            //Console.WriteLine("Incoming: " + message);
            MessageReceived?.Invoke(this, new MessageEventArgs(Id, message));

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
        public event MessageEventHandler MessageReceived;

        public BaseTcpServer(IPAddress address, int port) : base(address, port)
        {
        }

        protected override TcpSession CreateSession()
        {
            var session = new BaseTcpSession(this);
            session.MessageReceived += RaiseMessageReceivedEvent;
            return session;
        }

        public void RaiseMessageReceivedEvent(object sender, MessageEventArgs eventArgs)
        {
            MessageReceived?.Invoke(sender, eventArgs);
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
