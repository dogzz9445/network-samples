using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SettingNetwork
{
    public class BaseFileTcpClient : NetCoreServer.TcpClient
    {
        private readonly object _sendLock = new object();

        public BaseFileTcpClient(string address, int port) : base(address, port) { }

        public void DisconnectAndStop()
        {
            DisconnectAsync();
            while (IsConnected)
                Thread.Yield();
        }

        protected void SendFileAsync(string filename)
        {
            while (!IsConnected)
            {
                if (IsConnecting == false)
                {
                    if (IsConnected)
                    {
                        break;
                    }
                    else
                    {
                        return;
                    }
                }
                Thread.Yield();
            }

            long sent = Send(Path.GetFileName(filename) + "\n");
            lock (_sendLock)
            {
                try
                {
                    Socket.SendFile(filename);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    DisconnectAndStop();
                }
            }
        }

        public void SendFile(string filename)
        {
            Connect();
            SendFileAsync(filename);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            //string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Console.WriteLine(Encoding.UTF8.GetString(buffer, (int)offset, (int)size));
        }
    }
}
