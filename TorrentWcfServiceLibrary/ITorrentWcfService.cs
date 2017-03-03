using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TorrentWcfServiceLibrary {

    [ServiceContract]
    public interface ITorrentWcfService {
        [OperationContract]
        List<SearchResult> SearchFile(string FileName);
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
