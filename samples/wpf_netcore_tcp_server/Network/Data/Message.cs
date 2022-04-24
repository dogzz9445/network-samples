using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingNetwork
{
    public enum MessageType
    {
        None,
        Packet,
    }

    [Serializable]
    public partial class Message
    {
        public MessageType Type;
        public string Data;
    }
}
