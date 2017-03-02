using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TorrentWcfServiceLibrary {
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface ITorrentWcfService {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        List<SearchResult> GetPeers(string FileName);
    }

    [DataContract]
    public class SearchResult {

        [DataMember]
        public string FileName {
            get; set;
        }
        [DataMember]
        public long FileSize {
            get; set;
        }
        [DataMember]
        public Dictionary<string, string> PeerList {
            get; set;
        }
    }
}
