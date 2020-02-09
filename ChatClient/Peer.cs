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

        public delegate void PeerEventHandler(object s, PeerEventArgs ev);
        public event PeerEventHandler PeerRemoved;

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

        private bool Connected()
        {
            return !(Socket.Poll(1, SelectMode.SelectRead) && Socket.Available == 0);
        }

        private void ReceiveMessageLoop()
        {
            try {
                while(Connected()) {
                    OnMessageReceived(new MessageReceivedEventArgs() {
                        Message = StreamReader.ReadLine(),
                        On = DateTime.Now
                    });
                }
            } catch(Exception) {
                if(Socket.Connected) {
                    PeerRemoved?.Invoke(this, new PeerEventArgs() {
                        IPAddress = GetRemoteAddress().ToString(),
                        Port = GetRemotePort().ToString()
                    });
                    Socket.Close();                    
                }
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

        public IPEndPoint GetRemoteEndPoint()
        {
            return Socket.RemoteEndPoint as IPEndPoint;
        }

        public IPAddress GetRemoteAddress()
        {
            return GetRemoteEndPoint().Address;
        }

        public int GetRemotePort()
        {
            return GetRemoteEndPoint().Port;
        }

        public IPEndPoint GetLocalEndPoint()
        {
            return Socket.LocalEndPoint as IPEndPoint;
        }

        public IPAddress GetLocalAddress()
        {
            return GetLocalEndPoint().Address;
        }

        public int GetLocalPort()
        {
            return GetLocalEndPoint().Port;
        }
    }
}
