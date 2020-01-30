using Open.Nat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try {
                listenerSocket.Bind(localEndPoint);
                listenerSocket.Listen(10);

                openPort();
          
                while(true) {
                    Socket peerSocket = listenerSocket.Accept();
                    Peer peer = new Peer(peerSocket);
                    IPEndPoint peerIPEndPoint = (peerSocket.RemoteEndPoint as IPEndPoint);

                    PeerAdded?.Invoke(this, new PeerAddedEventArgs() {
                        IPAddress = peerIPEndPoint.Address.ToString(),
                        Port = peerIPEndPoint.Port.ToString(),
                    });

                    //foreach(Peer p in Peers) {
                    //    peer.guid ^= p.guid.GetHashCode();
                    //}

                    Peers.Add(peer);
                    //peer.SendGuid();
                    peer.MessageReceived += MessageReceived;
                }

            } catch(Exception ex) {
                throw ex;
            }
        }

        public async void openPort()
        {
            var discoverer = new NatDiscoverer();
            var cts = new CancellationTokenSource(15000);
            var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

            await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, 5959, 5959, "The mapping name"));
        }

        public void AddPeer(string peerIp)
        {
            string[] two = peerIp.Trim().Split(':');
            peerIp = two[0];
            int PORT = int.Parse(two[1]);

            IPAddress address = GetAddress(peerIp);

            IPAddress[] selfa = Array.FindAll(Dns.GetHostEntry(IPAddress.Loopback).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

            if(address.Equals(selfa[0]) && PORT == this.PORT) {
                throw new InvalidDataException("Cannot connect to self.");
            }

            foreach(Peer p in Peers) {
                IPEndPoint ep = p.Socket.RemoteEndPoint as IPEndPoint;
                if(ep.Address.Equals(address) && ep.Port == PORT) {
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
            IPAddress[] addresses = Dns.GetHostByName(server).AddressList;//Array.FindAll(Dns.GetHostEntry(server).AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

            foreach(IPAddress address in addresses) {
                IPEndPoint ipe = new IPEndPoint(address, port);

                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
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

        public IPAddress GetAddress(string host)
        {
            IPHostEntry e = Dns.GetHostByName(host);
            return e.AddressList[0];
            IPHostEntry hostEntry = Dns.GetHostEntry(host);

            if(hostEntry.AddressList.Length <= 0) {
                return GetAddress(hostEntry.HostName);
            }

            IPAddress[] addresses = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

            return addresses[0];
        }
    }
}
