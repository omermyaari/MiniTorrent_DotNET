﻿using System;
using System.ComponentModel;

namespace PeerUI {
    
    /// <summary>
    /// User class, holds the user's settings and properties.
    /// </summary>
    [Serializable]
    public class User : IDataErrorInfo {

        public string UserIP
        {
            get; set;
        }

        public string Name {
            get; set;
        }
        
        public string Password {
            get; set;
        }

        public string ServerIP {
            get; set;
        }

        public int ServerPort {
            get; set;
        }

        public int LocalPort {
            get; set;
        }

        public string SharedFolderPath {
            get; set;
        }

        public string DownloadFolderPath {
            get; set;
        }

        public string Error {
            get {
                return null;
            }
        }

        public User() {

        }

        public User(string Name, string Password, string ServerIP, int ServerPort, 
            int LocalPort, string SharedFolderPath, string DownloadFolderPath) {
            this.Name = Name;
            this.Password = Password;
            this.ServerIP = ServerIP;
            this.ServerPort = ServerPort;
            this.LocalPort = LocalPort;
            this.SharedFolderPath = SharedFolderPath;
            this.DownloadFolderPath = DownloadFolderPath;
        }

        public string this[string name] {
            get {
                string result = null;

                //basically we need one of these blocks for each property you wish to validate
                switch (name) {
                    case "Name":
                        if (Name.Length <= 0 || Name.Length > 10) {
                            result = "Name must not be less than 0 or greater than 10.";
                        }
                        break;

                    case "Password":
                        if (Password.Length <= 0 || Password.Length > 10) {
                            result = "Password must not be less than 0 or greater than 150.";
                        }
                        break;

                    case "LocalPort":
                        if (LocalPort <= 0 || LocalPort > 65535) {
                            result = "Port must not be 0 or greater than 65535.";
                        }
                        break;

                    case "ServerPort":
                        if (ServerPort <= 0 || ServerPort > 65535) {
                            result = "Port must not be 0 or greater than 65535.";
                        }
                        break;
                }
                return result;
            }
        }
    }
}
