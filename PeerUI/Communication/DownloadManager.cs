using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeerUI
{
    class DownloadManager
    {
        Socket client = null;
        DataFile file; //the Array of users is inside
        private string DownloadFolder { set; get; }
        private static ManualResetEvent connectDone = new ManualResetEvent(false);


        public DownloadManager(DataFile file, string folder)
        {
            this.file = file;
            DownloadFolder = folder;

            DownloadFile();
        }

        private void DownloadFile()
        {
            long totalSize = file.FileSize;
            long segmentSize = file.FileSize / file.UsersList.Count();

            for (int i = 0; i < file.UsersList.Count(); i++)
            {
                //open threads to download segments ()
                startDownloading(new Segment(file.FileName, segmentSize * i, segmentSize), file.UsersList[i]);
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState; // Retrieve the socket from the state object.
                client.EndConnect(ar);  // Complete the connection.
                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());
                connectDone.Set();// Signal that the connection has been made.
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void startDownloading(Segment segment, User user)
        {
            try
            {
                IPAddress ipAddress = IPAddress.Parse(user.UserIP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, user.LocalPort);

                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);      // Connect to the remote endpoint.
                bool success = connectDone.WaitOne(5000, false);
                Console.WriteLine("result: " + success);
                GetSegment(segment, client);
            }
            catch (Exception error)
            {
                Console.WriteLine("catch block: " + error.Message);
            }
            finally
            {
                Console.WriteLine("Press Enter");
                if (client != null)
                    client.Close();
                Console.ReadLine();

            }
        }



        private void GetSegment(Segment segment, Socket client)
        {
            throw new NotImplementedException();
        }
    }
}
