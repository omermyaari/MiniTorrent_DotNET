using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TorrentWcfServiceLibrary;

namespace PeerUI
{
    public class DownloadManager
    {
        public string DownloadFolder {
            set; get;
        }
        public static event TransferProgressDelegate transferProgressEvent;
        private static Stopwatch stopWatch = new Stopwatch();
        private ServiceDataFile serviceDataFile; 
        private FileStream fileStream;
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private AutoResetEvent[] downloadDone;
        private bool socketConnected = false;

        public DownloadManager(ServiceDataFile serviceDataFile, string folder, TransferProgressDelegate progressDelegate) {
            try {
                this.serviceDataFile = serviceDataFile;
                DownloadFolder = folder;
                downloadDone = new AutoResetEvent[serviceDataFile.PeerList.Count];
                fileStream = new FileStream(folder + "\\" + serviceDataFile.Name, FileMode.Create, FileAccess.Write);
                transferProgressEvent += progressDelegate;

                DownloadFile();
            }
            //  IOException to catch if the fileStream cannot open the file for writing.
            catch (IOException ioException) {
               // MessageBox.Show("Io exception was thrown, cannot open file stream at downloader.");
            }

        }

        //  Starts the downloading process.
        private void DownloadFile()
        {
            long segmentSize = serviceDataFile.Size / serviceDataFile.PeerList.Count;
            long segmentSizeMod = serviceDataFile.Size % serviceDataFile.PeerList.Count;
            stopWatch.Start();
            for (int i = 0; i < serviceDataFile.PeerList.Count; i++) 
            {
                downloadDone[i] = new AutoResetEvent(false);
                int j = i;
                Thread downloadingThread;
                //  Check if the users count doesnt divide file size evenly, let the last uploading peer send the remainder.
                if (j == serviceDataFile.PeerList.Count - 1 && (serviceDataFile.Size % serviceDataFile.PeerList.Count != 0))
                    downloadingThread = new Thread(() => startDownloading(new Segment(j, serviceDataFile.Name, segmentSize * j, segmentSize + segmentSizeMod), serviceDataFile.PeerList[j]));
                else
                    downloadingThread =  new Thread(()=> startDownloading(new Segment(j, serviceDataFile.Name, segmentSize * j, segmentSize), serviceDataFile.PeerList[j]));
                downloadingThread.Start();
            }
            WaitHandle.WaitAll(downloadDone);
            stopWatch.Stop();
            fileStream.Close();
        }

        //  Starts the segment downloading thread.
        private void startDownloading(Segment segment, PeerAddress peerAddress)
        {
            Socket clientSocket = null;
            try
            {
                IPAddress ipAddress = IPAddress.Parse(peerAddress.Ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, peerAddress.Port);
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

        //  Checks if the socket is connected.
        private void IsSocketConnected(Socket s) {
            Thread.Sleep(200);
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                socketConnected = false;
            else
                socketConnected = true;
        }

        //  Connects to the uploading peer.
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
            socketConnected = false;
            while (!socketConnected)
                IsSocketConnected(clientSocket);
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

        //  Downloads the file segment.
        private void GetFileSegment(Segment segment, Socket clientSocket) {
            socketConnected = false;
            while (!socketConnected)
                IsSocketConnected(clientSocket);
            NetworkStream nfs = new NetworkStream(clientSocket);
            int memoryStreamCapacity = (4 * 1024) * 10;
            //FileStream fileStream = new FileStream(DownloadFolder + @"\Fuck.txt", FileMode.Create, FileAccess.Write);
            MemoryStream memoryStream = new MemoryStream(memoryStreamCapacity);
            //long size=fi.Length ;
            int bytesReceived = 1;  //  Current batch of bytes received.
            long totalReceived = 0; //  Total bytes received so far.
            long totalReadInMemory = 0;
            try {
                while (totalReceived < segment.Size) {
                    byte[] buffer = new byte[4 * 1024];
                    //Read from the Network Stream
                    bytesReceived = nfs.Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, (int)bytesReceived);
                    totalReadInMemory += bytesReceived;
                    totalReceived = totalReceived + bytesReceived;
                    if (totalReceived == segment.Size)
                        WriteToDisk(memoryStream, segment.StartPosition);
                    else if (totalReadInMemory >= memoryStreamCapacity) {
                        WriteToDisk(memoryStream, segment.StartPosition + totalReceived - bytesReceived);
                        memoryStream.Flush();
                        totalReadInMemory = 0;
                    }
                    //Console.WriteLine("wrote: " + totalReceived + "from server " + segment.Id);
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

        //  Writes the data to the disk when the memory stream has been filled,
        //  or the file has been downloaded fully
        private void WriteToDisk(MemoryStream memoryStream, long position) {
            lock (fileStream) {
                if (fileStream != null && fileStream.CanWrite) {
                    fileStream.Seek(position, 0);
                    memoryStream.WriteTo(fileStream);
                    transferProgressEvent(serviceDataFile.Name, serviceDataFile.Size, position, stopWatch.ElapsedMilliseconds);
                }
            }
        }
    }
}
