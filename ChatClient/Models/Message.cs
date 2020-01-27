using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; } 
        public DateTime Date { get; set; }
    }
}
