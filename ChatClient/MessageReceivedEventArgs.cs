using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }
        public DateTime On { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
