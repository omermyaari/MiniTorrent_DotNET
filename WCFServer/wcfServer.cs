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
            /*using (ServiceHost host = new ServiceHost(typeof(TorrentWcfService), 
                new Uri[] { new Uri("http://10.7.107.3:8089/Wcf/") })) {
                host.AddServiceEndpoint(typeof(ITorrentWcfService), new BasicHttpBinding(), "TorrentWcfService");
                host.Open();
                Console.WriteLine("Server started");
                Console.ReadLine();
            }*/

            StartService();
            Console.WriteLine("Server started!");
            Console.ReadLine();
            StopService();
        }
    }
}
