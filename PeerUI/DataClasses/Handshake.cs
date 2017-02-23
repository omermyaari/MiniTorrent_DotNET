using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerUI.DataClasses {
    public class Handshake {

        public string FullFilename {
            get; set;
        }

        public long FileSize {
            get; set;
        }

        public int BufferSize {
            get; set;
        }

        public long Start {
            get; set;
        }

        public long End {
            get; set;
        }
    }
} //
