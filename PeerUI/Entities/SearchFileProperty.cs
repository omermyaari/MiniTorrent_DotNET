using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerUI.Entities {
    public class SearchFileProperty {
        public string Name {
            get; set;
        }
        public long Size {
            get; set;
        }
        public int Peers {
            get; set;
        }

        public SearchFileProperty(string Name, long Size, int Peers) {
            this.Name = Name;
            this.Size = Size;
            this.Peers = Peers;
        }
    }
}
