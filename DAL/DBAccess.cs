using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using System.IO;
//  CONSTRAINT [PK_File_Peer] PRIMARY KEY CLUSTERED ([FileId] ASC, [PeerName] ASC),
//  this line was removed from File_Peer table definition.

namespace DAL {
    public static class DBAccess {

        private static readonly string connectionString;
        private static Connection connection;
        public static long IdCounter = 0;

        static DBAccess() {
            //string RunningPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
            //string DatabaseFilePath = string.Format("{0}" + "TorrentDB2.mdf", Path.GetFullPath(Path.Combine(RunningPath, @"..\..\")));
            connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\omer\Source\Repos\MiniTorrent_DotNET\TorrentDB2.mdf;Integrated Security=True;Connect Timeout=30";
            connection = new Connection(connectionString);
        }

        public static int PeersConnected {
            get; private set;
        }

        /// <summary>
        /// Clear all data in the tables.
        /// </summary>
        /// <param name="alreadyConnected"></param>
        public static void ResetTables(bool alreadyConnected = false) {
            try {
                connection.Open();

                SqlCommand command = new SqlCommand(
                    "DELETE FROM File_Peer " +
                    "DELETE FROM Peers " +
                    "DELETE FROM DataFiles;", connection.DatabaseConnection);
                command.ExecuteNonQuery(); // Execute the getting command  
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                connection.Close();
            }
            PeersConnected = 0;
        }

        public static void UpdateLastId(bool alreadyConnected = false) {
            SqlDataReader reader = null;
            try {
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT FileId " +
                "FROM [DataFiles] " +
                "ORDER BY FileId DESC;" , connection.DatabaseConnection);
                reader = command.ExecuteReader(); // Execute the getting command
                if (reader.Read())
                    IdCounter = reader.GetInt64(0) + 1;
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                reader.Close();
                if (!alreadyConnected)
                    connection.Close();
            }
        }

