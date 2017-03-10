using PeerUI.Entities;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TorrentWcfServiceLibrary;

namespace PeerUI {

    /// <summary>
    /// This class is responsible of downloading files from other peers.
    /// </summary>
    public class DownloadManager {

        //  The folder that the manager will save the downloads to.
        public string DownloadFolder {
            set; get;
        }

        //  Stopwatch used to measure the time it takes to download the file.
        private static Stopwatch stopWatch = new Stopwatch();
        //  Total bytes received from all peers the client is downloading from.
        private long totalReceived = 0;
        //  Holds the file information and the peers (ip address and port) who share it.
        private ServiceDataFile serviceDataFile; 
        //  The file stream that saves the file to the download folder.
        private FileStream fileStream;
        //  AutoResetEvent used by the "main" DownloadManager thread,
        //  to determine if the download has finished.
        private AutoResetEvent[] downloadDone;
        //  Transfer id, used to distinguish between transfers in the progress list (library tab in ui).
        private int transferId;
        //  Boolean used when an exception has occured or the user wishes to end the downloading process.
        public bool stopDownloading = false;
        //  WcfMessageDelegate event used to notify the UI of errors and updates.
        public event WcfMessageDelegate wcfMessageEvent;
        //  Event delegate used by the DownloadManager to update the UI of the download progress.
        public static event TransferProgressDelegate transferProgressEvent;

        public DownloadManager(ServiceDataFile serviceDataFile, string folder, 
            TransferProgressDelegate progressDelegate, WcfMessageDelegate messageDelegate) {
            try {
                this.serviceDataFile = serviceDataFile;
                MainWindow.stopDownloadingEvent += stopDownloadingHandler;
                wcfMessageEvent += messageDelegate;
                Random random = new Random(DateTime.Now.Second);
                DownloadFolder = folder;
                transferProgressEvent += progressDelegate;
                transferId = random.Next();
                downloadDone = new AutoResetEvent[serviceDataFile.PeerList.Count];
                //  If the file already exists
                if (!File.Exists(folder + "\\" + serviceDataFile.Name))
                    using (fileStream = new FileStream(folder + "\\" + serviceDataFile.Name, FileMode.Create, FileAccess.Write)) {
                        DownloadFile();
                    }
                //  Create a new file with another name.
                else
                    using (fileStream = new FileStream(folder + "\\" + serviceDataFile.Name + random.Next(10000), FileMode.Create, FileAccess.Write)) {
                        DownloadFile();
                    }
            }

            //  IOException to catch if the fileStream cannot open the file for writing.
            catch (IOException ioException) {
                wcfMessageEvent(true, Properties.Resources.errorDLManager1 + ioException.Message);
                //fileStream.Close();
            }
        }

        /// <summary>
        /// Starts the download process.
        /// </summary>
        private void DownloadFile()  {
            //  Divides the file size and sets the segment size.
            long segmentSize = serviceDataFile.Size / serviceDataFile.PeerList.Count;
            //  Retrieves the remainder of the division (if there is any).
            long segmentSizeMod = serviceDataFile.Size % serviceDataFile.PeerList.Count;
            stopWatch.Start();
            //  Starts the downloading threads, each thread downloads a segment.
            for (int i = 0; i < serviceDataFile.PeerList.Count; i++) {
                downloadDone[i] = new AutoResetEvent(false);
                int j = i;
                Thread downloadingThread;
                //  If the users count doesnt divide file size evenly, let the last downloading thread get the remainder.
                if (j == serviceDataFile.PeerList.Count - 1 && (serviceDataFile.Size % serviceDataFile.PeerList.Count != 0))
                    downloadingThread = new Thread(() => StartDownloading(new Segment(j, serviceDataFile.Name, segmentSize * j, segmentSize + segmentSizeMod), serviceDataFile.PeerList[j]));
                else
                    downloadingThread =  new Thread(()=> StartDownloading(new Segment(j, serviceDataFile.Name, segmentSize * j, segmentSize), serviceDataFile.PeerList[j]));
                downloadingThread.Start();
            }
            //  Waits for all the downloading threads to complete.
            WaitHandle.WaitAll(downloadDone);
            stopWatch.Stop();
            stopWatch.Reset();
            totalReceived = 0;
        }

