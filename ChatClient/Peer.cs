using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatClient
{
    public class Peer
    {
        public int guid = 0.GetHashCode();

        public Socket Socket;
        private NetworkStream NetworkStream;
        private StreamWriter StreamWriter;
        private StreamReader StreamReader;

        public delegate void MessageReceivedEventHandler(object s, MessageReceivedEventArgs ev);
        public event MessageReceivedEventHandler MessageReceived;

        public Peer(Socket socket)
        {
            Socket = socket;
            NetworkStream = new NetworkStream(socket);
            StreamWriter = new StreamWriter(NetworkStream);
            StreamReader = new StreamReader(NetworkStream);

            new Thread(ReceiveMessageLoop).Start();
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceivedEventHandler handler = MessageReceived;
            handler?.Invoke(this, e);
        }

        private void ReceiveMessageLoop()
        {
            string text;
            while((text = StreamReader.ReadLine()) != null) {                
                OnMessageReceived(new MessageReceivedEventArgs() {
                    Message = text,
                    On = DateTime.Now
                });
            }
        }

        public void SendMessage(string message)
        {
            StreamWriter.WriteLine(message);
            StreamWriter.Flush();
        }

        public void SendGuid()
        {
            SendMessage("GUID:" + guid);
        }

        public IPEndPoint GetEndPoint()
        {
            return Socket.RemoteEndPoint as IPEndPoint;
        }

        public IPAddress GetAddress()
        {
            return GetEndPoint().Address;
        }

        public int GetPort()
        {
            return GetEndPoint().Port;
        }
    }
}
