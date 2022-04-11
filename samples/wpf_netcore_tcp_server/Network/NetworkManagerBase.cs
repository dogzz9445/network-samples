﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SettingNetwork.Core;

namespace SettingNetwork
{
    /// <summary>
    /// NetworkManagerBase
    /// </summary>
    public class NetworkManagerBase
    {
        public event MessageEventHandler MessageReceived;
        protected NetworkModule module;
        public NetworkModule Module { get => module; set => module = value; }

        public NetworkManagerBase()
        {
            Module = new NetworkModule();
            Module.MessageReceived += MessageReceived;
        }

        public void Send(int destinationId, string message, ProtocolType protocolType = ProtocolType.Tcp)
        {
            Module?.Send(destinationId, message, protocolType);
        }

        public void SendFile(int destinationId, string filename)
        {
            Module?.SendFile(destinationId, filename);
        }
    }
}
