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
        private int PORT = 10101;

        private List<Peer> Peers = new List<Peer>();

        public event PeerEventHandler PeerAdded;
        public event PeerEventHandler PeerRemoved;
        public event MessageReceivedEventHandler MessageReceived;

        public Client()
        {
            new Thread(ListenPeers).Start();
        }

        private void ListenPeers()
        {
            Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while(!listenerSocket.IsBound) {
                try {
                    listenerSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
                    listenerSocket.Listen(10);
                } catch(Exception) {                    
                    PORT += 1;
                    if(PORT > 10110) {
                        return;
                    }                    
                }
            }

            OpenPort();

            while(true) {
                Socket peerSocket = listenerSocket.Accept();
                Peer peer = new Peer(peerSocket);
                Peers.Add(peer);

                peer.MessageReceived += MessageReceived;
                peer.PeerRemoved += PeerRemoved;
                OnPeerAdded(peer);
            }
        }

        protected virtual void OnPeerAdded(Peer peer)
        {
            PeerAdded?.Invoke(this, new PeerEventArgs() {
                IPAddress = peer.GetRemoteAddress().ToString(),
                Port = peer.GetRemotePort().ToString()
            });
        }

        protected virtual void OnPeerRemoved(Peer peer)
        {
            PeerRemoved?.Invoke(this, new PeerEventArgs() {
                IPAddress = peer.GetRemoteAddress().ToString(),
                Port = peer.GetRemotePort().ToString()
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
            int port = PORT;

            SetAddressAndPort(ref address, ref port);                        

            IPAddress peerAddress = GetAddress(address);

            if((peerAddress.Equals(IPAddress.Loopback) ||
                peerAddress.Equals(GetAddress(string.Empty))) &&
                port == PORT) {
                throw new InvalidDataException("Cannot connect to self.");
            }

            foreach(Peer p in Peers) {
                if(peerAddress.Equals(p.GetRemoteAddress()) && 
                    (port == p.GetRemotePort() || port == p.GetLocalPort()) ||
                    PORT == p.GetLocalPort()) {                    
                    throw new InvalidDataException("This peer alrady exists.");
                }
            }

            Socket peerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            peerSocket.Connect(new IPEndPoint(peerAddress, port));

            Peer peer = new Peer(peerSocket);
            Peers.Add(peer);

            peer.MessageReceived += MessageReceived;
            peer.PeerRemoved += PeerRemoved;
            OnPeerAdded(peer);
        }

        public void RemovePeer(string address)
        {
            int port = PORT;

            SetAddressAndPort(ref address, ref port);

            foreach(Peer p in Peers) {
                if(p.GetRemoteAddress().Equals(IPAddress.Parse(address)) &&
                    p.GetRemotePort() == port) {
                    p.Socket.Close();
                    OnPeerRemoved(p);
                    Peers.Remove(p);
                    break;
                }
            }
        }

        public void SendMessage(string message)
        {
            foreach(Peer peer in Peers) {
                peer.SendMessage(message);
            }
        }

        private IPAddress GetAddress(string address)
        {
            IPHostEntry hostEntry = Dns.GetHostEntry(address);

            if(hostEntry.AddressList.Length <= 0) {
                return IPAddress.Parse(address);
            }

            return Array.Find(hostEntry.AddressList, a => a.AddressFamily == AddressFamily.InterNetwork);
        }

        private void SetAddressAndPort(ref string address, ref int port)
        {
            if(address.Contains(':')) {
                string[] ap = address.Split(':');
                address = ap[0];
                port = int.Parse(ap[1]);
            }
        }
    }
}
