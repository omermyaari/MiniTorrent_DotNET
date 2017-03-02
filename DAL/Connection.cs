using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    class Connection
    {
        public string ConnectionString { get; private set; }
        public SqlConnection DatabaseConnection { get; private set; }
        public bool InProcess { get; set; }

        internal Connection(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public void Close()
        {
            if (!InProcess)
            {
                DatabaseConnection.Close();
                DatabaseConnection.Dispose();
            }
        }

        public void Open()
        {
            if (DatabaseConnection == null || DatabaseConnection.State != ConnectionState.Open)
            {
                DatabaseConnection = new SqlConnection(ConnectionString);
                DatabaseConnection.Open();
            }
        }
    }
}
