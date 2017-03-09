using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using DAL.Entities;

namespace DAL {

    /// <summary>
    /// This class is responsible of handling the data from the database.
    /// </summary>
    public static class DBAccess {

        //  Holds the Connection settings.
        private static readonly string connectionString;
        //  Holds the database connection properties.
        private static Connection connection;
        //  Used to produce unique id numbers for files stored in the database.
        public static long IdCounter = 0;

        static DBAccess() {
            connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\omer\Source\Repos\MiniTorrent_DotNET\TorrentDB2.mdf;Integrated Security=True;Connect Timeout=30";
            connection = new Connection(connectionString);
        }

        /// <summary>
        /// Clear all data in the tables.
        /// </summary>
        public static void ResetTables(bool alreadyConnected = false) {
            try {
                // Connect to the database.
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                    "DELETE FROM File_Peer " +
                    "DELETE FROM Peers " +
                    "DELETE FROM DataFiles;", connection.DatabaseConnection);

                //  Execute the query.
                command.ExecuteNonQuery();
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                connection.Close();
            }
        }

        /// <summary>
        /// Updates the ID counter if the server has started recently.
        /// </summary>
        /// <param name="alreadyConnected"></param>
        public static void UpdateLastId(bool alreadyConnected = false) {
            SqlDataReader reader = null;
            try {
                // Connect to the database.
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT FileId " +
                "FROM [DataFiles] " +
                "ORDER BY FileId DESC;" , connection.DatabaseConnection);

                //  Execute the query.
                reader = command.ExecuteReader();
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

        /// <summary>
        /// Checks if the given peer username and password are correct.
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="alreadyConnected"></param>
        /// <returns>bool</returns>
        public static bool CheckPeerAuth(DBPeer peer, bool alreadyConnected = false) {
            bool LoginOk = false;
            SqlDataReader reader = null;
            try {
                // Connect to the database.
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [Peers] " +
                "WHERE PeerName = @peerName " +
                "AND PeerPassword = @peerPassword;", connection.DatabaseConnection);

                //  Set parameters for the query.
                command.Parameters.Add("@peerName", System.Data.SqlDbType.NVarChar).Value = peer.Name;
                command.Parameters.Add("@peerPassword", System.Data.SqlDbType.NVarChar).Value = peer.Password;
                //  Execute the query.
                reader = command.ExecuteReader();

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

        /// <summary>
        /// Registers a new peer (used by the ASP.Net website).
        /// </summary>
        /// <param name="peer"></param>
        public static void RegisterPeer(DBPeer peer) {
            SqlCommand command = null;
            try {
                // Connect to the database.
                connection.Open();

                if (!PeerExists(peer, true)) {
                    command = new SqlCommand(
                    "INSERT INTO [Peers] (PeerName, PeerPassword, PeerIP, PeerPort, PeerIsOnline ) " +
                    "VALUES(@PeerName, @PeerPassword, @PeerIP, @PeerPort, @PeerIsOnline);", connection.DatabaseConnection);

                    //  Set parameters for the query.
                    command.Parameters.Add("@PeerName", System.Data.SqlDbType.Char, peer.Name.Length).Value = peer.Name;
                    command.Parameters.Add("@PeerPassword", System.Data.SqlDbType.Char, peer.Password.Length).Value = peer.Password;
                    command.Parameters.Add("@PeerIP", System.Data.SqlDbType.NVarChar, peer.Ip.Length).Value = peer.Ip;
                    command.Parameters.Add("@PeerPort", System.Data.SqlDbType.Int).Value = peer.Port;
                    command.Parameters.Add("@PeerIsOnline", System.Data.SqlDbType.Bit).Value = false;
                    //  Execute the query.
                    command.ExecuteNonQuery(); 
                }
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                connection.Close();
            }
        }

        /// <summary>
        /// Adds a file shared by a peer to the database.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="peer"></param>
        /// <param name="alreadyConnected"></param>
        public static void AddFile(DBFile file, DBPeer peer, bool alreadyConnected = false) {
            SqlCommand command = null;
            try {
                // Connect to the database.
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

                    //  Set parameters for the query.
                    command.Parameters.Add("@FileId", System.Data.SqlDbType.BigInt).Value = ++IdCounter;
                    command.Parameters.Add("@FileName", System.Data.SqlDbType.VarChar, file.Name.Length).Value = file.Name;
                    command.Parameters.Add("@FileSize", System.Data.SqlDbType.BigInt).Value = file.Size;
                    command.Parameters.Add("@PeerName", System.Data.SqlDbType.NVarChar, peer.Name.Length).Value = peer.Name;
                    //  Execute the query.
                    command.ExecuteNonQuery();
                }
                //  If file exists, just add the peer's name and the file's id to the File_Peer table.
                else {
                    //  Check if the File_Peer table already contains the file's id and the peer's name.
                    if (!FileExistsByPeer(fileId, peer.Name, true)) {
                        command = new SqlCommand(
                    "INSERT INTO [File_Peer] (FileId, PeerName) " +
                    "VALUES(@FileId, @PeerName)", connection.DatabaseConnection);

                        //  Set parameters for the query.
                        command.Parameters.Add("@FileId", System.Data.SqlDbType.BigInt).Value = fileId;
                        command.Parameters.Add("@PeerName", System.Data.SqlDbType.NVarChar, peer.Name.Length).Value = peer.Name;
                        //  Execute the query.
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

        /// <summary>
        /// Checks if a given file (name and size) exists, if it does, the method will return its id,
        /// otherwise, it will return -1.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="alreadyConnected"></param>
        /// <returns>long</returns>
        public static long FileExists(DBFile file, bool alreadyConnected = false) {
            bool isExist = false;
            SqlDataReader reader = null;
            try {
                // Connect to the database.
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT FileId " +
                "FROM [DataFiles] " +
                "WHERE FileName = @fileName " +
                "AND FileSize = @fileSize", connection.DatabaseConnection);

                //  Set parameters for the query.
                command.Parameters.Add("@fileName", System.Data.SqlDbType.NVarChar, file.Name.Length).Value = file.Name;
                command.Parameters.Add("@fileSize", System.Data.SqlDbType.BigInt).Value = file.Size;
                //  Execute the query.
                reader = command.ExecuteReader();
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

        /// <summary>
        /// Checks if a given peer is registered and if his password is correct.
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="alreadyConnected"></param>
        /// <returns>bool</returns>
        public static bool PeerExists(DBPeer peer, bool alreadyConnected = false) {
            bool isExist = false;
            SqlDataReader reader = null;
            try {
                // Connect to the database.
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [Peers] " +
                "WHERE PeerName = @peerName;", connection.DatabaseConnection);

                //  Set parameters for the query.
                command.Parameters.Add("@peerName", System.Data.SqlDbType.NVarChar).Value = peer.Name;
                //  Executes the query.
                reader = command.ExecuteReader();

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

        /// <summary>
        /// Checks if a given peer is sharing a given file.
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="peerName"></param>
        /// <param name="alreadyConnected"></param>
        /// <returns>bool</returns>
        public static bool FileExistsByPeer(long fileId, string peerName, bool alreadyConnected = false) {
            bool isExist = false;
            SqlDataReader reader = null;

            try {
                // Connect to the database.
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [File_Peer] " +
                "WHERE [dbo].[File_Peer].[FileId] = @fileId " +
                "AND [dbo].[File_Peer].[PeerName] = @peerName;", connection.DatabaseConnection);

                //  Set parameters for the query.
                command.Parameters.Add("@fileId", System.Data.SqlDbType.BigInt).Value = fileId;
                command.Parameters.Add("@peerName", System.Data.SqlDbType.NVarChar).Value = peerName;
                //  Execute the query.
                reader = command.ExecuteReader();
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

        /// <summary>
        /// Returns list of peers that have the given filename.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="alreadyConnected"></param>
        /// <returns>Dictionary</returns>
        public static Dictionary<DBFile, List<DBPeer>> SearchFiles(string fileName, bool alreadyConnected = false) {
            var searchFileResult = new Dictionary<DBFile, List<DBPeer>>();
            SqlDataReader reader = null;
            SqlCommand command = null;
            try {
                // Connect to the database.
                if (!alreadyConnected)
                    connection.Open();

                if (fileName.Equals("*")) {
                    command = new SqlCommand(
                   "SELECT files.FileId, files.FileName, files.FileSize, peers.PeerIP, peers.PeerPort, peers.PeerIsOnline " +
                   "FROM DataFiles AS files, Peers AS peers, File_Peer AS files_peers " +
                   "WHERE files_peers.FileId = files.FileId " +
                   "AND peers.PeerName = files_peers.PeerName " +
                   "ORDER BY files.FileName, files.FileSize;", connection.DatabaseConnection);
                }
                else {
                    command = new SqlCommand(
                   "SELECT files.FileId, files.FileName, files.FileSize, peers.PeerIP, peers.PeerPort, peers.PeerIsOnline " +
                   "FROM DataFiles AS files, Peers AS peers, File_Peer AS files_peers " +
                   "WHERE files_peers.FileId = files.FileId " +
                   "AND peers.PeerName = files_peers.PeerName " +
                   "AND files.FileName LIKE '%'+@fileName+'%' " +
                   "ORDER BY files.FileName, files.FileSize;", connection.DatabaseConnection);

                    //  Set parameters for the query.
                    command.Parameters.Add("@fileName", System.Data.SqlDbType.NVarChar, fileName.Length).Value = fileName;
                }

                //  Execute the query.
                reader = command.ExecuteReader();

                //  Reads the data from the query and saves it in a dictionary.
                while (reader.Read()) {
                    var file = new DBFile(reader.GetString(1), reader.GetInt64(2));
                    file.ID = reader.GetInt64(0);
                    var peer = new DBPeer("user", "password", reader.GetString(3), reader.GetInt32(4));
                    peer.IsOnline = reader.GetBoolean(5);
                    if (searchFileResult.ContainsKey(file))
                        searchFileResult[file].Add(peer);
                    else {
                        searchFileResult.Add(file, new List<DBPeer>());
                        searchFileResult[file].Add(peer);
                    }
                }
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

        /// <summary>
        /// Updates the peer's IP address and port in the database,
        /// when a peer connects to the main server.
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="alreadyConnected"></param>
        public static void LoginPeer(DBPeer peer, bool alreadyConnected = false) {
            lock (connection) {
                try {
                    // Connect to the database.
                    if (!alreadyConnected)
                        connection.Open();
                    SqlCommand command = null;
                    command = new SqlCommand(
                    "UPDATE Peers " +
                    "SET PeerIP = @PeerIP, PeerPort = @PeerPort " +
                    "WHERE PeerName = @PeerName " +
                    "AND PeerPassword = @PeerPassword;", connection.DatabaseConnection);

                    //  Set parameters for the query.
                    command.Parameters.Add("@PeerIP", System.Data.SqlDbType.NVarChar, peer.Ip.Length).Value = peer.Ip;
                    command.Parameters.Add("@PeerPort", System.Data.SqlDbType.Int).Value = peer.Port;
                    command.Parameters.Add("@PeerName", System.Data.SqlDbType.VarChar, peer.Name.Length).Value = peer.Name;
                    command.Parameters.Add("@PeerPassword", System.Data.SqlDbType.VarChar, peer.Password.Length).Value = peer.Password;
                    //  Execute the query.
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

        /// <summary>
        /// Sets the status of the given peer name (online or offline) with the given bool.
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="online"></param>
        /// <param name="alreadyConnected"></param>
        public static void SetPeerStatus(DBPeer peer, bool online, bool alreadyConnected = false) {
            lock (connection) {
                try {
                    // Connect to the database.
                    if (!alreadyConnected)
                        connection.Open();
                    SqlCommand command = null;
                    command = new SqlCommand(
                    "UPDATE Peers " +
                    "SET PeerIsOnline = @PeerIsOnline " +
                    "WHERE PeerName = @PeerName " +
                    "AND PeerPassword = @PeerPassword;", connection.DatabaseConnection);

                    //  Set the parameters for the query.
                    command.Parameters.Add("@PeerIsOnline", System.Data.SqlDbType.Bit, peer.Ip.Length).Value = online;
                    command.Parameters.Add("@PeerName", System.Data.SqlDbType.VarChar, peer.Name.Length).Value = peer.Name;
                    command.Parameters.Add("@PeerPassword", System.Data.SqlDbType.VarChar, peer.Password.Length).Value = peer.Password;
                    //  Execute the query.
                    command.ExecuteNonQuery();

                    //  If the user is disconnecting, remove the files he's sharing from the
                    //  File_Peer table.
                    if (!online) {
                        command = new SqlCommand(
                        "DELETE FROM File_Peer " +
                        "WHERE PeerName = @PeerName;", connection.DatabaseConnection);

                        //  Set the paramters for the query.
                        command.Parameters.Add("@PeerName", System.Data.SqlDbType.VarChar, peer.Name.Length).Value = peer.Name;
                    }

                    //  Execute the query.
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
        
        /// <summary>
        /// Returns all peers registered in the database.
        /// </summary>
        /// <param name="alreadyConnected"></param>
        /// <returns>List</returns>
        public static List<DBPeer> GetAllPeers(bool alreadyConnected = false) {
            List<DBPeer> peers = new List<DBPeer>();
            SqlDataReader reader = null;
            try {
                //  Connect to the database.
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [Peers];", connection.DatabaseConnection);

                //  Execute the query.
                reader = command.ExecuteReader();

                //  Reads all peers from the query's result and saves them in a list.
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

        /// <summary>
        /// Returns all files ever shared by users that connected to the main server.
        /// </summary>
        /// <param name="alreadyConnected"></param>
        /// <returns>List</returns>
        public static List<DBFile> GetAllFiles(bool alreadyConnected = false) {
            List<DBFile> files = new List<DBFile>();
            SqlDataReader reader = null;
            try {

                //  Connect to the database.
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT * " +
                "FROM [DataFiles];", connection.DatabaseConnection);

                //  Execute the query.
                reader = command.ExecuteReader();

                //  Reads all files from the query's result and saves them in a list.
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

        /// <summary>
        /// Removes a peer from the database, used mainly by the administrator.
        /// </summary>
        /// <param name="peerName"></param>
        /// <param name="alreadyConnected"></param>
        /// <returns></returns>
        public static bool DeletePeer(string peerName, bool alreadyConnected = false) {
            lock (connection) {
                try {
                    //  Connect to the database.
                    if (!alreadyConnected)
                        connection.Open();

                    SqlCommand command = new SqlCommand(
                        "DELETE FROM File_Peer " +
                        "WHERE PeerName = @peerName " +
                        "DELETE FROM Peers " +
                        "WHERE PeerName = @peerName;", connection.DatabaseConnection);

                    //  SEt parameters for the query.
                    command.Parameters.Add("@peerName", System.Data.SqlDbType.Char, peerName.Length).Value = peerName;
                    //  Execute the query.
                    command.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR: " + e.Message);
                    return false;
                }
                finally { connection.Close(); }
            }
        }

        /// <summary>
        /// Updates a given peer's properties.
        /// </summary>
        /// <param name="oldPeer"></param>
        /// <param name="newPeer"></param>
        /// <param name="alreadyConnected"></param>
        /// <returns></returns>
        public static bool UpdatePeer(DBPeer oldPeer, DBPeer newPeer, bool alreadyConnected = false) {
            //  Checks if administrator didn't change the peer's properties.
            if (oldPeer.Equals(newPeer))
                return true;

            lock (connection) {
                try {

                    //  Connect to the database.
                    if (!alreadyConnected)
                        connection.Open();

                    SqlCommand command = null;

                    // If the peers have a same name, update only in Peers table.
                    if (oldPeer.Name.Equals(newPeer.Name)) {
                        command = new SqlCommand(
                        "UPDATE Peers " +
                        "SET PeerPassword = @newPassword, " +
                        "PeerIP = @newIp, PeerPort = @newPort " +
                        "WHERE PeerName = @oldName;", connection.DatabaseConnection);
                    }
                    //  Else update Peers table and File_Peer too.
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
                    //  Set paramters for the query.
                    command.Parameters.Add("@newName", System.Data.SqlDbType.Char, newPeer.Name.Length).Value = newPeer.Name;
                    command.Parameters.Add("@newPassword", System.Data.SqlDbType.Char, newPeer.Password.Length).Value = newPeer.Password;
                    command.Parameters.Add("@newIp", System.Data.SqlDbType.Char, newPeer.Ip.Length).Value = newPeer.Ip;
                    command.Parameters.Add("@newPort", System.Data.SqlDbType.Int).Value = newPeer.Port;
                    command.Parameters.Add("@oldName", System.Data.SqlDbType.Char, oldPeer.Name.Length).Value = oldPeer.Name;
                    //  Execute the query.
                    command.ExecuteNonQuery();
                    SetPeerStatus(newPeer, newPeer.IsOnline, true);
                    return true;
                }
                catch (Exception e) {
                    Console.WriteLine("ERROR: " + e.Message);
                    return false;
                }
                finally { connection.Close(); }
            }
        }

        /// <summary>
        /// Checks if a given file (name and size) exists, if it does, the method will return its id,
        /// otherwise, it will return -1.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="alreadyConnected"></param>
        /// <returns>long</returns>
        public static bool CheckPeerConnected(DBPeer peer, bool alreadyConnected = false) {
            bool isExist = false;
            SqlDataReader reader = null;
            try {
                // Connect to the database.
                if (!alreadyConnected)
                    connection.Open();

                SqlCommand command = new SqlCommand(
                "SELECT PeerIsOnline " +
                "FROM Peers " +
                "WHERE PeerName = @PeerName " +
                "AND PeerPassword = @PeerPassword;", connection.DatabaseConnection);

                //  Set parameters for the query.
                command.Parameters.Add("@PeerName", System.Data.SqlDbType.Char, peer.Name.Length).Value = peer.Name;
                command.Parameters.Add("@PeerPassword", System.Data.SqlDbType.Char, peer.Password.Length).Value = peer.Password;
                //  Execute the query.
                reader = command.ExecuteReader();
                isExist = reader.Read();
                if (isExist)
                    return reader.GetBoolean(0);
            }
            catch (Exception e) {
                Console.WriteLine("ERROR: " + e.Message);
            }
            finally {
                reader.Close();
                if (!alreadyConnected)
                    connection.Close();
            }
            return false;
        }
    }
}