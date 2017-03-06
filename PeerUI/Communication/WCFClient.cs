using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Windows.Threading;
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
            user.UserIP = GetLocalIp();
            CreateConnection();
            SignIn();
        }

        //  Creates a connection to the main server.
        private void CreateConnection() {
                EndpointAddress ep = new EndpointAddress(@"http://" + user.ServerIP + ":"
       + user.ServerPort + @"/Wcf");
                factory = new ChannelFactory<ITorrentWcfService>(new BasicHttpBinding(), ep);
                proxy = factory.CreateChannel();
        }

        private string GetLocalIp() {
            var localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress i in localIPs) {
                if (i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    && !IPAddress.IsLoopback(i)) {
                    return i.ToString();
                }
            }
            return "192.168.0.0";
        }

        public void UpdateConfig(User user) {
            SignOut();
            this.user = user;
            user.UserIP = GetLocalIp();
            CloseConnection();
            CreateConnection();
            SignIn();
        }

        //  Closes the connection to the main server.
        public void CloseConnection() {
            SignOut();
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
            serviceMessage.UserIP = user.UserIP;
            serviceMessage.UserPort = user.LocalPort;
            serviceMessage.FilesList = new List<ServiceDataFile> {
                new ServiceDataFile {
                    Name = FileName
                }
            };
            return serviceMessage;
        }

        //  Generates a xml sign in request to send to the main server.
        private ServiceMessage GenerateSignInRequest() {
            try {
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
            }
            catch (Exception ex) {
                Console.WriteLine("Directory not found.");
            }
            return null;

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
            serviceMessage.UserIP = user.UserIP;
            serviceMessage.UserPort = user.LocalPort;
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
        private MessageHeader SignIn() {
            var serviceMessage = GenerateSignInRequest();
            if (serviceMessage == null) {
                return MessageHeader.ConnectionFailed;
            }

            var xmlMessage = SerializeMessage(serviceMessage);
            try {
                xmlMessage = proxy.Request(xmlMessage);
            }
            catch (EndpointNotFoundException ex) {
                Console.WriteLine("Couldnt connect to server: " + ex.Message);
                return MessageHeader.ConnectionFailed;
            }
            serviceMessage = DeSerializeMessage(xmlMessage);
            return serviceMessage.Header;
        }

        //  Generates a sign out request from the user and sends it to the server.
        public void SignOut() {
            var serviceMessage = GenerateSignOutRequest();
            var xmlMessage = SerializeMessage(serviceMessage);
            try {
                proxy.Request(xmlMessage);
            }
            catch (EndpointNotFoundException ex) {
                Console.WriteLine("Couldnt connect to server: " + ex.Message);
            }
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
