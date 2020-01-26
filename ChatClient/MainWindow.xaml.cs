using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;
using ChatClient.Network;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket Socket;
        private TextTransfer TextTransfer;

        public MainWindow()
        {
            InitializeComponent();

            int port = 8573;

            IPHostEntry hostEntry = Dns.GetHostEntry("8.8.8.8");

            foreach(IPAddress address in hostEntry.AddressList) {
                IPEndPoint ipe = new IPEndPoint(address, port);

                Socket s = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
        }
    }
}
