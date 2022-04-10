using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using AppSettings;
using NetCoreServer;
using wpf_netcore_tcp_server.Network;
using wpf_netcore_tcp_server.Network.Core;

namespace Network
{
    public class NetworkManager : NetworkManagerBase
    {
        protected override void OnReceivedMessage(object sender, PacketEventArgs eventArgs)
        {
            Console.WriteLine("Received");
        }
    }
}
