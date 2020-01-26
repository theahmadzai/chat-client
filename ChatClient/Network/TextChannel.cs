using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Network
{
    interface TextChannel
    {
        void textStreamIn(String text);
        void textStreamOut(String text);
    }
}
