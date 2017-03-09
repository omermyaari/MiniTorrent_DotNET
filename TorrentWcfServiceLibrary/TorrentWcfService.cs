using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DAL.Entities;
using System.Xml.Serialization;
using System.Threading;

namespace TorrentWcfServiceLibrary {

    /// <summary>
    /// This class specifies how the WCF server's services are supplied.
    /// </summary>
    public class TorrentWcfService : ITorrentWcfService {

        //  XML serializer object.
        private XmlSerializer xmlSerializer = new XmlSerializer(typeof(ServiceMessage));

        /// <summary>
        /// Users call this function to send and receive messages to / from the main server.
        /// </summary>
        /// <param name="xmlMessage"></param>
        /// <returns>string</returns>
        public string Request(string xmlMessage) {
            //  De-serialize the message recieved from the user.
            var serviceMessage = DeSerializeMessage(xmlMessage);
            switch (serviceMessage.Header) {
                //  If its a sign in message.
                case MessageHeader.UserSignIn:
                    return UserSignIn(serviceMessage);
                //  If its a sign out message.
                case MessageHeader.UserSignOut:
                    DBPeer dbPeer = new DBPeer(serviceMessage.UserName, serviceMessage.UserPassword, serviceMessage.UserIP, serviceMessage.UserPort);
                    Console.WriteLine("Client IP: " + serviceMessage.UserIP + " Port: " + serviceMessage.UserPort + " Disconnected.");
                    DAL.DBAccess.SetPeerStatus(dbPeer, false);
                    return null;
                //  If its a file request message.
                case MessageHeader.FileRequest:
                    serviceMessage = SearchFile(serviceMessage);
                    return SerializeMessage(serviceMessage);
                default:
                    return null;
            }
        }

        /// <summary>
        /// User sign in method, checks the peer's username and password, marks him online in the database,
        /// and updates the shared files table in the database.
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns></returns>
        public string UserSignIn(ServiceMessage serviceMessage) {
            //  If the ID counter is 0, it means that this is the first user signing in since the server started,
            //  so the server has to retrieve the last file id to generate a unique id number.
            if (DAL.DBAccess.IdCounter == 0)
                DAL.DBAccess.UpdateLastId();
            //  Create a new peer instance with the supplied user properties.
            var peer = new DBPeer(serviceMessage.UserName, serviceMessage.UserPassword, 
                serviceMessage.UserIP, serviceMessage.UserPort);
            //  If the login credentials are correct.
            if (DAL.DBAccess.CheckPeerAuth(peer)) {
                //  Update the database with the current peer's IP and port.
                DAL.DBAccess.LoginPeer(peer);
                //  Mark the peer as online.
                DAL.DBAccess.SetPeerStatus(peer, true);
                //  Insert the files shared by the peer to the File_Peer table.  
                foreach (ServiceDataFile sdf in serviceMessage.FilesList) {
                    DBFile tempDBFile = new DBFile(sdf.Name, sdf.Size);
                    DAL.DBAccess.AddFile(tempDBFile, peer);
                }
                serviceMessage.Header = MessageHeader.ConnectionSuccessful;
                Console.WriteLine("Client IP: " + serviceMessage.UserIP + " Port: " + serviceMessage.UserPort + " Connected.");
                return SerializeMessage(serviceMessage);
            }
            //  If the login credentials aren't correct.
            else {
                serviceMessage.Header = MessageHeader.ConnectionFailed;
                return SerializeMessage(serviceMessage);
            }
        }

        /// <summary>
        /// Searches in the DB for a given file name (file name is in the ServiceMessage's file list).
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns>ServiceMessage</returns>
        public ServiceMessage SearchFile(ServiceMessage serviceMessage) {
            //  If the file name is null or empty, return file not found.
            if (String.IsNullOrEmpty(serviceMessage.FilesList[0].Name)) {
                serviceMessage.Header = MessageHeader.FileNotFound;
                return serviceMessage;
            }
            //  Search the DB for the given file name.
            //  Retrive a dictionary containing files as keys and lists of peers as values.
            Dictionary<DBFile, List<DBPeer>> DBResults = DAL.DBAccess.SearchFiles(serviceMessage.FilesList[0].Name);
            List<ServiceDataFile> searchResults = new List<ServiceDataFile>();
            //  For each file from the search dictionary result, add it to the search results list,
            //  and add the peers sharing it to a new list.
            foreach (DBFile dbfile in DBResults.Keys) {
                var sdf = new ServiceDataFile {
                    Name = dbfile.Name,
                    Size = dbfile.Size
                };
                sdf.PeerList = new List<PeerAddress>();
                //  For each peer sharing the file, if the peer is online and its not the requesting peer,
                //  add it to the list.
                foreach(DBPeer dbpeer in DBResults[dbfile]) {
                    if (dbpeer.IsOnline && !(dbpeer.Ip == serviceMessage.UserIP && dbpeer.Port == serviceMessage.UserPort)) {
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

        /// <summary>
        /// DeSerializes string messages received from the client.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>ServiceMessage</returns>
        public ServiceMessage DeSerializeMessage(string message) {
            ServiceMessage serviceMessage;
            using (TextReader reader = new StringReader(message)) {
                serviceMessage = (ServiceMessage)xmlSerializer.Deserialize(reader);
            }
            return serviceMessage;
        }

        /// <summary>
        /// Serializes messages to be sent to the client.
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns>string</returns>
        public string SerializeMessage(ServiceMessage serviceMessage) {
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb)) {
                xmlSerializer.Serialize(writer, serviceMessage);
            }
            return sb.ToString();
        }
    }
}