using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace ChatClient.Network
{
    class TextTransfer
    {
        private StreamWriter textOutput;
        private StreamReader textInput;
        private List<TextChannel> channels = new List<TextChannel>();

        public TextTransfer(Socket socket) 
        {
            if (socket == null) {
                throw new ArgumentNullException("Null socket given!");
            }

            Stream stream = new NetworkStream(socket);
            textInput = new StreamReader(stream);
            textOutput = new StreamWriter(stream);            

            new Thread(ListenText).Start();
        }

        private void ListenText()
        {        
            while(textInput.ReadLine() != null) {
                foreach(TextChannel channel in channels) {
                    channel.textStreamIn(textInput.ReadLine());
                }
            }
        }

        public void SendText(string text)
        {
            textOutput.WriteLine(text);
            textOutput.Flush();
        }

        public void AddChannel(TextChannel channel)
        {
            channels.Add(channel);
        }

        public void RemoveChannel(TextChannel channel)
        {
            channels.Remove(channel);
        }
    }
}
