using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PeerUI {
    public class UploadManager {
        private int localPort;
        private string sharedFolder;
        private ManualResetEvent clientConnected = new ManualResetEvent(false); // Thread signal.
        private bool keepAccepting;

        public void StopListening() {
            keepAccepting = false;
            clientConnected.Set();
        }

        public void StartListening(int port, string folder) {
            keepAccepting = true;
            localPort = port;
            sharedFolder = folder;
            IPAddress ipAddress = Dns.Resolve(Dns.GetHostName()).AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, localPort);
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Create a TCP/IP  socket.
            // Bind the  socket to the local endpoint and listen for incoming connections.
            try {
                listener.Bind(localEndPoint);
                listener.Listen(8); //pending connections queue
                while (keepAccepting) {
                    clientConnected.Reset();  // Set the event to  nonsignaled state.
                    // Start  an asynchronous socket to listen for connections.
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    clientConnected.WaitOne();  // Wait until a connection is made before continuing.
                }
            }
            catch (Exception e) {
                MessageBox.Show(e.ToString());
            }
            finally {
                listener.Close();
            }
        }

        public void AcceptCallback(IAsyncResult ar) {
            // Signal the main thread to continue.
            clientConnected.Set();
            Segment segment = new Segment();
            Socket handler = null;
            // Get the socket that handles the client request.
            try {
                Socket listener = (Socket)ar.AsyncState;
                handler = listener.EndAccept(ar);
                GetFileInfo(handler, segment);
                SendFile(handler, segment);
            }
            catch (ObjectDisposedException objectDisposedException) {
                Console.WriteLine(objectDisposedException.Message); //  Dont really need to take care of this exception,
            }                                                       //  its like a signal that says the connection closed.
            finally {
                //handler.Close();
                //MessageBox.Show("Socket closed at uploader.");
            }

        }

        private void GetFileInfo(Socket socket, Segment segment) {
            NetworkStream nfs = new NetworkStream(socket);
            try {
                StreamReader streamReader = new StreamReader(nfs);
                string str = streamReader.ReadLine();
                string[] fileInfo = str.Split('#');
                segment.FileName = fileInfo[(int)SegmentInfo.FileName];
                segment.StartPosition = Int64.Parse(fileInfo[(int)SegmentInfo.StartPosition]);
                segment.Size = Int64.Parse(fileInfo[(int)SegmentInfo.Size]);
                streamReader.Close();
            }
            catch (Exception ed) {
                MessageBox.Show("A Exception occured in file transfer in TRANSFERE MANAGER" + ed.Message);
            }
        }

        private void SendFile(Socket socket, Segment segment) {
            FileStream fin = null;
            NetworkStream nfs = null;
            try {
                //TODO check file existence of the file???
                nfs = new NetworkStream(socket);
                //  FileInfo ftemp = new FileInfo(FileName);
                long total = segment.Size;
                long totalSent = 0;
                int len = 0;
                byte[] buffer = new byte[1024 * 128];
                //Open the file requested for download 
                fin = new FileStream(sharedFolder + "\\" + segment.FileName, FileMode.Open, FileAccess.Read);
                fin.Seek(segment.StartPosition, 0);
                //One way of transfer over sockets is Using a NetworkStream 
                //It provides some useful ways to transfer data 

                while (totalSent < total && nfs.CanWrite) {
                    //Read from the File (len contains the number of bytes read)
                    if (buffer.Length < total - totalSent)
                        len = fin.Read(buffer, 0, buffer.Length);
                    else
                        len = fin.Read(buffer, 0, (int)(total - totalSent));
                    //MessageBox.Show("len =  " + len + "\n");
                    //Write the Bytes on the Socket
                    nfs.Write(buffer, 0, len);
                    //Increase the bytes Read counter
                    totalSent = totalSent + len;
                }
            }
            catch (Exception ed) {
                MessageBox.Show("A Exception occured in transfer the FILE in UPLOAD MANAGER!" + ed.ToString());
            }
            finally {
                if (fin != null)
                    fin.Close();
                if (nfs != null)
                    nfs.Close();
                if (socket != null) {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
        }
    }
}
