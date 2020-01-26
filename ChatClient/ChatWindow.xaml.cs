using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        Client Client;

        public ChatWindow(Client client)
        {
            InitializeComponent();

            Client = client;

            client.MessageReceived += (s, e) => {
                MessageBox.Show(e.Message);
                Dispatcher.Invoke(() =>
                {
                    ChatBox.AppendText(e.Message);
                    ChatBox.ScrollToEnd();
                });                
            };
        }
    }
}
