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
        public string IP { get; set; }
        public string Port { get; set; }

        public Peer(string name, string ip, string port)
        {
            Name = name;
            IP = ip;
            Port = port;
        }
    }
}
