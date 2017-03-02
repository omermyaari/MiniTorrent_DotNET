using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using PeerUI.ServiceReference1;

namespace PeerUI.Communication {
    public class WCFClient {
        private User user;
        private ITorrentWcfService proxy;
        ChannelFactory<ITorrentWcfService> factory;

        public WCFClient(User user) {
            this.user = user;
            ConnectToWcfServer();
        }

        //  Creates a connection to the main server.
        private void ConnectToWcfServer() {
            try {
                EndpointAddress ep = new EndpointAddress(@"http://" + user.ServerIP + ":"
    + user.ServerPort + @"/Wcf/TorrentService");
                factory = new ChannelFactory<ITorrentWcfService>(new BasicHttpBinding(), ep);
                proxy = factory.CreateChannel();
            }
            catch (Exception ex) {
                if (factory != null) {
                    factory.Abort();
                }
            }

        }

        public void DisconnectFromWcfServer() {
            factory.Close();
        }

        //  Generates a xml file request to send to the main server.
        public string GenerateFileRequest(User user, string FileName) {
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
    }
}
