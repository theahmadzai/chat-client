using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatClient
{  
    public class Client
    {
        public delegate void MessageReceivedEventHandler(object sender, MessageReceivedEventArgs e);

        private NetworkStream NetworkStream;
        private StreamWriter StreamWriter;
        private StreamReader StreamReader;
        public event MessageReceivedEventHandler MessageReceived;

        public Client(string server, int port)
        {
            Socket socket = ConnectSocket(server, port);

            if(socket == null) {
                throw new NullReferenceException("Connection Failed!");
            }


            NetworkStream = new NetworkStream(socket);
            StreamWriter = new StreamWriter(NetworkStream);
            StreamReader = new StreamReader(NetworkStream);

            new Thread(ReceiveMessage).Start();
        }

        public static Client Connect(string server, int port)
        {
            return new Client(server, port);
        }

        private Socket ConnectSocket(string server, int port)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(server);

            foreach(IPAddress address in hostEntry.AddressList) {
                IPEndPoint ipe = new IPEndPoint(address, port);

                Socket socket = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipe);

                if(socket.Connected) {
                    return socket;
                }
            }

            return null;
        }

        protected virtual void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceivedEventHandler handler = MessageReceived;
            handler?.Invoke(this, e);
        }

        private void ReceiveMessage() 
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
    }
}
