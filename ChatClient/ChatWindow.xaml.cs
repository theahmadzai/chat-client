﻿using ChatClient.Models;
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
using static ChatClient.Client;
using static ChatClient.Peer;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public Client Client;
        public ChatBase ChatBase;
        private string username;

        public ChatWindow(string username)
        {
            InitializeComponent();

            this.username = username;

            ChatBase = new ChatBase();

            try {
                Client = new Client();            

                Loaded += (s, ev) => {
                    foreach(Message message in ChatBase.Messages) {
                        AppendMessage(message.Text, message.Date);
                    }
                };

                Client.PeerAdded += new PeerEventHandler(PeerAdded);

                Client.PeerRemoved += new PeerEventHandler(PeerRemoved);

                Client.MessageReceived += new MessageReceivedEventHandler(MessageReceived);

                AddPeerButton.Click += new RoutedEventHandler(AddPeer);

                RemovePeerButton.Click += new RoutedEventHandler(RemovePeer);

                SendButton.Click += new RoutedEventHandler(SendMessage);

                MessageTextBox.KeyDown +=  (s, ev) => {                
                    if(ev.Key == Key.Return) {
                        SendMessage(s, ev);
                    }
                };
            } catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void AddPeer(object s, RoutedEventArgs ev)
        {
            try {
                Client.AddPeer(AddPeerTextBox.Text.Trim());
            }                
            catch(Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void RemovePeer(object s, RoutedEventArgs ev)
        {
            Client.RemovePeer(PeerList.SelectedItem.ToString());
            MessageBox.Show(PeerList.SelectedItem.ToString());
        }

        private void PeerAdded(object s, PeerEventArgs ev)
        {
            Dispatcher.Invoke(() => {
                PeerList.Items.Add(ev.ToString());
            });
        }

        private void PeerRemoved(object s, PeerEventArgs ev)
        {
            Dispatcher.Invoke(() => {
                PeerList.Items.Remove(ev.ToString());
            });
        }

        private void SendMessage(object s, RoutedEventArgs ev)
        {            
            Client.SendMessage(MessageTextBox.Text);
            Dispatcher.Invoke(() => {
                AppendMessage(MessageTextBox.Text, DateTime.Now);
                MessageTextBox.Clear();
            });            
        }

        private void MessageReceived(object s, MessageReceivedEventArgs ev)
        {
            Dispatcher.Invoke(() =>
            {
                AppendMessage(ev.Message, ev.On);

                ChatBase.Messages.Add(new Message() {
                    Text = ev.Message,
                    Date = ev.On
                });
                ChatBase.SaveChanges();
            });
        }

        private void AppendMessage(string message, DateTime on)
        {
            ChatTextBox.AppendText(String.Format("[{0}] {1} \n", on.ToShortTimeString(), message));
            ChatTextBox.ScrollToEnd();
        }
    }
}
