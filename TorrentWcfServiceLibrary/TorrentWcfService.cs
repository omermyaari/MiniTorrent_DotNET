using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DAL.Entities;
using System.Xml.Serialization;

namespace TorrentWcfServiceLibrary {
    public class TorrentWcfService : ITorrentWcfService {

        private XmlSerializer xmlSerializer = new XmlSerializer(typeof(ServiceMessage));

        //  Users call this function to send and receive messages to / from the main server.
        public string Request(string xmlMessage) {
            var serviceMessage = DeSerializeMessage(xmlMessage);
            switch (serviceMessage.Header) {
                case MessageHeader.UserSignIn:
                    return UserSignIn(serviceMessage);
                case MessageHeader.UserSignOut:
                    DBPeer dbPeer = new DBPeer(serviceMessage.UserName, serviceMessage.UserPassword, null, 0);
                    DAL.DBAccess.SetPeerStatus(dbPeer, false);
                    return null;
                case MessageHeader.FileRequest:
                    serviceMessage = SearchFile(serviceMessage);
                    return SerializeMessage(serviceMessage);
                default:
                    return null;
            }
        }

        //  User sign in method
        public string UserSignIn(ServiceMessage serviceMessage) {
            var peer = new DBPeer(serviceMessage.UserName, serviceMessage.UserPassword, 
                serviceMessage.UserIP, serviceMessage.UserPort);
            if (DAL.DBAccess.PeerExists(peer)) {
                DAL.DBAccess.SetPeerStatus(peer, true);
                foreach (ServiceDataFile sdf in serviceMessage.FilesList) {
                    DBFile tempDBFile = new DBFile(sdf.Name, sdf.Size);
                    DAL.DBAccess.AddFile(tempDBFile, peer);
                }
                serviceMessage.Header = MessageHeader.ConnectionSuccessful;
                return SerializeMessage(serviceMessage);

            }
            else {
                serviceMessage.Header = MessageHeader.ConnectionFailed;
                return SerializeMessage(serviceMessage);
            }
        }

        //  Searches in the DB and returns a list of files and peers sharing them.
        public ServiceMessage SearchFile(ServiceMessage serviceMessage) {
            if (String.IsNullOrEmpty(serviceMessage.FilesList[0].Name)) {
                serviceMessage.Header = MessageHeader.FileNotFound;
                return serviceMessage;
            }
            //  TODO generate list from db;
            Dictionary<DBFile, List<DBPeer>> DBResults = DAL.DBAccess.SearchFiles(serviceMessage.FilesList[0].Name);
            List <ServiceDataFile> searchResults = new List<ServiceDataFile>();
            foreach (DBFile dbfile in DBResults.Keys) {
                var sdf = new ServiceDataFile {
                    Name = dbfile.Name,
                    Size = dbfile.Size
                };
                sdf.PeerList = new List<PeerAddress>();
                foreach(DBPeer dbpeer in DBResults[dbfile]) {
                    if (dbpeer.IsOnline) {
                        sdf.PeerList.Add(new PeerAddress {
                            Ip = dbpeer.Ip,
                            Port = dbpeer.Port
                        });
                    }
                };
                if (sdf.PeerList.Count > 0)
                    searchResults.Add(sdf);
            }
            serviceMessage.FilesList = searchResults;
            return serviceMessage;
        }

        //  DeSerializes string messages received from the main server.
        public ServiceMessage DeSerializeMessage(string message) {
            ServiceMessage serviceMessage;
            using (TextReader reader = new StringReader(message)) {
                serviceMessage = (ServiceMessage)xmlSerializer.Deserialize(reader);
            }
            return serviceMessage;
        }

        //  Serializes ServiceMessages to be sent to the main server.
        public string SerializeMessage(ServiceMessage serviceMessage) {
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb)) {
                xmlSerializer.Serialize(writer, serviceMessage);
            }
            return sb.ToString();
        }
    }
}
