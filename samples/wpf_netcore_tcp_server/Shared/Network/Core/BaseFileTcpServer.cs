using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;

namespace SettingNetwork
{
    public class BaseFileTcpSession : NetCoreServer.TcpSession
    {
        private bool _isInitialized = false;
        private bool _isSaveFileAbsolutePath;
        private string _fileSavedDirectory = null;
        FileStream _fileStream = null;

        public BaseFileTcpSession(TcpServer server, bool isSaveFileAbsolutePath, string fileSavedDirectory) : base(server)
        {
            _isSaveFileAbsolutePath = isSaveFileAbsolutePath;
            _fileSavedDirectory = fileSavedDirectory ?? throw new ArgumentException("Parameter cannot be null", nameof(fileSavedDirectory));
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
            if (_fileStream != null)
            {
                _fileStream.Flush();
                _fileStream.Close();
            }
        }


        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            if (!_isInitialized)
            {
                _isInitialized = true;
                int index = (int)offset;
                for (; index < size; index++)
                {
                    if (buffer[index] == 0xA)
                    {
                        break;
                    }
                }
                string filename = Encoding.UTF8.GetString(buffer, (int)offset, (int)index);
                var fileDirectory = _isSaveFileAbsolutePath ? _fileSavedDirectory :
                    Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName),
                        _fileSavedDirectory);
                var fullFilePath = Path.Combine(fileDirectory, filename);
                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }
                _fileStream = File.Create(fullFilePath);
                int indexOutOfFilename = index + 1;
                if (size <= indexOutOfFilename)
                {
                    _fileStream.Write(buffer, (int)indexOutOfFilename, (int)size - indexOutOfFilename);
                    _fileStream.Flush();
                }
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
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"Chat TCP session caught an error with code {error}");
        }
    }

    public class BaseFileTcpServer : TcpServer
    {
        bool _isSaveFileAbsolutePath = false;
        string _fileSavedDirectory = null;

        public BaseFileTcpServer(IPAddress address, int port, bool isSaveFileAbsolutePath, string fileSavedDirectory) : base(address, port)
        {
            _isSaveFileAbsolutePath = isSaveFileAbsolutePath;
            _fileSavedDirectory = fileSavedDirectory ?? throw new ArgumentException("Parameter cannot be null", nameof(fileSavedDirectory));
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
