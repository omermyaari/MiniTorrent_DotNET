using PeerUI.Entities;
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

namespace PeerUI {
    public class DownloadManager {
        public string DownloadFolder {
            set; get;
        }

        public static event TransferProgressDelegate transferProgressEvent;
        private static Stopwatch stopWatch = new Stopwatch();
        private long totalReceived = 0;
        private ServiceDataFile serviceDataFile; 
        private FileStream fileStream;
        private ManualResetEvent connectDone = new ManualResetEvent(false);
        private AutoResetEvent[] downloadDone;
        private int transferId;
        public bool stopDownloading = false;

        public DownloadManager(ServiceDataFile serviceDataFile, string folder, TransferProgressDelegate progressDelegate) {
            try {
                this.serviceDataFile = serviceDataFile;
                MainWindow.stopDownloadingEvent += stopDownloadingHandler;
                DownloadFolder = folder;
                downloadDone = new AutoResetEvent[serviceDataFile.PeerList.Count];
                using (fileStream = new FileStream(folder + "\\" + serviceDataFile.Name, FileMode.Create, FileAccess.Write)) {
                    transferProgressEvent += progressDelegate;
                    transferId = (new Random(DateTime.Now.Second)).Next();
                    DownloadFile();
                }

            }
            //  IOException to catch if the fileStream cannot open the file for writing.
            catch (IOException ioException) {
                Console.WriteLine("Socket exception at DownloadManager function (DownloadManager): " + ioException.Message);
                fileStream.Close();
            }

        }

        //  Starts the downloading process.
        private void DownloadFile()  {
            long segmentSize = serviceDataFile.Size / serviceDataFile.PeerList.Count;
            long segmentSizeMod = serviceDataFile.Size % serviceDataFile.PeerList.Count;
            stopWatch.Start();
            for (int i = 0; i < serviceDataFile.PeerList.Count; i++) {
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
            stopWatch.Reset();
            totalReceived = 0;
        }

        //  Starts the segment downloading thread.
        private void startDownloading(Segment segment, PeerAddress peerAddress) {
            Socket clientSocket = null;
            try {
                IPAddress ipAddress = IPAddress.Parse(peerAddress.Ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, peerAddress.Port);
                using (clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                    clientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), clientSocket);      // Connect to the remote endpoint.
                    bool success = connectDone.WaitOne(5000, false);
                    SendFileInfo(segment, clientSocket);
                    GetFileSegment(segment, clientSocket);
                }
            }
            catch (SocketException socketException) {
                Console.WriteLine("Socket exception at startDownloading function (DownloadManager): " + socketException.Message);
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                Thread.Yield();
            }
        }

        //  Checks if the socket is connected.
        private void IsSocketConnected(Socket s, ref bool socketConnected) {
            Thread.Sleep(200);
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                socketConnected = false;
            else
                socketConnected = true;
        }

        //  Connects to the uploading peer.
        private void ConnectCallback(IAsyncResult ar) {
            try {
                Socket client = (Socket)ar.AsyncState; // Retrieve the socket from the state object.
                client.EndConnect(ar);  // Complete the connection.
                //MessageBox.Show("Socket connected to " + client.RemoteEndPoint.ToString());
                connectDone.Set();// Signal that the connection has been made.
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
            }
        }

        //  Sends the requested segment info to the relevant peer.
        private void SendFileInfo(Segment segment, Socket clientSocket) {
            bool socketConnected = false;
            while (!socketConnected)
                IsSocketConnected(clientSocket, ref socketConnected);
            using (NetworkStream nfs = new NetworkStream(clientSocket)) {
                StreamWriter streamWriter = new StreamWriter(nfs);
                try {
                    streamWriter.WriteLine(segment.FileName + "#" + segment.StartPosition + "#" + segment.Size);
                    streamWriter.Flush();
                    //Console.WriteLine("File INFO sent to server successfully !\n\n");
                }
                catch (IOException ioException) {
                    Console.WriteLine("IO exception at SendFileInfo function (DownloadManager): " + ioException.Message);
                    nfs.Close();
            }
            }
        }

