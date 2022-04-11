using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetCoreServer;
using SettingNetwork.Core;

namespace SettingNetwork
{
    public class NetworkManager : NetworkManagerBase
    {
        protected void OnReceivedMessage(object sender, MessageEventArgs eventArgs)
        {
            Console.WriteLine("Received");
        }
    }
}
