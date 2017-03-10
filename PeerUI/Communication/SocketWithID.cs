using System.Net.Sockets;

namespace PeerUI {
    public class SocketWithID {
        public Socket Sock {
            get; set;
        }

        public int Id {
            get; set;
        }

        public SocketWithID(Socket Sock, int Id) {
            this.Id = Id;
            this.Sock = Sock;
        }
    }
}
