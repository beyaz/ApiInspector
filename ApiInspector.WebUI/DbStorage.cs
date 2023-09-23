using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace ApiInspector.WebUI;

static class DbStorage
{
    public static void Delete()
    {
        using (var sqlConnection = new SqlConnection(Config.DbStorage.ConnectionString))
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                sqlConnection.Open();
            }

            sqlConnection.ExecuteScalar("");


        }
    }
}