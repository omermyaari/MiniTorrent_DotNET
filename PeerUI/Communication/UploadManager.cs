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

namespace PeerUI
{
    class UploadManager
    {
        public static int localPort;
        public static string sharedFolder;
        public static Segment segment;

        public static ManualResetEvent allDone = new ManualResetEvent(false); // Thread signal.

        public static void StartListening(int port, string folder)
        {
            localPort = port;
            sharedFolder = folder;

            IPAddress ipAddress = Dns.Resolve(Dns.GetHostName()).AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, localPort);

            MessageBox.Show("Binding port " + localPort + " at local address " + ipAddress.ToString());
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // Create a TCP/IP  socket.
            MessageBox.Show("Waiting for a connection...\n\n");

            // Bind the  socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(8); //pending connections queue
                while (true)
                {
                    allDone.Reset();  // Set the event to  nonsignaled state.
                    // Start  an asynchronous socket to listen for connections.
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();  // Wait until a connection is made before continuing.
                }
            }
            catch (Exception e) { MessageBox.Show(e.ToString()); }

            MessageBox.Show("\nHit enter to continue...");
            Console.Read();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();
            segment = new Segment();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            MessageBox.Show("Connected to client at " + handler.RemoteEndPoint.ToString());

            GetFileInfo(handler);
            SendFile(handler);
        }

        static void GetFileInfo(Socket socket)
        {
            NetworkStream nfs = new NetworkStream(socket);
            try
            {
                    StreamReader streamReader = new StreamReader(nfs);
                    string str = streamReader.ReadLine();
                    string[] fileInfo = str.Split('#');

                    segment.FileName = fileInfo[(int)SegmentInfo.FileName];
                    segment.StartPosition = Int32.Parse(fileInfo[(int)SegmentInfo.StartPosition]);
                    segment.SegmentSize = Int32.Parse(fileInfo[(int)SegmentInfo.Size]);

            /*  

                byte[] bufferName = new byte[10];
                byte[] bufferPosition = new byte[8];
                byte[] bufferSize = new byte[8];

                string cmd = "FILEOK";
                Byte[] sender = new Byte[1024]; ;
                sender = Encoding.ASCII.GetBytes(cmd);

               */

          /*      nfs.Read(bufferName, 0, bufferName.Length);
                nfs.Write(sender, 0, sender.Length);
                nfs.Read(bufferPosition, 0, bufferPosition.Length);
                nfs.Write(sender, 0, sender.Length);
                nfs.Read(bufferSize, 0, bufferSize.Length);*/

            /*    segment.FileName = Encoding.UTF8.GetString(bufferName).TrimEnd('\0');
                segment.StartPosition = BitConverter.ToInt32(bufferPosition, 0);
                segment.SegmentSize = BitConverter.ToInt32(bufferSize, 0);*/

                  streamReader.Close();

                MessageBox.Show("The name is: " + segment.FileName + "\n" +
                                "The start position is: " + segment.StartPosition + "\n" +
                                "The size of segment is: " + segment.SegmentSize + "\n" +
                                "File INFO received successfully !");
            }
            catch (Exception ed)
            {
                MessageBox.Show("A Exception occured in file transfer in TRANSFERE MANAGER" + ed.Message);
            }
            finally { if (nfs != null) nfs.Close(); }
        }

        static void SendFile(Socket socket)
        {
            FileStream fin = null;
            try
            {
                //TODO check file existence of the file???
                NetworkStream nfs = new NetworkStream(socket);
                //  FileInfo ftemp = new FileInfo(FileName);
                long total = segment.StartPosition + segment.SegmentSize;
                long ToatlSent = 0;

                int len = 0;
                byte[] buffer = new byte[100];
                //Open the file requested for download 
                fin = new FileStream(sharedFolder + "\\" + segment.FileName, FileMode.Open, FileAccess.Read);
                fin.Seek(segment.StartPosition, 0);
                //One way of transfer over sockets is Using a NetworkStream 
                //It provides some useful ways to transfer data 

                while (ToatlSent < total && nfs.CanWrite)
                {
                    //Read from the File (len contains the number of bytes read)
                    len = fin.Read(buffer, 0, buffer.Length);

                    MessageBox.Show("len =  " + len + "\n");
                    //Write the Bytes on the Socket
                    nfs.Write(buffer, 0, len);
                    //Increase the bytes Read counter
                    ToatlSent = ToatlSent + len;
                    MessageBox.Show("wrote: " + ToatlSent);
                }
                MessageBox.Show("File sent to server successfully !\n\n");
            }
            catch (Exception ed)
            {
                MessageBox.Show("A Exception occured in transfer the FILE in UPLOAD MANAGER!" + ed.ToString());
            }
            finally { if (fin != null) fin.Close(); }
        }
    }
}
