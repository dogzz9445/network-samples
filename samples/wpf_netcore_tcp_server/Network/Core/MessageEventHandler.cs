using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SettingNetwork.Core
{
    public class MessageEventArgs : EventArgs
    {
        public Guid Id;
        public MessageType MessageType;
        public string Message;

        public MessageEventArgs(Guid id, string message, MessageType type = MessageType.None)
        {
            Id = id;
            Message = message;
            MessageType = type;
        }
    }

    public delegate void MessageEventHandler(object sender, MessageEventArgs e);
}
