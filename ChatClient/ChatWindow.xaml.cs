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
        public Client Client = new Client();

        public ChatWindow()
        {
            InitializeComponent();

            Loaded += (s, ev) => {
                Client.MessageReceived += (s, ev) => {                    
                    Dispatcher.Invoke(() =>
                    {
                        if(ev.Message.StartsWith("<ADD USERNAME>")) {
                            string[] userList = ev.Message
                                                    .Replace("<ADD USERNAME>[","")
                                                    .Replace("]","")
                                                    .Split(",");
                            foreach(string user in userList) {
                                UsersList.Items.Add(user.Trim());
                            }
                            return;
                        }
                        ChatTextBox.AppendText(String.Format("[{0}] {1} \n", ev.On.ToShortTimeString(), ev.Message));
                        ChatTextBox.ScrollToEnd();                        
                    });
                };
            };

            MessageTextBox.KeyDown += (s, ev) => {
                if(ev.Key == Key.Return) {
                    Client.SendMessage(MessageTextBox.Text);
                    MessageTextBox.Clear();
                }
            };

            SendButton.Click += (s, ev) => {
                Client.SendMessage(MessageTextBox.Text);
                MessageTextBox.Clear();
            };
        }
    }
}
