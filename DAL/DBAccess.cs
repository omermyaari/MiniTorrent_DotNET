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
        private const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=TorrentDB2.mdf;Integrated Security=True;Connect Timeout=30";
        private static Connection connection = new Connection(connectionString);
        public static int PeersConnected { get; private set; }

        public static void ResetTables(bool alreadyConnected = false)
        {
            try
            {
                connection.Open();

                SqlCommand command = new SqlCommand(
                    "DELETE FROM File_Peer;" +
                    "DELETE FROM Peers;" +
                    "DELETE FROM DataFiles:" , connection.DatabaseConnection);
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

        public static void AddFile(Entities.File file, Entities.Peer peer, bool alreadyConnected = false)
        {
            try
            {
                if (!alreadyConnected)
                    connection.Open();

                if(!FileExists(file.Name))
                {
                    SqlCommand command = new SqlCommand(
                    "INSERT INTO [Peers] (PeerName, PeerIP, PeerPort) " +
                    "VALUES(@PeerName, @PeerIP, @PeerPort)" +
                    "INSERT INTO [DataFiles] (FileName, FileSize) " +
                    "VALUES(@FileName, @FileSize)" +
                    "INSERT INTO [File_Peer] (FileName, PeerName) " +
                    "VALUES(@FileName, @PeerName)"
                    , connection.DatabaseConnection);
                    command.Parameters.Add("@PeerName", System.Data.SqlDbType.Char, peer.Name.Length).Value = peer.Name;
                    command.Parameters.Add("@PeerIP", System.Data.SqlDbType.NVarChar, peer.IP.Length).Value = peer.IP;
                    command.Parameters.Add("@PeerPort", System.Data.SqlDbType.NVarChar, peer.Port.Length).Value = peer.Port;
                    command.Parameters.Add("@FileName", System.Data.SqlDbType.NVarChar, peer.Port.Length).Value = file.Name;
                    command.Parameters.Add("@FileSize", System.Data.SqlDbType.Int).Value = file.Size;
                    command.ExecuteNonQuery();
                }
                
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

        public static bool FileExists(string fileName)
        {
            SqlDataReader reader = null;

            SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [DataFiles] " +
                "WHERE FileName = @fileName;", connection.DatabaseConnection);
            command.Parameters.Add("@fileName", System.Data.SqlDbType.NVarChar).Value = fileName;
                
            reader = command.ExecuteReader(); // Execute the getting commande

            return reader.Read();             
            }            
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
