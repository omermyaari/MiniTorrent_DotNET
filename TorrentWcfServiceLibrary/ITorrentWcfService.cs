using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TorrentWcfServiceLibrary {

    /// <summary>
    /// The services that the main WCF server provides.
    /// </summary>
    [ServiceContract]
    public interface ITorrentWcfService {

        /// <summary>
        /// Searches for a given file name and returns the result to the user.
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns>ServiceMessage</returns>
        [OperationContract]
        ServiceMessage SearchFile(ServiceMessage serviceMessage);

        /// <summary>
        /// Recieves a XML string from the user and performs the request,
        /// then after the request is done, sends back the result to the user.
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <returns>string</returns>
        [OperationContract]
        string Request(string xmlMessage);

        /// <summary>
        /// Serializes a given ServiceMessage to an XML string.
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns>string</returns>
        [OperationContract]
        string SerializeMessage(ServiceMessage serviceMessage);

        /// <summary>
        /// De-serializes a given XML string and produces a ServiceMessage out of it.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [OperationContract]
        ServiceMessage DeSerializeMessage(string message);

        /// <summary>
        /// Used to sign in a user.
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns></returns>
        [OperationContract]
        string UserSignIn(ServiceMessage serviceMessage);
    }

    /// <summary>
    /// Message header used by the ServiceMessage class.
    /// </summary>
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

    /// <summary>
    /// Service message used to send and receive information to and from users.
    /// </summary>
    [DataContract]
    public class ServiceMessage {

        public ServiceMessage() {

        }

        //  Message header.
        [DataMember]
        public MessageHeader Header {
            get; set;
        }

        //  User properties.
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

        //  Files list (contains ServiceDataFiles).
        [DataMember]
        public List<ServiceDataFile> FilesList {
            get; set;
        }
    }

    /// <summary>
    /// Used to hold files information and sharing peers list.
    /// </summary>
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

        //  Peers sharing this file.
        [DataMember]
        public List<PeerAddress> PeerList {
            get; set;
        }
    }

    /// <summary>
    /// Peer details.
    /// </summary>
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
