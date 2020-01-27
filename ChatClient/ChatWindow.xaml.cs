using ChatClient.Models;
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
        public ChatBase ChatBase = new ChatBase();

        public ChatWindow()
        {
            InitializeComponent();

            Loaded += (s, ev) => {
                //foreach(Message message in ChatBase.Messages) {
                //    AppendMessage(message.Text, message.Date);
                //}

                Client.MessageReceived += (s, ev) => {                    
                    Dispatcher.Invoke(() =>
                    {
                        if(ev.Message.StartsWith("<ADD USERNAME>")) {
                            UpdateUsersList(ev.Message);
                            return;
                        }
                        AppendMessage(ev.Message, ev.On);
                        //ChatBase.Messages.Add(new Message() {
                        //    Text = ev.Message,
                        //    Date = ev.On
                        //});
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

        private void AppendMessage(string message, DateTime on)
        {
            ChatTextBox.AppendText(String.Format("[{0}] {1} \n", on.ToShortTimeString(), message));
            ChatTextBox.ScrollToEnd();
        }

        private void UpdateUsersList(string users)
        {
            string[] userList = users
                        .Replace("<ADD USERNAME>[", "")
                        .Replace("]", "")
                        .Split(",");

            foreach(string user in userList) {
                UsersList.Items.Add(user.Trim());
            }
        }
    }
}
