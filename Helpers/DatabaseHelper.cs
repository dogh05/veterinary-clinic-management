using System.Data;
using System.Data.SqlClient;

namespace CliniqueVeterinaire.Helpers
{
    public static class DatabaseHelper
    {
        private static string connectionString = "Server=localhost\\SQLEXPRESS;Database=CliniqueVeto;Integrated Security=True;";

        public static IDbConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}