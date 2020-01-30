using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace ChatClient.Models
{
    public class ChatBase : DbContext
    {
        public DbSet<Message> Messages { get; set; }

        private static bool _created = false;

        public ChatBase() 
        {
            if(!_created) {
                _created = true;
                Database.EnsureCreated();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionbuilder)
        {
            optionbuilder.UseSqlite(@"Data Source=Chat.db");
        }

    }
}
