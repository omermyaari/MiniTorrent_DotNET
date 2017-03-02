using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    class Peer
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public string Port { get; set; }

        public Peer(string Name, string Ip, string Port)
        {
            this.Name = Name;
            this.Ip = Ip;
            this.Port = Port;
        }
    }
}
