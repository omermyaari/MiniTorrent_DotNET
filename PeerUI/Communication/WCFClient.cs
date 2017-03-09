using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Xml.Serialization;
using TorrentWcfServiceLibrary;

namespace PeerUI.Communication {

    /// <summary>
    /// This is the WCF client, used by the program to send and receive messages to and from the WCF main server.
    /// </summary>
    public class WCFClient {
        //  Holds the current user settings.
        private User user;
        //  Proxy used to send requests to the WCF main server.
        private ITorrentWcfService proxy;
        private ChannelFactory<ITorrentWcfService> factory;
        //  XML serializer used to serialize and deserialize service messages.
        private XmlSerializer xmlSerializer = new XmlSerializer(typeof(ServiceMessage));
        //  Boolean used to detect whether the user has signed in to the WCF server.
        public bool userConnected = false;
        //  WcfMessageDelegate event used to notify the UI of errors and updates.
        public event WcfMessageDelegate WcfMessageEvent;

        /// <summary>
        /// Creates a connection to the WCF server.
        /// </summary>
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

        /// <summary>
        /// Retrieves the local machine's IP address.
        /// </summary>
        /// <returns>string</returns>
        private string GetLocalIp() {
            var localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress i in localIPs) {
                if (i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                    && !IPAddress.IsLoopback(i)) {
                    return i.ToString();
                }
            }
            return Properties.Resources.defaultIP;
        }

        /// <summary>
        /// Updates the configuration file.
        /// </summary>
        /// <param name="user"></param>
        public void UpdateConfig(User user) {
            if (userConnected) {
                CloseConnection();
                userConnected = false;
            }
            this.user = user;
            user.UserIP = GetLocalIp();
            CreateConnection();
            SignIn();
        }

        /// <summary>
        /// Closes the connection to the main server.
        /// </summary>
        public void CloseConnection() {
            SignOut();
            factory.Close();
        }

        /// <summary>
        /// Generates a xml file request to send to the main server.
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns>ServiceMessage</returns>
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

        /// <summary>
        /// Generates a xml sign in request to send to the main server.
        /// </summary>
        /// <returns>ServiceMessage</returns>
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
                WcfMessageEvent(true, Properties.Resources.errorDirectoryNotFound);
            }
            return null;
        }

        /// <summary>
        /// Generates a xml sign out request to send to the main server.
        /// </summary>
        /// <returns>ServiceMessage</returns>
        public ServiceMessage GenerateSignOutRequest() {
            var serviceMessage = new ServiceMessage();
            serviceMessage.Header = MessageHeader.UserSignOut;
            serviceMessage.UserName = user.Name;
            serviceMessage.UserPassword = user.Password;
            serviceMessage.UserIP = user.UserIP;
            serviceMessage.UserPort = user.LocalPort;
            return serviceMessage;
        }

        /// <summary>
        /// Generates a file request from the user and sends it to the server,
        /// then returns the result to the user.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>List</returns>
        public List<ServiceDataFile> FileRequest(string fileName) {
            if (String.IsNullOrEmpty(fileName)) {
                WcfMessageEvent(true, Properties.Resources.errorFileNameEmpty);
                return null;
            }
            //  Generate the file request xml message.
            var serviceMessage = GenerateFileRequest(fileName);
            //  Serialize the xml message.
            var xmlMessage = SerializeMessage(serviceMessage);
            try {
                //  Send the request to the WCF server.
                xmlMessage = proxy.Request(xmlMessage);
            }
            catch (EndpointNotFoundException) {
                WcfMessageEvent(true, Properties.Resources.errorEndpointNotFound);
                return null;
            }
            catch (TimeoutException) {
                WcfMessageEvent(true, Properties.Resources.errorFileRequestTimeout);
                return null;
            }
            //  DeSerialize the message received from the server.
            serviceMessage = DeSerializeMessage(xmlMessage);
            //  If the main server has returned an empty list of files, notify the user.
            if (serviceMessage.Header == MessageHeader.ConnectionFailed) {
                WcfMessageEvent(true, Properties.Resources.errorUserNotSigned);
                return null;
            }
            if (serviceMessage.FilesList.Count == 0) {
                WcfMessageEvent(true, "Error: File " + fileName + " was not found.");
                return null;
            }
            WcfMessageEvent(false, Properties.Resources.connectedString);
            return serviceMessage.FilesList;
        }

        /// <summary>
        /// Generates a sign in request from the user and sends it to the server,
        /// then returns if the user signed in successfuly.
        /// </summary>
        private void SignIn() {
            //  Generate the sign in xml message.
            var serviceMessage = GenerateSignInRequest();
            if (serviceMessage == null) {
                WcfMessageEvent(true, Properties.Resources.errorGenerateSignIn);
                return;
            }
            //  Serialize the xml message.
            var xmlMessage = SerializeMessage(serviceMessage);
            try {
                //  Send the request to the WCF server.
                xmlMessage = proxy.Request(xmlMessage);
            }
            catch (EndpointNotFoundException) {
                WcfMessageEvent(true, Properties.Resources.errorEndpointNotFound);
                return;
            }
            catch (TimeoutException) {
                WcfMessageEvent(true, Properties.Resources.errorSignInTimeout);
                return;
            }
            //  DeSerialize the message received from the server.
            serviceMessage = DeSerializeMessage(xmlMessage);
            if (serviceMessage.Header == MessageHeader.ConnectionSuccessful) {
                userConnected = true;
                WcfMessageEvent(false, Properties.Resources.connectedString);
                return;
            }
            else {
                WcfMessageEvent(true, Properties.Resources.errorUsernamePassword);
            }

        }

        /// <summary>
        /// Generates a sign out request from the user and sends it to the server.
        /// </summary>
        public void SignOut() {
            //  Generate the sign out xml message.
            var serviceMessage = GenerateSignOutRequest();
            //  Serialize the xml message.
            var xmlMessage = SerializeMessage(serviceMessage);
            try {
                //  Send the request to the WCF server.
                proxy.Request(xmlMessage);
            }
            catch (EndpointNotFoundException) {
                WcfMessageEvent(true, Properties.Resources.errorEndpointNotFound);
            }
            catch (TimeoutException) {
                WcfMessageEvent(true, Properties.Resources.errorSignInTimeout);
            }
            userConnected = false;
        }

        /// <summary>
        /// DeSerializes string messages received from the main server.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>ServiceMessage</returns>
        private ServiceMessage DeSerializeMessage(string message) {
            ServiceMessage serviceMessage;
            using (TextReader reader = new StringReader(message)) {
                serviceMessage = (ServiceMessage)xmlSerializer.Deserialize(reader);
            }
            return serviceMessage;
        }

        /// <summary>
        /// Serializes ServiceMessages to be sent to the main server.
        /// </summary>
        /// <param name="serviceMessage"></param>
        /// <returns>ServiceMessage</returns>
        private string SerializeMessage(ServiceMessage serviceMessage) {
            var sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb)) {
                xmlSerializer.Serialize(writer, serviceMessage);
            }
            return sb.ToString();
        }
    }
}