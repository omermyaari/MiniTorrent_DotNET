using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PeerUI
{
    public class DownloadManager
    {
        DataFile file; //the Array of users is inside

        byte[] fileResult;

        private string DownloadFolder { set; get; }
        private static ManualResetEvent connectDone = new ManualResetEvent(false);


        public DownloadManager(DataFile file, string folder)
        {
            this.file = file;
            DownloadFolder = folder;

            fileResult = new byte[file.FileSize];

            DownloadFile();
        }

        private void DownloadFile()
        {
            //TODO check amount of segments!!!
            long segmentSize = file.FileSize / file.UsersList.Count;

           // long segmentCount = file.FileSize / segmentSize;
           // if (file.FileSize % segmentSize > 0) segmentCount++;

            for (int i = 0; i <= file.UsersList.Count; i++) 
            {
                //open threads to download segments ()
                Thread downloadingThread =  new Thread(()=> startDownloading(new Segment(file.FileName, segmentSize * i, segmentSize), file.UsersList[i]));
                downloadingThread.Start();
            }
        }

        private void startDownloading(Segment segment, User user)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            NetworkStream nfs = new NetworkStream(clientSocket);
            try
            {
                IPAddress ipAddress = IPAddress.Parse(user.UserIP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, user.LocalPort);
                clientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), clientSocket);      // Connect to the remote endpoint.
                bool success = connectDone.WaitOne(5000, false);
                MessageBox.Show("result: " + success);
                SendFileInfo(segment, nfs);
                GetSegment(segment, clientSocket);
            }
            catch (Exception error)
            {
                MessageBox.Show("catch block: " + error.Message);
            }
            finally
            {
                if (clientSocket != null)
                    clientSocket.Close();
                if (nfs != null)
                    nfs.Close();
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState; // Retrieve the socket from the state object.
                client.EndConnect(ar);  // Complete the connection.
                MessageBox.Show("Socket connected to " + client.RemoteEndPoint.ToString());
                connectDone.Set();// Signal that the connection has been made.
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
        }

        //  Sends the requested segment info to the relevant peer.
        private void SendFileInfo(Segment segment, NetworkStream nfs) {
            StreamWriter streamWriter = new StreamWriter(nfs);
            try {
                streamWriter.AutoFlush = true;
                streamWriter.WriteLine(segment.FileName + "#" + segment.StartPosition + "#" + segment.Size);
                streamWriter.Flush();
                streamWriter.Close();
                Console.WriteLine("File INFO sent to server successfully !\n\n");
            }
            catch (Exception ed) {
                Console.WriteLine("A Exception occured in transfer in TESTER CLIENT" + ed.ToString());
            }
            finally {
                if (streamWriter != null)
                    streamWriter.Close();
            }
        }

        private void GetSegment(Segment segment, Socket clientSocket)
        {
            NetworkStream nfs = new NetworkStream(clientSocket);

            long i = 1;
            long rby = 0;
            try
            {
                //loop till the Full bytes have been read
                while (i < segment.Size)
                {
                    byte[] buffer = new byte[100];

                    //Read from the Network Stream
                    i = nfs.Read(buffer, 0, buffer.Length);

                    for (int j = 0; j < buffer.Length; j++)
                    {

                    }

                    rby = rby + i;
                    Console.WriteLine("wrote: " + rby);
                }

                Console.WriteLine("file received successfully !");
            }
            catch (Exception ed)
            {
                Console.WriteLine("A Exception occured in file transfer in Tester File Receiving!" + ed.Message);

            }
            finally
            {
              //  if (fout != null)
              //      fout.Close();
            }
        }
    }
}