        /// <summary>
        /// Starts a segment downloading thread.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="peerAddress"></param>
        private void StartDownloading(Segment segment, PeerAddress peerAddress) {
            Socket clientSocket = null;
            try {
                IPAddress ipAddress = IPAddress.Parse(peerAddress.Ip);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, peerAddress.Port);
                using (clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                    // Connect to the remote endpoint.
                    clientSocket.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), clientSocket);
                    //  Sends the uploading peer the requested file info.
                    SendFileInfo(segment, clientSocket);
                    //  Begins downloading the segment.
                    GetFileSegment(segment, clientSocket);
                }
            }

            catch (SocketException socketException) {
                wcfMessageEvent(true, Properties.Resources.errorDLManager2 + socketException.Message);
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                Thread.Yield();
            }
        }

        /// <summary>
        /// Checks if the socket is connected.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="socketConnected"></param>
        private void IsSocketConnected(Socket s, ref bool socketConnected) {
            Thread.Sleep(200);
            bool part1 = s.Poll(1000, SelectMode.SelectRead);
            bool part2 = (s.Available == 0);
            if (part1 && part2)
                socketConnected = false;
            else
                socketConnected = true;
        }

        /// <summary>
        /// Connects to the uploading peer.
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;
                // Complete the connection.
                client.EndConnect(ar);  
            }
            catch (Exception e) {
                wcfMessageEvent(true, e.ToString());
            }
        }

        /// <summary>
        /// Sends the requested segment info to the relevant uploading peer.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="clientSocket"></param>
        private void SendFileInfo(Segment segment, Socket clientSocket) {
            bool socketConnected = false;
            while (!socketConnected)
                IsSocketConnected(clientSocket, ref socketConnected);
            NetworkStream nfs = null;

            try {
                using (nfs = new NetworkStream(clientSocket)) {
                    StreamWriter streamWriter = new StreamWriter(nfs);
                    streamWriter.WriteLine(segment.FileName + "#" + segment.StartPosition + "#" + segment.Size);
                    streamWriter.Flush();
                }
            }

            catch (IOException ioException) {
                wcfMessageEvent(true, Properties.Resources.errorDLManager3 + ioException.Message);
                if (nfs != null)
                    nfs.Close();
            }
        }
        //
        /// <summary>
        /// Downloads the file segment.
        /// </summary>
        /// <param name="segment"></param>
        /// <param name="clientSocket"></param>
        private void GetFileSegment(Segment segment, Socket clientSocket) {
            bool socketConnected = false;
            while (!socketConnected)
                IsSocketConnected(clientSocket, ref socketConnected);
            NetworkStream nfs = null;

            //  Current batch of bytes received.
            int bytesReceived = 1;
            //  Total bytes (of segment) received so far.
            long segmentTotalReceived = 0;
            byte[] buffer = new byte[1024 * 64];
            try {
                using (nfs = new NetworkStream(clientSocket)) {
                    //  Stop downloading if stopDownloading bool set to true or the segment download has completed.
                    while (segmentTotalReceived < segment.Size && !stopDownloading) {
                        Array.Clear(buffer, 0, buffer.Length);
                        //  Read 64K bytes from the Network Stream
                        bytesReceived = nfs.Read(buffer, 0, buffer.Length);
                        //  Update the total amount of bytes received for this specific segment.
                        segmentTotalReceived = segmentTotalReceived + bytesReceived;
                        //  Write the buffer to the file stream.
                        WriteToDisk(buffer, segment.StartPosition + segmentTotalReceived - bytesReceived, bytesReceived);
                        //  Update the segment downloading progress.
                        UpdateDownloadProgress(bytesReceived, transferId);
                    }
                    //  When the segment downloading has finished, update the reset event.
                }
                downloadDone[segment.Id].Set();
            }
            catch (IOException ioException) {
                wcfMessageEvent(true, Properties.Resources.errorDLManager4 + ioException.Message);
                stopDownloading = true;
                if (nfs != null)
                    nfs.Close();
                if (fileStream != null)
                    fileStream.Close();
                downloadDone[segment.Id].Set();
            }
            catch (ObjectDisposedException objectDisposedException) {
                wcfMessageEvent(true, Properties.Resources.errorDLManager5 + objectDisposedException.Message);
                stopDownloading = true;
                if (nfs != null)
                    nfs.Close();
                downloadDone[segment.Id].Set();
            }
        }

        /// <summary>
        /// Writes buffer to the file stream at the given position.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="position"></param>
        /// <param name="amount"></param>
        private void WriteToDisk(byte[] buffer, long position, int amount) {
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

        /// <summary>
        /// Updates the GUI with the progress of the download.
        /// </summary>
        /// <param name="bytesReceived"></param>
        /// <param name="transferId"></param>
        private void UpdateDownloadProgress(long bytesReceived, int transferId) {
            lock (this) {
                totalReceived += bytesReceived;
                transferProgressEvent(transferId, serviceDataFile.Name, serviceDataFile.Size, totalReceived, stopWatch.ElapsedMilliseconds, TransferType.Download);
            }
        }

        /// <summary>
        /// Used by the UI to stop all downloading threads.
        /// </summary>
        private void stopDownloadingHandler() {
            stopDownloading = true;
        }
    }
}