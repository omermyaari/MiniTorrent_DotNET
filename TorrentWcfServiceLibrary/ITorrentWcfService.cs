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
        ServiceMessage SearchFile(ServiceMessage serviceMessage);

        [OperationContract]
        string Request(string xmlMessage);

        [OperationContract]
        string SerializeMessage(ServiceMessage serviceMessage);

        [OperationContract]
        ServiceMessage DeSerializeMessage(string message);

        [OperationContract]
        string UserSignIn(ServiceMessage serviceMessage);
    }

    [DataContract]
    public enum MessageHeader {
        [EnumMember]
        ConnectionSuccessful,
        [EnumMember]
        ConnectionFailed,
        [EnumMember]
        FileRequest,
        [EnumMember]
        UserSignIn,
        [EnumMember]
        UserSignOut,
        [EnumMember]
        SearchResult,
        [EnumMember]
        FileNotFound
    }

  /*  [DataContract]
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
    */
    [DataContract]
    public class ServiceMessage {

        public ServiceMessage() {

        }

        [DataMember]
        public MessageHeader Header {
            get; set;
        }

        [DataMember]
        public string UserName {
            get; set;
        }

        [DataMember]
        public string UserPassword {
            get; set;
        }

        [DataMember]
        public string UserIP {
            get; set;
        }

        [DataMember]
        public int UserPort {
            get; set;
        }

        [DataMember]
        public List<ServiceDataFile> FilesList {
            get; set;
        }
    }

    [DataContract]
    public class ServiceDataFile {

        public ServiceDataFile() {

        }

        [DataMember]
        public string Name {
            get; set;
        }

        [DataMember]
        public long Size {
            get; set;
        }

        [DataMember]
        public List<PeerAddress> PeerList {
            get; set;
        }
    }

    [DataContract]
    public class PeerAddress {

        public PeerAddress() {

        }

        [DataMember]
        public string Ip {
            get; set;
        }

        [DataMember]
        public int Port {
            get; set;
        }
    }
}
