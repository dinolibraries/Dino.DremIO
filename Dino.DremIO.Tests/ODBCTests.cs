using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dino.DremIO.Tests
{
    public class ODBCTests
    {
        public void ODBCConnectTest()
        {
            var connString = "DSN=DremioDSN;UID=your_user;PWD=your_password;";

            using var conn = new OdbcConnection(connString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM your_table LIMIT 10";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine(reader[0]);
            }
        }
    }
}
