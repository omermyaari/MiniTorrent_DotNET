using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DBPeer
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public bool IsOnline { get; set; }

        public DBPeer(string Name, string Password, string Ip, int Port)
        {
            this.Name = Name;
            this.Password = Password;
            this.Ip = Ip;
            this.Port = Port;
        }

        public DBPeer() {
        }
    }
}
