using PeerUI.Entities;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PeerUI.Communication;

namespace PeerUI {

    /// <summary>
    /// This class is responsible of upload files to other peers.
    /// </summary>
    public class UploadManager {

        //  Local port.
        private int localPort;
        //  Shared files folder path.
        private string sharedFolder;
        //  Manual reset event used by the main UploadManager thread to accept new connections,
        //  and also used by the StopListening function if the user wishes to stop uploading.
        private ManualResetEvent clientConnected = new ManualResetEvent(false); 
        //  Used by the StopListening class to stop the main UploadManager thread from accepting new connections.
        private bool keepAccepting;
        //  Delegate used to update the UI of the any upload progress.
        public static event TransferProgressDelegate transferProgressEvent;
        //  Used to help the UI distinguish between transfers.
        private static Random random = new Random();
        //  WcfMessageDelegate event used to notify the UI of errors and updates.
        public event WcfMessageDelegate wcfMessageEvent;

        public UploadManager(TransferProgressDelegate progressDelegate, WcfMessageDelegate messageDelegate) {
            transferProgressEvent += progressDelegate;
            wcfMessageEvent += messageDelegate;
        }

        /// <summary>
        /// Stops accepting new upload connection requests.
        /// </summary>
        public void StopListening() {
            keepAccepting = false;
            clientConnected.Set();
        }

        /// <summary>
        /// Starts listening for new upload connection requests.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="folder"></param>
        public void StartListening(int port, string folder) {
            keepAccepting = true;
            localPort = port;
            sharedFolder = folder;
            IPAddress ipAddress = Dns.Resolve(Dns.GetHostName()).AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, localPort);

            // Create a TCP/IP  socket.
            // Bind the  socket to the local endpoint and listen for incoming connections.
            using (Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {                                                                                                         
                try {
                    listener.Bind(localEndPoint);
                    listener.Listen(8);
                    while (keepAccepting) {
                        // Set the event to  nonsignaled state.
                        clientConnected.Reset();
                        // Start an asynchronous method to listen for connections.                          
                        listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                        // Wait until a connection is made before continuing.
                        clientConnected.WaitOne();  
                    }
                }

                catch (SocketException socketException) {
                    wcfMessageEvent(true, Properties.Resources.errorULManager1 +  socketException.ToString());
                    //listener.Shutdown(SocketShutdown.Both);
                    //listener.Close(0);
                }
            }
        }

        /// <summary>
        /// When a new connection request is accepted, this method starts the uploading process.
        /// </summary>
        /// <param name="ar"></param>
        public void AcceptCallback(IAsyncResult ar) {
            // Signal the main thread to continue accepting new connection requests.
            clientConnected.Set();
            Segment segment = new Segment();
            Socket handler = null;
            
            try {
                Socket listener = (Socket)ar.AsyncState;
                //  Get the socket that handles the client request.
                using (handler = listener.EndAccept(ar)) {
                    //  Get requested file info from the downloading peer.
                    GetFileInfo(handler, segment);
                    //  Sends the requested file to the downloading peer.
                    SendFile(handler, segment);
                }
            }
            catch (ObjectDisposedException objectDisposedException) { 
                Console.WriteLine(objectDisposedException.Message);    

            }                                                       
            catch (SocketException) {
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }

        }

        /// <summary>
        /// Recieves the requested file information from the downloading peer.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="segment"></param>
        private void GetFileInfo(Socket socket, Segment segment) {
            bool socketConnected = false;
            while (!socketConnected)
                IsSocketConnected(socket, ref socketConnected);
            NetworkStream nfs = null;
            try {
                using (nfs = new NetworkStream(socket)) {
                    StreamReader streamReader = new StreamReader(nfs);
                    string str = streamReader.ReadLine();
                    string[] fileInfo = str.Split('#');
                    segment.FileName = fileInfo[(int)SegmentInfo.FileName];
                    segment.StartPosition = Int64.Parse(fileInfo[(int)SegmentInfo.StartPosition]);
                    segment.Size = Int64.Parse(fileInfo[(int)SegmentInfo.Size]);
                    streamReader.Close();
                }
            }
            catch (IOException ioException) {
                wcfMessageEvent(true, Properties.Resources.errorULManager2 + ioException.Message);
                if (nfs != null)
                    nfs.Close();
            }
        }

        /// <summary>
        /// Sends the requested file to the downloading peer.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="segment"></param>
        private void SendFile(Socket socket, Segment segment) {
            bool socketConnected = false;
            while (!socketConnected)
                IsSocketConnected(socket, ref socketConnected);
            FileStream fin = null;
            NetworkStream nfs = null;
            Stopwatch stopWatch = new Stopwatch();
            int transferId;
            lock (random) {
                transferId = random.Next();
            }
            stopWatch.Start();
            try {
                using (nfs = new NetworkStream(socket)) {
                    long total = segment.Size;
                    //  Total bytes (of the segment) sent so far.
                    long totalSent = 0;
                    //  Length of the current buffer read from the file stream.
                    int bufferLength = 0;
                    byte[] buffer = new byte[1024 * 64];
                    //  Open the file requested for download 
                    using (fin = new FileStream(sharedFolder + "\\" + segment.FileName, FileMode.Open, FileAccess.Read)) {
                        //  Move the file stream position to the start of the requested segment.
                        fin.Seek(segment.StartPosition, 0);
                        //  Keep sending until the whole segment was sent and while the network stream is available.
                        while (totalSent < total && nfs.CanWrite) {
                            //  Read from the File (bufferLength contains the number of bytes read).
                            //  If the amount read from the file stream is small than whats left to send
                            if (buffer.Length < total - totalSent)
                                bufferLength = fin.Read(buffer, 0, buffer.Length);
                            //  Else, send the amount left to send (don't send empty bytes).
                            else
                                bufferLength = fin.Read(buffer, 0, (int)(total - totalSent));
                            //  Write the bytes to the network stream.
                            nfs.Write(buffer, 0, bufferLength);
                            //  Update the UI with the upload progress.
                            UpdateUploadProgress(transferId, segment.FileName, totalSent, total, stopWatch.ElapsedMilliseconds);
                            totalSent = totalSent + bufferLength;
                        }
                    }
                }
            }
            catch (IOException ioException) {
                wcfMessageEvent(true, Properties.Resources.errorULManager3 + ioException.Message);
                if (nfs != null)
                    nfs.Close();
                if (fin != null)
                    fin.Close();
            }
            stopWatch.Stop();
        }
        //
        /// <summary>
        /// Updates the UI with the progress of an upload.
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="fileName"></param>
        /// <param name="totalSent"></param>
        /// <param name="segmentSize"></param>
        /// <param name="time"></param>
        private void UpdateUploadProgress(int transferId, string fileName, long totalSent, long segmentSize, long time) {
               transferProgressEvent(transferId, fileName, segmentSize, totalSent, time, TransferType.Upload);
        }

        /// <summary>
        /// Checks if the socket is connected.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="socketConnected"></param>
        private void IsSocketConnected(Socket s, ref bool socketConnected) {
            Thread.Sleep(500);
            socketConnected = !(s.Poll(0, SelectMode.SelectRead) && s.Available == 0);
        }
    }
}
