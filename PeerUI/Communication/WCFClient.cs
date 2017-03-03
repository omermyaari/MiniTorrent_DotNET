using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using TorrentWcfServiceLibrary;

namespace PeerUI.Communication {
    public class WCFClient {
        private User user;
        private ITorrentWcfService proxy;
        private ChannelFactory<ITorrentWcfService> factory;
        private XmlSerializer xmlSerializer = new XmlSerializer(typeof(ServiceMessage));

        public WCFClient(User user) {
            this.user = user;
            CreateConnection();
        }

        //  Creates a connection to the main server.
        private void CreateConnection() {
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

        public void UpdateConfig(User user) {
            this.user = user;
            CloseConnection();
            CreateConnection();
        }

        //  Closes the connection to the main server.
        public void CloseConnection() {
            GenerateSignOutRequest();
            factory.Close();
        }

        //  Generates a xml file request to send to the main server.
        private ServiceMessage GenerateFileRequest(string FileName) {
            /*XDocument xmlFileRequest = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement("FileRequest",
            new XElement("Username", user.Username),
            new XElement("Password", user.Password),
            new XElement("File", new XAttribute("Name", FileName))));
            return xmlFileRequest.ToString();*/
            ServiceMessage serviceMessage = new ServiceMessage();
            serviceMessage.Header = MessageHeader.FileRequest;
            serviceMessage.UserName = user.Name;
            serviceMessage.UserPassword = user.Password;
            serviceMessage.FilesList = new List<ServiceDataFile> {
                new ServiceDataFile {
                    Name = FileName
                }
            };
            return serviceMessage;
        }

        //  Generates a xml sign in request to send to the main server.
        private ServiceMessage GenerateSignInRequest() {
            var directoryInfo = new DirectoryInfo(user.SharedFolderPath);
            var sharedFilesInfo = directoryInfo.GetFiles("*");
            var serviceMessage = new ServiceMessage();
            serviceMessage.Header = MessageHeader.UserSignIn;
            serviceMessage.UserName = user.Name;
            serviceMessage.UserPassword = user.Password;
            serviceMessage.UserIP = user.UserIP;
            serviceMessage.UserPort = user.LocalPort;
            serviceMessage.FilesList = new List<ServiceDataFile>();
            foreach (FileInfo fi in sharedFilesInfo) {
                ServiceDataFile tempDataFile = new ServiceDataFile {
                    Name = fi.Name,
                    Size = fi.Length
                };
                serviceMessage.FilesList.Add(tempDataFile);
            }

            return serviceMessage;
            /*
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
            return xmlSignInRequest.ToString();*/
        }
        //  Generates a xml sign out request to send to the main server.
        public ServiceMessage GenerateSignOutRequest() {
            /*XDocument xmlSignOutRequest = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement("SignOutRequest",
            new XElement("Username", user.Username),
            new XElement("Password", user.Password)
                ));
            return xmlSignOutRequest.ToString();
            */
            var serviceMessage = new ServiceMessage();
            serviceMessage.Header = MessageHeader.UserSignOut;
            serviceMessage.UserName = user.Name;
            serviceMessage.UserPassword = user.Password;
            return serviceMessage;
        }

        //  Generates a file request from the user and sends it to the server,
        //  then returns the result to the user.
        public List<ServiceDataFile> FileRequest(string FileName) {
            var serviceMessage = GenerateFileRequest(FileName);
            var xmlMessage = SerializeMessage(serviceMessage);
            xmlMessage = proxy.Request(xmlMessage);
            serviceMessage = DeSerializeMessage(xmlMessage);
            return serviceMessage.FilesList;
        }

        //  Generates a sign in request from the user and sends it to the server,
        //  then returns if the user signed in successfuly.
        public MessageHeader SignIn() {
            var serviceMessage = GenerateSignInRequest();
            var xmlMessage = SerializeMessage(serviceMessage);
            xmlMessage = proxy.Request(xmlMessage);
            serviceMessage = DeSerializeMessage(xmlMessage);
            return serviceMessage.Header;
        }

        //  Generates a sign out request from the user and sends it to the server.
        public void SignOut() {
            var serviceMessage = GenerateSignOutRequest();
            var xmlMessage = SerializeMessage(serviceMessage);
            proxy.Request(xmlMessage);
        }

        //  DeSerializes string messages received from the main server.
        private ServiceMessage DeSerializeMessage(string message) {
            ServiceMessage serviceMessage;
            using (TextReader reader = new StringReader(message)) {
                serviceMessage = (ServiceMessage)xmlSerializer.Deserialize(reader);
            }
            return serviceMessage;
        }

        //  Serializes ServiceMessages to be sent to the main server.
        private string SerializeMessage(ServiceMessage serviceMessage) {
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb)) {
                xmlSerializer.Serialize(writer, serviceMessage);
            }
            return sb.ToString();
        }
    }
}
