using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using ApiInspector.Models;
using ApiInspector.Serialization;
using Dapper;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;

namespace ApiInspector.History
{
    /// <summary>
    ///     The boa dev data source
    /// </summary>
    class BoaDevDataSource
    {
        #region Fields
        /// <summary>
        ///     The connection
        /// </summary>
        readonly IDbConnection connection;

        /// <summary>
        ///     The serializer
        /// </summary>
        readonly Serializer serializer = new Serializer();
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSource" /> class.
        /// </summary>
        public BoaDevDataSource()
        {
            const string ConnectionString = "server=srvdev\\ATLAS;database =BOA;integrated security=true";

            connection = new SqlConnection(ConnectionString);
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets the history.
        /// </summary>
        public IReadOnlyList<InvocationInfo> GetHistory()
        {
            var records = connection.Query<RecordModel>($"SELECT * FROM DBT.ApiInspectorWhiteStone WITH (NOLOCK) WHERE UserName = @{nameof(Environment.UserName)}", new {Environment.UserName});

            return records.Select(x => JsonConvert.DeserializeObject<InvocationInfo>(x.Value)).ToList();
        }

        /// <summary>
        ///     Saves to history.
        /// </summary>
        public void SaveToHistory(InvocationInfo info)
        {
            var key = info.ToString();

            var sql = $@"
DELETE FROM DBT.ApiInspectorWhiteStone 
 WHERE UserName = @{nameof(Environment.UserName)} 
   AND [Key] = @{nameof(key)}
                       ";

            connection.Execute(sql, new {Environment.UserName,key});

            var model = new RecordModel
            {
                Key      = key,
                UserName = Environment.UserName,
                Value    = serializer.SerializeToJsonIgnoreDefaultValuesHandleObjectTypeNames(info)
            };
            connection.Insert(model);
        }
        #endregion

        /// <summary>
        ///     The record model
        /// </summary>
        [Table("DBT.ApiInspectorWhiteStone")]
        class RecordModel
        {
            #region Public Properties
            /// <summary>
            ///     Gets or sets the key.
            /// </summary>
            public string Key { get; set; }

            /// <summary>
            ///     Gets or sets the name of the user.
            /// </summary>
            public string UserName { get; set; }

            /// <summary>
            ///     Gets or sets the value.
            /// </summary>
            public string Value { get; set; }
            #endregion
        }
    }
}