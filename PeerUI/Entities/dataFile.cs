using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerUI
{
    public class DataFile
    {
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public List<User> UsersList { get; set; }

        public DataFile(string FileName, long FileSize, List<User> UsersList) {
            this.FileName = FileName;
            this.FileSize = FileSize;
            this.UsersList = UsersList;
        }
    }
}
