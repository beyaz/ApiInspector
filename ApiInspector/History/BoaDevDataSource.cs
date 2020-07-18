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
        ///     Removes the specified information.
        /// </summary>
        public void Remove(InvocationInfo info)
        {
            var model = CreateFrom(info);

            connection.Delete(model);
        }

        /// <summary>
        ///     Saves to history.
        /// </summary>
        public void SaveToHistory(InvocationInfo info)
        {
            var model = CreateFrom(info);

            Remove(info);

            connection.Insert(model);
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Creates from.
        /// </summary>
        RecordModel CreateFrom(InvocationInfo info)
        {
            var key = info.ToString();

            return new RecordModel
            {
                Key      = key,
                UserName = Environment.UserName,
                Value    = serializer.SerializeToJsonIgnoreDefaultValuesHandleObjectTypeNames(info)
            };
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
            [ExplicitKey]
            public string Key { get; set; }

            /// <summary>
            ///     Gets or sets the name of the user.
            /// </summary>
            [ExplicitKey]
            public string UserName { get; set; }

            /// <summary>
            ///     Gets or sets the value.
            /// </summary>
            public string Value { get; set; }
            #endregion
        }
    }
}