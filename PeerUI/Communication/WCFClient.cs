using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Xml.Linq;
using TorrentWcfServiceLibrary;

namespace PeerUI.Communication {
    public class WCFClient {
        private User user;
        private ITorrentWcfService proxy;
        private ChannelFactory<ITorrentWcfService> factory;

        public WCFClient(User user) {
            this.user = user;
            ConnectToWcfServer();
        }

        //  Creates a connection to the main server.
        private void ConnectToWcfServer() {
            try {
                EndpointAddress ep = new EndpointAddress(@"http://" + user.ServerIP + ":"
       + user.ServerPort + @"/Wcf");
                factory = new ChannelFactory<ITorrentWcfService>(new BasicHttpBinding(), ep);
                proxy = factory.CreateChannel();
            }
            catch (Exception ex) {
                if (factory != null) {
                    factory.Abort();
                }
            }
        }

        //  Closes the connection to the main server.
        public void DisconnectFromWcfServer() {
            GenerateSignOutRequest(user);
            factory.Close();
        }

        //  Generates a xml file request to send to the main server.
        private string GenerateFileRequest(User user, string FileName) {
            XDocument xmlFileRequest = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement("FileRequest",
            new XElement("Username", user.Username),
            new XElement("Password", user.Password),
            new XElement("File", new XAttribute("Name", FileName))));
            return xmlFileRequest.ToString();
        }

        //  Generates a xml sign in request to send to the main server.
        public string GenerateSignInRequest(User user) {
            DirectoryInfo directoryInfo = new DirectoryInfo(user.SharedFolderPath);
            FileInfo[] sharedFilesInfo = directoryInfo.GetFiles("*");
            XDocument xmlSignInRequest = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement("SignInRequest",
                new XElement("Username", user.Username),
                new XElement("Password", user.Password),
                new XElement("UserIP", user.UserIP),
                new XElement("UserPort", user.LocalPort),
                new XElement("Files"))
                );
            foreach (FileInfo fi in sharedFilesInfo) {
                xmlSignInRequest.Element("User").Element("Files").Add(
                    new XElement("File", new XAttribute("Name", fi.Name), new XAttribute("Size", fi.Length)));
            }
            return xmlSignInRequest.ToString();
        }
        //  Generates a xml sign out request to send to the main server.
        public string GenerateSignOutRequest(User user) {
            XDocument xmlSignOutRequest = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement("SignOutRequest",
            new XElement("Username", user.Username),
            new XElement("Password", user.Password)
                ));
            return xmlSignOutRequest.ToString();
        }

        //  Generates a file request from the user and sends it to the server, then returning the result to the user.
        public List<TorrentWcfServiceLibrary.SearchResult> FileRequest(string FileName) {
            return proxy.SearchFile(GenerateFileRequest(user, FileName));
        }
    }
}
