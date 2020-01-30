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
        private const int PORT = 7777;

        private List<Peer> Peers = new List<Peer>();

        public delegate void PeerAddedEventHandler(object s, PeerAddedEventArgs ev);

        public event PeerAddedEventHandler PeerAdded;
        public event MessageReceivedEventHandler MessageReceived;

        public Client() 
        {
            new Thread(ListenPeers).Start();
        }

        public void ListenPeers()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, PORT);    
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            listenerSocket.Bind(localEndPoint);
            listenerSocket.Listen(10);

            OpenPort();
          
            while(true) {
                Socket peerSocket = listenerSocket.Accept();
                Peer peer = new Peer(peerSocket);
                Peers.Add(peer);

                peer.MessageReceived += MessageReceived;
                OnPeerAdded(peer);
            }
        }

        protected virtual void OnPeerAdded(Peer peer)
        {
            PeerAdded?.Invoke(this, new PeerAddedEventArgs() {
                IPAddress = peer.GetAddress().ToString(),
                Port = peer.GetPort().ToString()
            });
        }

        public async void OpenPort()
        {
            var discoverer = new NatDiscoverer();
            var cts = new CancellationTokenSource(15000);            
            var device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
            await device.CreatePortMapAsync(new Mapping(Protocol.Tcp, PORT, PORT, "ClientChat"));
        }

        public void AddPeer(string address)
        {            
            IPAddress peerAddress = GetAddress(IPAddress.Parse(address));

            if(peerAddress.Equals(GetAddress(IPAddress.Loopback))){
                throw new InvalidDataException("Cannot connect to self.");
            }

            foreach(Peer p in Peers) {               
                if(peerAddress.Equals(p.GetAddress())) {
                    throw new InvalidDataException("This peer alrady exists.");
                }
            }

            IPEndPoint remoteEndPoint = new IPEndPoint(peerAddress, PORT);
            Socket peerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            peerSocket.Connect(remoteEndPoint);

            Peer peer = new Peer(peerSocket);
            Peers.Add(peer);

            peer.MessageReceived += MessageReceived;
            OnPeerAdded(peer);
        }

        public void SendMessage(string message)
        {
            foreach(Peer peer in Peers) {
                peer.SendMessage(message);
            }
        }

        public IPAddress GetAddress(IPAddress address)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(address);

            if(hostEntry.AddressList.Length <= 0) {
                return address;
            }

            IPAddress[] addresses = Array.FindAll(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);

            return addresses[0];
        }
    }
}
