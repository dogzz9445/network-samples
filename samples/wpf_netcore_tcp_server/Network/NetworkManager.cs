using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NetCoreServer;
using Newtonsoft.Json;
using SettingNetwork.Core;
using SettingNetwork.Data;

namespace SettingNetwork
{
    public class NetworkManager : NetworkManagerBase
    {
        public EventHandler<Packet> PacketReceived;

        public NetworkManager() : base()
        {
            MessageReceived += OnMessageReceived;
        }

        /// <summary>
        /// Called when [message received].
        /// 메시지를 패킷으로 변환하여 PacketReceived 이벤트 발생
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="MessageEventArgs"/> instance containing the event data.</param>
        public void OnMessageReceived(object sender, MessageEventArgs eventArgs)
        {
            if (eventArgs.MessageType == MessageType.Packet)
            {
                var packet = JsonConvert.DeserializeObject<Packet>(eventArgs.Message);
                PacketReceived?.Invoke(this, packet);
            }
        }
    }
}
