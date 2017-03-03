using PeerUI;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAL
{
    class DBAccess
    {
        private const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Vitaly\Source\Repos\MiniTorrent_DotNET\TorrentDB2.mdf;Integrated Security=True;Connect Timeout=30";
        private static Connection connection = new Connection(connectionString);
        public static int PeersConnected { get; private set; }

        /// <summary>
        /// //////////////////////////////////??????
        /// </summary>
        /// <param name="alreadyConnected"></param>
        public static void ResetTables(bool alreadyConnected = false)
        {
            try
            {
                connection.Open();

                SqlCommand command = new SqlCommand(
                    "DELETE FROM File_Peer " +
                    "DELETE FROM Peers", connection.DatabaseConnection);
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally
            {
                connection.Close();
            }
            PeersConnected = 0;
        }

        //TODO Check Peer existense and fileSize
        public static void AddFile(Entities.File file, Entities.Peer peer, bool alreadyConnected = false)
        {
            SqlCommand command = null;
            try
            {
                //asas
                if (!alreadyConnected)
                    connection.Open();

                if (!FileExists(file.Name, true))
                {
                    command = new SqlCommand(
                    "INSERT INTO [Peers] (PeerName, PeerIP, PeerPort) " +
                    "VALUES(@PeerName, @PeerIP, @PeerPort)" +
                    "INSERT INTO [DataFiles] (FileName, FileSize) " +
                    "VALUES(@FileName, @FileSize)" +
                    "INSERT INTO [File_Peer] (FileName, PeerName) " +
                    "VALUES(@FileName, @PeerName)", connection.DatabaseConnection);
                }
                else if (!FileExistsByPeer(file.Name, peer.Name, true))
                {
                    command = new SqlCommand(
                    "INSERT INTO [File_Peer] (FileName, PeerName) " +
                    "VALUES(@FileName, @PeerName)", connection.DatabaseConnection);
                }
                else return;
                command.Parameters.Add("@PeerName", System.Data.SqlDbType.Char, peer.Name.Length).Value = peer.Name;
                command.Parameters.Add("@PeerIP", System.Data.SqlDbType.NVarChar, peer.Ip.Length).Value = peer.Ip;
                command.Parameters.Add("@PeerPort", System.Data.SqlDbType.NVarChar, peer.Port.Length).Value = peer.Port;
                command.Parameters.Add("@FileName", System.Data.SqlDbType.NVarChar, peer.Port.Length).Value = file.Name;
                command.Parameters.Add("@FileSize", System.Data.SqlDbType.Int).Value = file.Size;
                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally
            {
                if (!alreadyConnected) connection.Close();
            }
        }

        
        public static bool FileExists(string fileName, bool alreadyConnected = false)
        {
            bool isExist = false;
            SqlDataReader reader = null;
            try
            {
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [DataFiles] " +
                "WHERE FileName = @fileName;", connection.DatabaseConnection);
                command.Parameters.Add("@fileName", System.Data.SqlDbType.NVarChar).Value = fileName;

                reader = command.ExecuteReader(); // Execute the getting commande
                isExist = reader.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally
            {
                reader.Close();
                if (!alreadyConnected) connection.Close();
            }
            return isExist;
        }

        public static bool FileExistsByPeer(string fileName, string peerName, bool alreadyConnected = false)
        {
            bool isExist = false;
            SqlDataReader reader = null;
            try
            {
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [File_Peer] " +
                "WHERE [dbo].[File_Peer].[FileName] = @fileName " + 
                "AND [dbo].[File_Peer].[PeerName] = @peerName;", connection.DatabaseConnection);
                command.Parameters.Add("@fileName", System.Data.SqlDbType.NVarChar).Value = fileName;
                command.Parameters.Add("@peerName", System.Data.SqlDbType.NVarChar).Value = peerName;

                reader = command.ExecuteReader(); // Execute the getting command
                isExist = reader.Read();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally
            {
                reader.Close();
                if (!alreadyConnected) connection.Close();
            }
            return isExist;
        }

        public static List<Entities.Peer> GetPeersByFile(string fileName, bool alreadyConnected = false)
        {
            List<Entities.Peer> peers = new List<Entities.Peer>();

            // Initialize a data reader
            SqlDataReader reader = null;
            try
            {
                // Connect to the database
                if (!alreadyConnected)
                    connection.Open();

                // Initialize the getting command
                SqlCommand command = new SqlCommand(
                    "SELECT PeerIP, PeerPort " +
                    "FROM Peers, File_Peer " +
                    "WHERE [dbo].[File_Peer].[FileName] = @fileName " +
                    "AND [dbo].[Peers].[PeerName] = [dbo].[File_Peer].[PeerName];"
                    , connection.DatabaseConnection);
                command.Parameters.Add("@fileName", System.Data.SqlDbType.VarChar, fileName.Length).Value = fileName;

                // Execute the getting command
                reader = command.ExecuteReader();

                while (reader.Read())
                    peers.Add(new Entities.Peer( "name",reader.GetString(0),reader.GetString(1) ));

                // Close the data reader
                reader.Close();
            }
            catch (Exception e)
            {
                // Print the error message
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally
            {
                // Close the data reader and the database connection
                reader.Close();
                if (!alreadyConnected)
                    connection.Close();
            }

            // Return albums
            return peers;
        }

        /*  public static DataFile GetDataFileByName(string fileName, bool alreadyConnected = false)
          {
              DataFile dataFile = null;
              SqlDataReader reader = null;    // Initialize a data reader
              try
              {
                  // Connect to the database
                  if (!alreadyConnected)
                      connection.Open();

                  // Initialize the getting command
                  SqlCommand command = new SqlCommand(
                      "SELECT * " +
                      "FROM [DataFiles] " +
                      "WHERE FileName = @fileName;", connection.DatabaseConnection);
                  command.Parameters.Add("@fileName", System.Data.SqlDbType.NVarChar).Value = fileName;

                  // Execute the getting commande
                  reader = command.ExecuteReader();

                  // Get the returned user ID
                  if (reader.Read())
                  {
                     // reader.NextResult();
                     // List<User> peers = new List<User>(GetPeerByFileName(fileName));
                      List<Peer> peers = new List<Peer>();
                      dataFile = new DataFile(reader.GetString(1), long.Parse(reader.GetString(2)), peers);
                  }

              }
              catch (Exception e)
              {
                  Console.WriteLine("ERROR: " + e.Message);
              }
              finally
              {
                  reader.Close();
                  if (!alreadyConnected)
                      connection.Close();
              }
              return dataFile;
          }*/
    }
    
}
