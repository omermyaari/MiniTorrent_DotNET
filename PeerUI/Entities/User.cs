using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace PeerUI {
    //test test
    [Serializable]
    public class User {

        public string UserIP///////////////////////////
        {
            get; set;
        }

        public string Username {
            get; set;
        }
        
        public string Password {
            get; set;
        }

        public string ServerIP {
            get; set;
        }

        public int ServerPort {
            get; set;
        }

        public int LocalPort {
            get; set;
        }

        public string SharedFolderPath {
            get; set;
        }

        public string DownloadFolderPath {
            get; set;
        }

        public User() {

        }
    }
}
