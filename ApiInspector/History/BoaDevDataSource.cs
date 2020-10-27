using System;
using System.Collections.Generic;
using System.Linq;
using ApiInspector.Invoking.BoaSystem;
using ApiInspector.Models;
using ApiInspector.Serialization;
using Dapper;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using static  ApiInspector.Application.ConnectionInfo;

namespace ApiInspector.History
{
    /// <summary>
    ///     The boa dev data source
    /// </summary>
    class BoaDevDataSource
    {
        #region Fields
        /// <summary>
        ///     The environment variable
        /// </summary>
        readonly EnvironmentVariable environmentVariable;

        /// <summary>
        ///     The serializer
        /// </summary>
        readonly Serializer serializer;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSource" /> class.
        /// </summary>
        public BoaDevDataSource(EnvironmentVariable environmentVariable, Serializer serializer)
        {
            this.environmentVariable = environmentVariable ?? throw new ArgumentNullException(nameof(environmentVariable));
            this.serializer          = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets the history.
        /// </summary>
        public IReadOnlyList<InvocationInfo> GetHistory()
        {
            var userName = environmentVariable.GetUserName();

            var records = GetDbConnection().Query<RecordModel>($"SELECT * FROM DBT.ApiInspectorWhiteStone WITH (NOLOCK) WHERE UserName = @{nameof(userName)} ORDER BY LastExecutionTime DESC", new {userName});

            return records.Select(x => JsonConvert.DeserializeObject<InvocationInfo>(x.Value)).ToList();
        }

        /// <summary>
        ///     Removes the specified information.
        /// </summary>
        public void Remove(InvocationInfo info)
        {
            var model = CreateFrom(info);

            GetDbConnection().Delete(model);
        }

        /// <summary>
        ///     Saves to history.
        /// </summary>
        public void SaveToHistory(InvocationInfo info)
        {
            var model = CreateFrom(info);

            Remove(info);

            GetDbConnection().Insert(model);
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
                Key               = key,
                UserName          = environmentVariable.GetUserName(),
                Value             = serializer.SerializeToJsonIgnoreDefaultValuesHandleObjectTypeNames(info),
                LastExecutionTime = DateTime.Now
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
            ///     Gets or sets the last execution time.
            /// </summary>
            public DateTime? LastExecutionTime { get; set; }

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