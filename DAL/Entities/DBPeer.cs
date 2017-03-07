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

        public override bool Equals(object obj) {
            DBPeer other = obj as DBPeer;
            return other != null && other.Name.Equals(this.Name) &&
                other.Password.Equals(this.Password) &&
                other.Ip.Equals(this.Ip) &&
                other.Port == this.Port;
        }

        public override int GetHashCode() {
            if (Name == null)
                return 0;
            return Name.GetHashCode();
        }
    }
}
