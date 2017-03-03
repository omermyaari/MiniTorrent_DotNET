using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerUI.Entities
{
    class Peer
    {
        public int ID { get; set; }
        public string IP { get; set; }
        public string Port { get; set; }

        public Peer(int id, string ip, string port)
        {
            ID = id;
            IP = ip;
            Port = port;
        }
    }
}
