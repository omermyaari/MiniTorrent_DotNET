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
        public string DownloadFolder {
            set; get;
        }
        private DataFile dataFile; //the Array of users is inside
        private FileStream fileStream;
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private AutoResetEvent[] downloadDone;


        public DownloadManager(DataFile dataFile, string folder)
        {
            this.dataFile = dataFile;
            DownloadFolder = folder;
            downloadDone = new AutoResetEvent[dataFile.UsersList.Count];
            fileStream = new FileStream(folder + "\\" + dataFile.FileName, FileMode.Create, FileAccess.Write);
            DownloadFile();
        }

        private void DownloadFile()
        {
            //TODO check amount of segments!!!
            long segmentSize = dataFile.FileSize / dataFile.UsersList.Count;
            long segmentSizeMod = dataFile.FileSize % dataFile.UsersList.Count;
            // long segmentCount = file.FileSize / segmentSize;
            // if (file.FileSize % segmentSize > 0) segmentCount++;

            for (int i = 0; i < dataFile.UsersList.Count; i++) 
            {
                //open threads to download segments ()
                downloadDone[i] = new AutoResetEvent(false);
                //Segment seg = new Segment(i, dataFile.FileName, segmentSize * i, segmentSize);
                //Thread downloadingThread = new Thread(() => startDownloading(seg, dataFile.UsersList[i]));
                //startDownloading(seg, dataFile.UsersList[i]);
                int j = i;
                Thread downloadingThread;
                //  Check if the users count doesnt divide file size evenly, let the last uploading peer send the remainder.
                if (j == dataFile.UsersList.Count - 1 && (dataFile.FileSize % dataFile.UsersList.Count != 0))
                    downloadingThread = new Thread(() => startDownloading(new Segment(j, dataFile.FileName, segmentSize * j, segmentSize + segmentSizeMod), dataFile.UsersList[j]));
                else
                    downloadingThread =  new Thread(()=> startDownloading(new Segment(j, dataFile.FileName, segmentSize * j, segmentSize), dataFile.UsersList[j]));
                downloadingThread.Start();
            }
            WaitHandle.WaitAll(downloadDone);
            fileStream.Close();
        }

        private void startDownloading(Segment segment, User user)
        {
            Socket clientSocket = null;
            try
            {
                IPAddress ipAddress = IPAddress.Parse(user.UserIP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, user.LocalPort);
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), clientSocket);      // Connect to the remote endpoint.
                bool success = connectDone.WaitOne(5000, false);
                SendFileInfo(segment, clientSocket);
                GetFileSegment(segment, clientSocket);

            }
            catch (Exception error)
            {
                MessageBox.Show("catch block: " + error.Message);
            }
            finally
            {
                if (clientSocket != null) {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.Close();
                    Console.WriteLine("Socket closed at downloader.");
                }
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState; // Retrieve the socket from the state object.
                client.EndConnect(ar);  // Complete the connection.
                //MessageBox.Show("Socket connected to " + client.RemoteEndPoint.ToString());
                connectDone.Set();// Signal that the connection has been made.
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }
        }

        //  Sends the requested segment info to the relevant peer.
        private void SendFileInfo(Segment segment, Socket clientSocket) {
            NetworkStream nfs = new NetworkStream(clientSocket);
            StreamWriter streamWriter = new StreamWriter(nfs);
            try {
                streamWriter.AutoFlush = true;
                streamWriter.WriteLine(segment.FileName + "#" + segment.StartPosition + "#" + segment.Size);
                streamWriter.Flush();
                Console.WriteLine("File INFO sent to server successfully !\n\n");
            }
            catch (Exception ed) {
                Console.WriteLine("A Exception occured in transfer in TESTER CLIENT" + ed.ToString());
            }
        }

        private void GetFileSegment(Segment segment, Socket clientSocket) {
            NetworkStream nfs = new NetworkStream(clientSocket);
            int memoryStreamCapacity = (4 * 1024) ^ 2;
            //FileStream fileStream = new FileStream(DownloadFolder + @"\Fuck.txt", FileMode.Create, FileAccess.Write);
            MemoryStream memoryStream = new MemoryStream(memoryStreamCapacity);
            //long size=fi.Length ;
            int bytesReceived = 1;  //  Current batch of bytes received.
            long totalReceived = 0; //  Total bytes received so far.
            long totalReadInMemory = 0;
            try {
                //loop till the Full bytes have been read
                while (totalReceived < segment.Size) {
                    byte[] buffer = new byte[4 * 1024];
                    //Read from the Network Stream
                    bytesReceived = nfs.Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, (int)bytesReceived);
                    totalReadInMemory += bytesReceived;
                    totalReceived = totalReceived + bytesReceived;
                    if (totalReceived == segment.Size)
                        WriteToDisk(memoryStream, segment.StartPosition);
                    else if (totalReadInMemory == memoryStreamCapacity) {
                        WriteToDisk(memoryStream, segment.StartPosition + totalReceived - bytesReceived);
                        memoryStream.Flush();
                        totalReadInMemory = 0;
                    }
                    Console.WriteLine("wrote: " + totalReceived + "from server " + segment.Id);
                }
                downloadDone[segment.Id].Set();
                Console.WriteLine("file segment received successfully !");
            }
            catch (Exception ed) {
                Console.WriteLine("A Exception occured in file transfer in Tester File Receiving!" + ed.Message);
            }
            finally {
               if (memoryStream != null)
                    memoryStream.Close();
            }
        }

        private void WriteToDisk(MemoryStream memoryStream, long position) {
            lock (fileStream) {
                fileStream.Seek(position, 0);
                memoryStream.WriteTo(fileStream);
            }
        }
    }
}
