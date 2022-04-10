using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wpf_netcore_tcp_server.Network.Core
{
    public class PacketEventArgs : EventArgs
    {
        public Guid Id;
        public string Message;

        public PacketEventArgs(Guid id, string message)
        {
            Id = id;
            Message = message;
        }
    }

    public delegate void PacketEventHandler(object sender, PacketEventArgs e);
}
