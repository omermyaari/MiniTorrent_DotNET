using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TorrentWcfServiceLibrary {
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class TorrentWcfService : ITorrentWcfService {
        public string GetData(int value) {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetPeers(string FileName) {
            if (FileName == null) {
                throw new ArgumentNullException("Null string received at web service.");
            }
            var ct = new CompositeType {
                FileName = FileName,
                FileSize = 1955,
                PeerList = new Dictionary<string, string> {
                    {"10.0.0.1", "4001"}
                }
            };
            return ct;
        }
    }
}
