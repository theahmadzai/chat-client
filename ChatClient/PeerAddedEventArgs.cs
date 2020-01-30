using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient
{
    public class PeerAddedEventArgs
    {
        public string IPAddress { get; set; }
        public string Port { get; set; }

        public override string ToString()
        {
            return String.Format("{0}:{1}", IPAddress, Port);
        }
    }
}
