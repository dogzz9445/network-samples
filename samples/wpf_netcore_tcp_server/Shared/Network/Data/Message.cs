using System;

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
