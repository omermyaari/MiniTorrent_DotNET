using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Xml.Serialization;
using TorrentWcfServiceLibrary;

namespace PeerUI.Communication {
    public class WCFClient {
        private User user;
        private ITorrentWcfService proxy;
        private ChannelFactory<ITorrentWcfService> factory;
        private XmlSerializer xmlSerializer = new XmlSerializer(typeof(ServiceMessage));
        private bool userConnected = false;
        public event WcfMessageDelegate WcfMessageEvent;


        //  Creates a connection to the main server.
        private void CreateConnection() {
                EndpointAddress ep = new EndpointAddress(@"http://" + user.ServerIP + ":"
       + user.ServerPort + @"/Wcf");
            factory = new ChannelFactory<ITorrentWcfService>(new BasicHttpBinding() {
                SendTimeout = TimeSpan.FromSeconds(10),
                ReceiveTimeout = TimeSpan.FromSeconds(10),
                OpenTimeout = TimeSpan.FromSeconds(10),
                CloseTimeout = TimeSpan.FromSeconds(10),
            }, ep);
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
            if (userConnected) {
                SignOut();
                CloseConnection();
            }
            this.user = user;
            user.UserIP = GetLocalIp();
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
            catch (Exception) {
                WcfMessageEvent(true, "Error: Directory not found.");
            }
            return null;
        }

        //  Generates a xml sign out request to send to the main server.
        public ServiceMessage GenerateSignOutRequest() {
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
        public List<ServiceDataFile> FileRequest(string fileName) {
            if (String.IsNullOrEmpty(fileName)) {
                WcfMessageEvent(true, "Error: File name cannot be empty.");
                return null;

            }
            var serviceMessage = GenerateFileRequest(fileName);
            var xmlMessage = SerializeMessage(serviceMessage);
            try {
                xmlMessage = proxy.Request(xmlMessage);
            }
            catch (EndpointNotFoundException) {
                WcfMessageEvent(true, "Error: Couldnt request file from main server - endpoint not found.");
                return null;
            }
            catch (TimeoutException) {
                WcfMessageEvent(true, "Error: Couldnt request file from main server - timeout.");
                return null;
            }
            serviceMessage = DeSerializeMessage(xmlMessage);
            //  If the main server has returned an empty list of files, notify the user.
            if (serviceMessage.FilesList.Count == 0) {
                WcfMessageEvent(true, "Error: File " + fileName + " was not found.");
                return null;
            }
            WcfMessageEvent(false, "Connected.");
            return serviceMessage.FilesList;
        }

        //  Generates a sign in request from the user and sends it to the server,
        //  then returns if the user signed in successfuly.
        private void SignIn() {
            var serviceMessage = GenerateSignInRequest();
            if (serviceMessage == null) {
                WcfMessageEvent(true, "Error: Couldn't generate a sign in request, check your settings.");
                return;
            }
            var xmlMessage = SerializeMessage(serviceMessage);
            try {
                xmlMessage = proxy.Request(xmlMessage);
            }
            catch (EndpointNotFoundException) {
                WcfMessageEvent(true, "Error: Couldnt sign in to main server - endpoint not found.");
                return;
            }
            catch (TimeoutException) {
                WcfMessageEvent(true, "Error: Couldn't sign in to main server - timeout.");
                return;
            }
            serviceMessage = DeSerializeMessage(xmlMessage);
            if (serviceMessage.Header == MessageHeader.ConnectionSuccessful) {
                userConnected = true;
                WcfMessageEvent(false, "Connected.");
                return;
            }
            else {
                WcfMessageEvent(true, "Error: Couldn't sign in to main server - username or password is incorrect.");
            }

        }

        //  Generates a sign out request from the user and sends it to the server.
        public void SignOut() {
            var serviceMessage = GenerateSignOutRequest();
            var xmlMessage = SerializeMessage(serviceMessage);
            try {
                proxy.Request(xmlMessage);
            }
            catch (EndpointNotFoundException) {
                WcfMessageEvent(true, "Error: Couldnt sign out of server: endpoint not found.");
            }
            catch (TimeoutException) {
                WcfMessageEvent(true, "Error: Couldn't sign in to main server - timeout.");
            }
            userConnected = false;
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