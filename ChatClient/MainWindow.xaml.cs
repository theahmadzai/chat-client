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

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ChatWindow chatWindow = new ChatWindow();

        public MainWindow()
        {
            InitializeComponent();

            ConnectButton.Click += (s, ev) => {
                try {
                    chatWindow.Show();
                    chatWindow.Hide();
                    Show();
                    chatWindow.Client.Connect(ServerTextBox.Text, int.Parse(PortTextBox.Text));
                    chatWindow.Client.SetUsername(UsernameTextBox.Text);
                    chatWindow.Show();
                    Hide();                    
                } catch(Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            };
        }
    }
}
