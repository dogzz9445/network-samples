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
        public string Message;

        public MessageEventArgs(Guid id, string message)
        {
            Id = id;
            Message = message;
        }
    }

    public delegate void MessageEventHandler(object sender, MessageEventArgs e);
}
