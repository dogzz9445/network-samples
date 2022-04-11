using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;

namespace SettingNetwork.Core
{
    public class BaseFileTcpSession : NetCoreServer.TcpSession
    {
        private bool _isInitialized = false;
        private bool _isSaveFileAbsolutePath;
        private string _fileSavedDirectory = "./Data";
        FileStream _fileStream = null;

        public BaseFileTcpSession(TcpServer server, bool isSaveFileAbsolutePath, string fileSaveDirectory) : base(server)
        {
            _isSaveFileAbsolutePath = isSaveFileAbsolutePath;
            _fileSavedDirectory = fileSaveDirectory;
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
            if (_fileStream != null)
            {
                _fileStream.Close();
            }
        }


        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                string filename = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
                var filePath = _isSaveFileAbsolutePath ?
                    Path.Combine(_fileSavedDirectory, filename) :
                    Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName),
                        _fileSavedDirectory, filename);
                _fileStream = File.Create(filePath);
            }
            else
            {
                if (_fileStream == null)
                {
                    return;
                }
                _fileStream.Write(buffer, (int)offset, (int)size);
                _fileStream.Flush();
            }
            //string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            //Console.WriteLine("Incoming: " + message);

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

    public class BaseFileTcpServer : TcpServer
    {
        bool _isSaveFileAbsolutePath = false;
        string _fileSavedDirectory = "./Data";

        public BaseFileTcpServer(IPAddress address, int port, bool isSaveFileAbsolutePath, string fileSavedDirectory) : base(address, port)
        {
            _isSaveFileAbsolutePath = isSaveFileAbsolutePath;
            _fileSavedDirectory = fileSavedDirectory;
        }

        protected override TcpSession CreateSession()
        {
            return new BaseFileTcpSession(this, _isSaveFileAbsolutePath, _fileSavedDirectory);
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP server caught an error with code {error}");
        }
    }
}
