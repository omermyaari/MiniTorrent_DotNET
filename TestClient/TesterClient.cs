using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Text;

namespace TestClient
{

    class Class1
    {
        private static ManualResetEvent connectDone = new ManualResetEvent(false);

        public static Segment segment = new Segment { FileName = "Fuck.txt", startPosition = 1, size = 50 };

        [STAThread]
        static void Main(string[] args)
        {

            Socket client = null;
            string RemoteIP = "192.168.43.165";
            int RemotePort = 4080;
            try
            {

                //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
                //IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPAddress ipAddress = IPAddress.Parse(RemoteIP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, RemotePort);

                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);      // Connect to the remote endpoint.
                bool success = connectDone.WaitOne(5000, false);
                Console.WriteLine("result: " + success);
                SendFileInfo(client);
                GetFileSegment(client);

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

        static void SendFileInfo(Socket s)
        {
            Socket socket = s;
            FileStream fin = null;

            try
            {
                NetworkStream nfs = new NetworkStream(socket);

              /*         byte[] bufferName = new byte[10];
                       bufferName = Encoding.ASCII.GetBytes(segment.FileName);

                       byte[] bufferPosition = new byte[8];
                       bufferPosition = BitConverter.GetBytes(segment.startPosition);

                       byte[] bufferSize = new byte[8];
                       bufferSize = BitConverter.GetBytes(segment.size);

                       

                       nfs.Write(bufferName, 0, bufferName.Length);
                       nfs.Read(bufferName, 0, bufferName.Length);
                       nfs.Write(bufferPosition, 0, bufferName.Length);
                       nfs.Read(bufferName, 0, bufferName.Length);
                       nfs.Write(bufferSize, 0, bufferName.Length);*/

                StreamWriter streamWriter = new StreamWriter(nfs);
                streamWriter.AutoFlush = true;
                string segmentStr = segment.FileName + "#" + segment.startPosition + "#" + segment.size;
                
                streamWriter.WriteLine(segmentStr);

                streamWriter.Flush();
                streamWriter.Close();
                

                Console.WriteLine("File INFO sent to server successfully !\n\n");
            }
            catch (Exception ed)
            {
                Console.WriteLine("A Exception occured in transfer in TESTER CLIENT" + ed.ToString());
            }
            finally
            {
                if (fin != null)
                    fin.Close();
            }
        }

        static void GetFileSegment(Socket socket)
        {

            FileStream fout = new FileStream("E:\\AFEKA\\Year 3\\DotNET\\PROJECT\\MY_PROJECT\\Fuck.txt", FileMode.Create, FileAccess.Write);

            NetworkStream nfs = new NetworkStream(socket);
            //long size=fi.Length ;

            int i = 1;
            long rby = 0;
            try
            {
                fout.Seek(segment.startPosition,0);
                //loop till the Full bytes have been read
                while (i< segment.size)
                {
                    byte[] buffer = new byte[100];

                    //Read from the Network Stream
                    i = nfs.Read(buffer, 0, buffer.Length);
                    fout.Write(buffer, 0, (int)i);

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
                if (fout != null)
                    fout.Close();
            }
        }


        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {

                Socket client = (Socket)ar.AsyncState; // Retrieve the socket from the state object.
                client.EndConnect(ar);  // Complete the connection.
                Console.WriteLine("Socket connected to "+ client.RemoteEndPoint.ToString());
                connectDone.Set();// Signal that the connection has been made.
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
