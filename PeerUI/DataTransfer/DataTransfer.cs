using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using PeerUI.DataClasses;
using System.Xml.Linq;
using System.Xml;

namespace PeerUI.FileTransfer {
    /*public class DataTransfer {

        //public Socket connectionSocket;
        public const int BufferSize = 1024;             //  Buffer size
        public byte[] buffer = new byte[BufferSize];    //  Buffer

    }
    */

    public class AsynchronousSocketListener {

        private const int BufferSize = 1024;             //  Buffer size
        private byte[] buffer = new byte[BufferSize];    //  Buffer
        private Socket connectionSocket;
        private string sharedFolderPath;
        private string downloadFolderPath;

        public string FullFileName {
            get; set;
        }

        public int LocalPort {
            get; set;
        }

        private ManualResetEvent allDone = new ManualResetEvent(false);

        public AsynchronousSocketListener(int LocalPort, string sharedFolderPath, string downloadFolderPath) {
            this.LocalPort = LocalPort;
            this.sharedFolderPath = sharedFolderPath;
            this.downloadFolderPath = downloadFolderPath;
        }

        public void startListening() {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress localIpAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(localIpAddress, LocalPort);
            Console.WriteLine("Binding port {0} at local address {1}", LocalPort, localIpAddress.ToString());
            //  Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Waiting for a connection..\n\n");

            //  Bind the socket to the local endpoint and listen for incoming connections.

            try {
                listener.Bind(localEndPoint);
                listener.Listen(100);   //  The maximum length of pending connections queue before acceptance.

                while (true) {
                    //  Reset the manual reset event so
                    allDone.Reset();    //  Set the event to nonsignaled state.
                    //  Start an asynchronous socket to listen for connections.
                    listener.BeginAccept(new AsyncCallback(AcceptCallBack), listener);
                    allDone.WaitOne();  //  Wait until a conncetion is made before continuing.
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
        }

        public void AcceptCallBack(IAsyncResult ar) {
            //  Signal the connections accepting thread to continue.
            allDone.Set();

            //  Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            connectionSocket = handler;
            Console.WriteLine("Connected to client at {0}\n ", handler.RemoteEndPoint.ToString());
            SendFile();
        }

        private void SendFile() {
            NetworkStream networkStream = new NetworkStream(connectionSocket);
            StreamReader streamReader = new StreamReader(networkStream);
            StreamWriter streamWriter = new StreamWriter(networkStream);
            XmlSerializer xmlSerialzer = new XmlSerializer(typeof(Handshake));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(streamReader.ReadToEnd());
            var fileRequest = doc.GetElementsByTagName("File");
            string filename = fileRequest[0].ToString();
            FullFileName = sharedFolderPath + filename;
            if (File.Exists(FullFileName)) {
                var fileInfo = new FileInfo(FullFileName);
                var handshake = new Handshake {
                    FullFilename = FullFileName,
                    FileSize = fileInfo.Length,
                    BufferSize = BufferSize,
                    Start = 0,
                    End = fileInfo.Length
                };
                xmlSerialzer.Serialize(streamWriter, handshake);
                doc.LoadXml(streamReader.ReadToEnd());
                handshake = (Handshake) xmlSerialzer.Deserialize(streamReader);
                SendByBytes(handshake, networkStream);
            }
            else {
                XDocument xDoc = new XDocument(new XElement("File", new XAttribute("missing", "true"), filename));
                streamWriter.Write(Encoding.UTF8.GetBytes(xDoc.ToString()).Length);
                streamWriter.Write(xDoc.ToString());
                streamWriter.Flush();
            }
        }

        //  Sends the requested part of the file.
        private void SendByBytes(Handshake handshake, NetworkStream networkStream) {
            FileStream fileStream = new FileStream(FullFileName, FileMode.Open, FileAccess.Read);
            fileStream.Seek(handshake.Start, SeekOrigin.Begin);
            long totalSent = 0;
            int length = 0;
            while ((totalSent < (handshake.End - handshake.Start) && networkStream.CanWrite)) {
                length = fileStream.Read(buffer, 0, buffer.Length);
                networkStream.Write(buffer, 0, length);
                totalSent += length;
                Console.WriteLine("Sent: " + totalSent);
            }
        }
    }
}