        //  Checks if the given peer username and password are correct.
        public static bool CheckPeerAuth(DBPeer peer, bool alreadyConnected = false) {
            bool LoginOk = false;
            SqlDataReader reader = null;
            try {
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [Peers] " +
                "WHERE PeerName = @peerName " +
                "AND PeerPassword = @peerPassword;", connection.DatabaseConnection);
                command.Parameters.Add("@peerName", System.Data.SqlDbType.NVarChar).Value = peer.Name;
                command.Parameters.Add("@peerPassword", System.Data.SqlDbType.NVarChar).Value = peer.Password;
                reader = command.ExecuteReader(); // Execute the getting command

                LoginOk = reader.Read();
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                reader.Close();
                if (!alreadyConnected)
                    connection.Close();
            }
            return LoginOk;
        }

        //  Registers a new peer (used by the ASP.Net website).
        public static void RegisterPeer(DBPeer peer) {
            SqlCommand command = null;
            try {
                // Connect to the database
                connection.Open();

                if (!PeerExists(peer, true)) {
                    command = new SqlCommand(
                    "INSERT INTO [Peers] (PeerName, PeerPassword, PeerIP, PeerPort, PeerIsOnline ) " +
                    "VALUES(@PeerName, @PeerPassword, @PeerIP, @PeerPort, @PeerIsOnline);", connection.DatabaseConnection);
                    //  set parameters for the query
                    command.Parameters.Add("@PeerName", System.Data.SqlDbType.Char, peer.Name.Length).Value = peer.Name;
                    command.Parameters.Add("@PeerPassword", System.Data.SqlDbType.Char, peer.Password.Length).Value = peer.Password;
                    command.Parameters.Add("@PeerIP", System.Data.SqlDbType.NVarChar, peer.Ip.Length).Value = peer.Ip;
                    command.Parameters.Add("@PeerPort", System.Data.SqlDbType.Int).Value = peer.Port;
                    command.Parameters.Add("@PeerIsOnline", System.Data.SqlDbType.Bit).Value = false;
                    command.ExecuteNonQuery();    // Execute the getting command   
                }
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                connection.Close();
            }
        }

        //  Adds a file shared by a peer to the database.
        public static void AddFile(DBFile file, DBPeer peer, bool alreadyConnected = false) {
            SqlCommand command = null;
            try {
                // Connect to the database
                if (!alreadyConnected)
                    connection.Open();

                long fileId = FileExists(file, true);
                //  If file doesnt exist, add it to the DataFiles table and add the peer's name
                //  and the file's id to the File_Peer table.
                if (fileId == -1) {
                    command = new SqlCommand(
                    "INSERT INTO [DataFiles] (FileId, FileName, FileSize) " +
                    "VALUES(@FileId, @FileName, @FileSize)" +

                    "INSERT INTO [File_Peer] (FileId, PeerName) " +
                    "VALUES(@FileId, @PeerName)", connection.DatabaseConnection);

                    //  set parameters for the query
                    command.Parameters.Add("@FileId", System.Data.SqlDbType.BigInt).Value = ++IdCounter;
                    command.Parameters.Add("@FileName", System.Data.SqlDbType.VarChar, file.Name.Length).Value = file.Name;
                    command.Parameters.Add("@FileSize", System.Data.SqlDbType.BigInt).Value = file.Size;
                    command.Parameters.Add("@PeerName", System.Data.SqlDbType.NVarChar, peer.Name.Length).Value = peer.Name;
                    command.ExecuteNonQuery();    // Execute the getting command   
                }
                //  If file exists, just add the peer's name and the file's id to the File_Peer table.
                else {
                    //  Check if the File_Peer table already contains the file's id and the peer's name.
                    if (!FileExistsByPeer(fileId, peer.Name, true)) {
                        command = new SqlCommand(
                    "INSERT INTO [File_Peer] (FileId, PeerName) " +
                    "VALUES(@FileId, @PeerName)", connection.DatabaseConnection);
                        command.Parameters.Add("@FileId", System.Data.SqlDbType.BigInt).Value = fileId;
                        command.Parameters.Add("@PeerName", System.Data.SqlDbType.NVarChar, peer.Name.Length).Value = peer.Name;
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                if (!alreadyConnected)
                    connection.Close();
            }
        }

        //  Checks if a given file (name and size) exists, if it does, the method will return its id,
        //  otherwise, it will return -1.
        public static long FileExists(DBFile file, bool alreadyConnected = false) {
            bool isExist = false;
            SqlDataReader reader = null;
            try {
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT FileId " +
                "FROM [DataFiles] " +
                "WHERE FileName = @fileName " +
                "AND FileSize = @fileSize", connection.DatabaseConnection);
                command.Parameters.Add("@fileName", System.Data.SqlDbType.NVarChar, file.Name.Length).Value = file.Name;
                command.Parameters.Add("@fileSize", System.Data.SqlDbType.BigInt).Value = file.Size;
                reader = command.ExecuteReader(); // Execute the getting command
                isExist = reader.Read();
                if (isExist)
                    return reader.GetInt64(0);
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                reader.Close();
                if (!alreadyConnected)
                    connection.Close();
            }
            return -1;
        }

        //  Checks if a given peer is registered and if his password is correct.
        public static bool PeerExists(DBPeer peer, bool alreadyConnected = false) {
            bool isExist = false;
            SqlDataReader reader = null;
            try {
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [Peers] " +
                "WHERE PeerName = @peerName;", connection.DatabaseConnection);
                command.Parameters.Add("@peerName", System.Data.SqlDbType.NVarChar).Value = peer.Name;
                reader = command.ExecuteReader(); // Execute the getting command

                isExist = reader.Read();
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                reader.Close();
                if (!alreadyConnected)
                    connection.Close();
            }
            return isExist;
        }

        //  Checks if a given peer is sharing a given file.
        public static bool FileExistsByPeer(long fileId, string peerName, bool alreadyConnected = false) {
            bool isExist = false;
            SqlDataReader reader = null;

            try {
                // Connect to the database
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [File_Peer] " +
                "WHERE [dbo].[File_Peer].[FileId] = @fileId " +
                "AND [dbo].[File_Peer].[PeerName] = @peerName;", connection.DatabaseConnection);
                command.Parameters.Add("@fileId", System.Data.SqlDbType.BigInt).Value = fileId;
                command.Parameters.Add("@peerName", System.Data.SqlDbType.NVarChar).Value = peerName;
                reader = command.ExecuteReader(); // Execute the getting command
                isExist = reader.Read();
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                reader.Close();
                if (!alreadyConnected)
                    connection.Close();
            }
            return isExist;
        }

        // Returns list of peers that have the given filename.
        public static Dictionary<DBFile, List<DBPeer>> SearchFiles(string fileName, bool alreadyConnected = false) {
            // Initialize a data reader
            var searchFileResult = new Dictionary<DBFile, List<DBPeer>>();
            SqlDataReader reader = null;
            SqlCommand command = null;
            try {
                // Connect to the database
                if (!alreadyConnected)
                    connection.Open();
                if (fileName.Equals("*")) {
                    command = new SqlCommand(
                   "SELECT FileName, FileSize, PeerIP, PeerPort, PeerIsOnline " +
                   "FROM DataFiles, Peers, File_Peer " +
                   "WHERE [dbo].[File_Peer].[FileId] = [dbo].[DataFiles].[FileId] " +
                   "AND [dbo].[Peers].[PeerName] = [dbo].[File_Peer].[PeerName] " +
                   "ORDER BY FileName, FileSize;", connection.DatabaseConnection);
                }
                else {
                    command = new SqlCommand(
                   "SELECT FileName, FileSize, PeerIP, PeerPort, PeerIsOnline " +
                   "FROM DataFiles, Peers, File_Peer " +
                   "WHERE [dbo].[File_Peer].[FileId] = [dbo].[DataFiles].[FileId] " +
                   "AND [dbo].[Peers].[PeerName] = [dbo].[File_Peer].[PeerName] " +
                   "AND [dbo].[DataFiles].[FileName] LIKE '%'+@fileName+'%' " +
                   "ORDER BY FileName, FileSize;", connection.DatabaseConnection);
                    command.Parameters.Add("@fileName", System.Data.SqlDbType.NVarChar, fileName.Length).Value = fileName;
                }
               
                reader = command.ExecuteReader(); // Execute the getting command

                while (reader.Read()) {
                    var file = new DBFile(reader.GetString(0), reader.GetInt64(1));
                    var peer = new DBPeer("user", "password", reader.GetString(2), reader.GetInt32(3));
                    peer.IsOnline = reader.GetBoolean(4);
                    if (searchFileResult.ContainsKey(file))
                        searchFileResult[file].Add(peer);
                    else {
                        searchFileResult.Add(file, new List<DBPeer>());
                        searchFileResult[file].Add(peer);
                    }
                }

                // Close the data reader
                reader.Close();
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                reader.Close();
                if (!alreadyConnected)
                    connection.Close();
            }
            return searchFileResult;
        }
        /*
        public static bool CheckPeerOnline(DBPeer peer, bool alreadyConnected = false) {
            SqlDataReader reader = null;
            bool IsOnline = false;
            try {
                // Connect to the database
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = null;
                command = new SqlCommand(
                "SELECT PeerIsOnline " +
                "FROM [Peers] " +
                "WHERE [dbo].[Peers].[PeerName] = @PeerName;", connection.DatabaseConnection);
                command.Parameters.Add("@PeerName", System.Data.SqlDbType.NVarChar).Value = peer.Name;
                reader = command.ExecuteReader(); // Execute the getting command
                while (reader.Read()) {
                    IsOnline = reader.GetBoolean(0);
                }
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                if (!alreadyConnected)
                    connection.Close();
            }
            return IsOnline;
        }
        */

        public static void LoginPeer(DBPeer peer, bool alreadyConnected = false) {
            lock (connection) {
                try {
                    // Connect to the database
                    if (!alreadyConnected)
                        connection.Open();
                    SqlCommand command = null;
                    command = new SqlCommand(
                    "UPDATE Peers " +
                    "SET PeerIP = @PeerIP, PeerPort = @PeerPort " +
                    "WHERE PeerName = @PeerName " +
                    "AND PeerPassword = @PeerPassword;", connection.DatabaseConnection);
                    command.Parameters.Add("@PeerIP", System.Data.SqlDbType.NVarChar, peer.Ip.Length).Value = peer.Ip;
                    command.Parameters.Add("@PeerPort", System.Data.SqlDbType.Int).Value = peer.Port;
                    command.Parameters.Add("@PeerName", System.Data.SqlDbType.VarChar, peer.Name.Length).Value = peer.Name;
                    command.Parameters.Add("@PeerPassword", System.Data.SqlDbType.VarChar, peer.Password.Length).Value = peer.Password;

                    command.ExecuteNonQuery();
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR: " + e.Message);
                }
                finally {
                    if (!alreadyConnected)
                        connection.Close();
                }
            }
        }

        //  Sets the status of the given peer name (online or offline) with the given bool.
        public static void SetPeerStatus(DBPeer peer, bool online, bool alreadyConnected = false) {
            lock (connection) {
                try {
                    // Connect to the database
                    if (!alreadyConnected)
                        connection.Open();
                    SqlCommand command = null;
                    command = new SqlCommand(
                    "UPDATE Peers " +
                    "SET PeerIsOnline = @PeerIsOnline " +
                    "WHERE PeerName = @PeerName " +
                    "AND PeerPassword = @PeerPassword;", connection.DatabaseConnection);
                    command.Parameters.Add("@PeerIsOnline", System.Data.SqlDbType.Bit, peer.Ip.Length).Value = online;
                    command.Parameters.Add("@PeerName", System.Data.SqlDbType.VarChar, peer.Name.Length).Value = peer.Name;
                    command.Parameters.Add("@PeerPassword", System.Data.SqlDbType.VarChar, peer.Password.Length).Value = peer.Password;
                    command.ExecuteNonQuery();

                    if (!online) {
                        command = new SqlCommand(
                        "DELETE FROM File_Peer " +
                        "WHERE PeerName = @PeerName;", connection.DatabaseConnection);
                        command.Parameters.Add("@PeerName", System.Data.SqlDbType.VarChar, peer.Name.Length).Value = peer.Name;
                    }
                    command.ExecuteNonQuery();
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR: " + e.Message);
                }
                finally {
                    if (!alreadyConnected)
                        connection.Close();
                }
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
        public static List<DBPeer> GetAllPeers(bool alreadyConnected = false) {
            List<DBPeer> peers = new List<DBPeer>();
            SqlDataReader reader = null;
            try {
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [Peers];", connection.DatabaseConnection);
                reader = command.ExecuteReader(); // Execute the getting command

                while (reader.Read()) {
                    DBPeer peer = new DBPeer(reader.GetString(0), reader.GetString(1),
                        reader.GetString(2), reader.GetInt32(3));
                    peer.IsOnline = reader.GetBoolean(4);
                    peers.Add(peer);
                }
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                reader.Close();
                if (!alreadyConnected)
                    connection.Close();
            }
            return peers;
        }

        public static List<DBFile> GetAllFiles(bool alreadyConnected = false) {
            List<DBFile> files = new List<DBFile>();
            SqlDataReader reader = null;
            try {
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [DataFiles];", connection.DatabaseConnection);
                reader = command.ExecuteReader(); // Execute the getting command

                while (reader.Read()) {
                    DBFile file = new DBFile(reader.GetString(1), reader.GetInt64(2));
                    file.ID = reader.GetInt64(0);
                    files.Add(file);
                }
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                reader.Close();
                if (!alreadyConnected)
                    connection.Close();
            }
            return files;
        }

        public static bool DeletePeer(string peerName, bool alreadyConnected = false) {
            lock (connection) {
                try {
                    connection.Open();
                    SqlCommand command = new SqlCommand(
                        "DELETE FROM File_Peer " +
                        "WHERE PeerName = @peerName " +
                        "DELETE FROM Peers " +
                        "WHERE PeerName = @peerName;", connection.DatabaseConnection);
                    command.Parameters.Add("@peerName", System.Data.SqlDbType.Char, peerName.Length).Value = peerName;
                    command.ExecuteNonQuery(); // Execute the getting command 
                    return true;
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR: " + e.Message);
                    return false;
                }
                finally { connection.Close(); }
            }
        }

        //Update the Peer fields.
        public static bool UpdatePeer(DBPeer oldPeer, DBPeer newPeer, bool alreadyConnected = false) {
            //Check existance of the newPeer: TODO: implement equals in DBPeer
            if (oldPeer.Equals(newPeer))
                return true;
            lock (connection) {
                try {
                    SqlCommand command = null;
                    connection.Open();

                    //If the peers have a same name, update only in Peers table.
                    if (oldPeer.Name.Equals(newPeer.Name)) {
                        command = new SqlCommand(
                        "UPDATE Peers " +
                        "SET PeerPassword = @newPassword, " +
                        "PeerIP = @newIp, PeerPort = @newPort " +
                        "WHERE PeerName = @oldName;", connection.DatabaseConnection);
                    }
                    //else update Peers table and File_Peer too.
                    else {
                        command = new SqlCommand(
                        "BEGIN TRANSACTION;" +
                        "EXEC sp_msforeachtable \"ALTER TABLE ? NOCHECK CONSTRAINT ALL\";" +
                        "UPDATE Peers " +
                        "SET PeerName = @newName, PeerPassword = @newPassword, " +
                        "PeerIP = @newIp, PeerPort = @newPort " +
                        "WHERE PeerName = @oldName;" +
                        "UPDATE File_Peer " +
                        "SET PeerName = @newName " +
                        "WHERE PeerName = @oldName;" +
                        "EXEC sp_msforeachtable \"ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL\";" +
                        "COMMIT;", connection.DatabaseConnection);
                    }
                    command.Parameters.Add("@newName", System.Data.SqlDbType.Char, newPeer.Name.Length).Value = newPeer.Name;
                    command.Parameters.Add("@newPassword", System.Data.SqlDbType.Char, newPeer.Password.Length).Value = newPeer.Password;
                    command.Parameters.Add("@newIp", System.Data.SqlDbType.Char, newPeer.Ip.Length).Value = newPeer.Ip;
                    command.Parameters.Add("@newPort", System.Data.SqlDbType.Int).Value = newPeer.Port;
                    command.Parameters.Add("@oldName", System.Data.SqlDbType.Char, oldPeer.Name.Length).Value = oldPeer.Name;
                    command.ExecuteNonQuery(); // Execute the getting command  
                    return true;
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR: " + e.Message);
                    return false;
                }
                finally { connection.Close(); }
            }
        }
    }
}