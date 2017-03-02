using System;
using System.ServiceModel;
using TorrentWcfServiceLibrary;

namespace WCFServer {
    public class WcfServer {

        private static ServiceHost server;

        private static void StartService() {
            server = new ServiceHost((typeof(TorrentWcfService)));
            server.Open();
        }

        private static void StopService() {
            server.Close();
        }
        public static void Main(string[] args) {
            StartService();
            Console.WriteLine("Server started!");
            Console.ReadLine();
            StopService();
        }
    }
}
