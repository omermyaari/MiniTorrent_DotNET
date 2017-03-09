using System;
using System.ServiceModel;
using TorrentWcfServiceLibrary;

namespace WCFServer {

    /// <summary>
    /// WCF server project, handles new connections using the TorrentWcfServiceLibrary (contract and service).
    /// </summary>
    public class WcfServer {

        private static ServiceHost server;

        //  Starts listening for new requests.
        private static void StartService() {
            server = new ServiceHost((typeof(TorrentWcfService)));
            server.Open();
        }

        //  Stops listening.
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
