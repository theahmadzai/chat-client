using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Network
{
    class TextMessage
    {
        public TextTransfer client { get; set; }
        public string text { get; set; }
    }
}
