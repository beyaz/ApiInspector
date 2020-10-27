using System.Data;
using System.Data.SqlClient;

namespace ApiInspector.Application
{
    /// <summary>
    ///     The connection information
    /// </summary>
    static class ConnectionInfo
    {
        #region Public Methods
        /// <summary>
        ///     Gets the database connection.
        /// </summary>
        public static IDbConnection GetDbConnection()
        {
            return new SqlConnection("server=srvdev\\ATLAS;database =BOA;integrated security=true");
        }
        #endregion
    }
}