        //  Downloads the file segment.
        private void GetFileSegment(Segment segment, Socket clientSocket) {
            bool socketConnected = false;
            while (!socketConnected)
                IsSocketConnected(clientSocket, ref socketConnected);
            using (NetworkStream nfs = new NetworkStream(clientSocket)) {

                //int memoryStreamCapacity = (4 * 1024) ^ 2;
                //FileStream fileStream = new FileStream(DownloadFolder + @"\Fuck.txt", FileMode.Create, FileAccess.Write);
                //MemoryStream memoryStream = new MemoryStream(memoryStreamCapacity);
                //long size=fi.Length ;
                int bytesReceived = 1;  //  Current batch of bytes received.
                long totalReceived = 0; //  Total bytes received so far.
                                        //long totalReadInMemory = 0;
                byte[] buffer = new byte[1024 * 64];
                try {
                    while (totalReceived < segment.Size && !stopDownloading) {
                        Array.Clear(buffer, 0, buffer.Length);
                        //Read from the Network Stream
                        bytesReceived = nfs.Read(buffer, 0, buffer.Length);
                        //memoryStream.Write(buffer, 0, (int)bytesReceived);
                        //totalReadInMemory += bytesReceived;
                        totalReceived = totalReceived + bytesReceived;
                        WriteToDisk2(buffer, segment.StartPosition + totalReceived - bytesReceived, bytesReceived);
                        UpdateDownloadProgress(bytesReceived, transferId);
                        //if (totalReceived == segment.Size)
                        //    WriteToDisk(memoryStream, segment.StartPosition);
                        //else if (totalReadInMemory >= memoryStreamCapacity) {
                        //    WriteToDisk(memoryStream, segment.StartPosition + totalReceived - bytesReceived);
                        //    totalReadInMemory = 0;
                        // }
                        //Console.WriteLine("wrote: " + totalReceived + "from server " + segment.Id);
                    }
                    downloadDone[segment.Id].Set();
                    //Console.WriteLine("file segment received successfully !");
                }
                catch (IOException ioException) {
                    Console.WriteLine("IO exception at GetFileSegment function (DownloadManager): " + ioException.Message);
                    stopDownloading = true;
                    nfs.Close();
                    fileStream.Close();
                    downloadDone[segment.Id].Set();
                }
                catch (ObjectDisposedException objectDisposedException) {
                    Console.WriteLine("Object FileStream was used but has been disposed at function WriteToDisk (DownloadManager): " + objectDisposedException.Message);
                    stopDownloading = true;
                    nfs.Close();
                    downloadDone[segment.Id].Set();
                }
            }
        }

        //  Writes the data to the disk when the memory stream has been filled,
        //  or the file has been downloaded fully
        /*private void WriteToDisk(MemoryStream memoryStream, long position) {
            lock (fileStream) {
                if (fileStream != null && fileStream.CanWrite) {
                    fileStream.Seek(position, SeekOrigin.Begin);
                    memoryStream.WriteTo(fileStream);
                    //transferProgressEvent(serviceDataFile.Name, serviceDataFile.Size, position, stopWatch.ElapsedMilliseconds);
                }
            }
        }
        */
        private void WriteToDisk2(byte[] buffer, long position, int amount) {
            try {
                lock (fileStream) {
                    if (fileStream != null && fileStream.CanWrite) {
                        fileStream.Seek(position, SeekOrigin.Begin);
                        fileStream.Write(buffer, 0, amount);
                    }
                }
            }
            catch (Exception e) {
                throw e;
            }
        }

        //  Updates the GUI with the progress of the download.
        private void UpdateDownloadProgress(long bytesReceived, int transferId) {
            lock (this) {
                totalReceived += bytesReceived;
                transferProgressEvent(transferId, serviceDataFile.Name, serviceDataFile.Size, totalReceived, stopWatch.ElapsedMilliseconds, TransferType.Download);
            }
        }

        private void stopDownloadingHandler() {
            stopDownloading = true;
        }
    }
}