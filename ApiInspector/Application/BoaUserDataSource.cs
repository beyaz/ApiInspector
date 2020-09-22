using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace ApiInspector.Application
{
    /// <summary>
    ///     The boa user data source
    /// </summary>
    class BoaUserDataSource
    {
        #region Fields
        /// <summary>
        ///     The connection
        /// </summary>
        readonly IDbConnection connection;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="BoaUserDataSource" /> class.
        /// </summary>
        public BoaUserDataSource(ConnectionString connectionString)
        {
            connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            connection = new SqlConnection(connectionString.CurrentConnectionString);
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets the users.
        /// </summary>
        public IReadOnlyList<BoaUserModel> GetUsers(string search)
        {
            return connection.Query<BoaUserModel>($"SELECT [UserCode], [Name]  FROM COR.BoaUser WITH(NOLOCK) WHERE [Name] LIKE '%' + @{nameof(search)} + '%'", new {search}).ToList();
        }
        #endregion
    }

    /// <summary>
    ///     The user model
    /// </summary>
    class BoaUserModel
    {
        #region Public Properties
        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the user code.
        /// </summary>
        public string UserCode { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
        #endregion
    }
}