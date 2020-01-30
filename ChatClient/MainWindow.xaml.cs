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
        public MainWindow()
        {
            InitializeComponent();

            ConnectButton.Click += (s, ev) => {
                try {
                    Hide();
                    ChatWindow chatWindow = new ChatWindow(UsernameTextBox.Text);
                    chatWindow.Show();
                } catch(Exception ex) {
                    MessageBox.Show(ex.Message);
                }
            };
        }
    }
}
