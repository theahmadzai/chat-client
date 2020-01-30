using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static ChatClient.Peer;

namespace ChatClient
{  
    public class Client
    {
        public int PORT = 55555;

        private List<Peer> Peers = new List<Peer>();

        public delegate void PeerAddedEventHandler(object s, PeerAddedEventArgs ev);
        public event PeerAddedEventHandler PeerAdded;

        public event MessageReceivedEventHandler MessageReceived;

        public Client(int port) 
        {
            PORT = port;
            new Thread(ListenPeers).Start();
        }

        public void ListenPeers()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, PORT);    
            Socket listenerSocket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try {
                listenerSocket.Bind(localEndPoint);
                listenerSocket.Listen(10);

                while(true) {
                    Socket peerSocket = listenerSocket.Accept();
                    Peer peer = new Peer(peerSocket);
                    IPEndPoint peerIPEndPoint = (peerSocket.RemoteEndPoint as IPEndPoint);
                    PeerAdded?.Invoke(this, new PeerAddedEventArgs() {
                        IPAddress = peerIPEndPoint.Address.ToString(),
                        Port = peerIPEndPoint.Port.ToString(),
                    });

                    foreach(Peer p in Peers) {
                        peer.guid ^= p.guid.GetHashCode();
                    }

                    Peers.Add(peer);
                    peer.SendGuid();
                    peer.MessageReceived += MessageReceived;
                }

            } catch(Exception ex) {
                throw ex;
            }
        }

        public void AddPeer(string peerIp)
        {
            string[] two = peerIp.Trim().Split(':');
            peerIp = two[0];
            int PORT = int.Parse(two[1]);

            IPAddress ip = IPAddress.Parse(peerIp);
            
            foreach(Peer p in Peers) {
                IPEndPoint ep = p.Socket.RemoteEndPoint as IPEndPoint;
                if(ep.Address.Equals(ip) && ep.Port == PORT) {
                    throw new InvalidDataException("This peer alrady exists.");
                }
            }
            
            Socket peerSocket = ConnectSocket(peerIp, PORT);

            if(peerSocket == null) {
                throw new NullReferenceException("Connection Failed!");
            }

            Peer peer = new Peer(peerSocket);
            IPEndPoint peerIPEndPoint = (peerSocket.RemoteEndPoint as IPEndPoint);
            PeerAdded?.Invoke(this, new PeerAddedEventArgs() {
                IPAddress = peerIPEndPoint.Address.ToString(),
                Port = peerIPEndPoint.Port.ToString(),
            });
            Peers.Add(peer);
            peer.MessageReceived += MessageReceived;
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

        public void SendMessage(string message)
        {
            foreach(Peer peer in Peers) {
                peer.SendMessage(message);
            }
        }
    }
}
