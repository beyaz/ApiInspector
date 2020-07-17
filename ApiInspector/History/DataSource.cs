using System;
using System.Collections.Generic;
using System.IO;
using ApiInspector.Models;
using Newtonsoft.Json;

namespace ApiInspector.History
{
    /// <summary>
    ///     The data source
    /// </summary>
    class DataSource
    {
        #region Fields
        /// <summary>
        ///     The directory path
        /// </summary>
        readonly string directoryPath;
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSource" /> class.
        /// </summary>
        public DataSource()
        {
            directoryPath = Path.GetDirectoryName(typeof(DataSource).Assembly.Location) +
                            Path.DirectorySeparatorChar +
                            nameof(ApiInspector) + "History" +
                            Path.DirectorySeparatorChar;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSource" /> class.
        /// </summary>
        public DataSource(string directoryPath)
        {
            this.directoryPath = directoryPath ?? throw new ArgumentNullException(nameof(directoryPath));
        }
        #endregion

        #region Public Methods
        /// <summary>
        ///     Gets the history.
        /// </summary>
        public IReadOnlyList<InvocationInfo> GetHistory()
        {
            var localItems = new List<InvocationInfo>();

            if (!Directory.Exists(directoryPath))
            {
                return localItems;
            }

            foreach (var file in Directory.GetFiles(directoryPath))
            {
                Utility.TryRun(() => localItems.Add(JsonConvert.DeserializeObject<InvocationInfo>(File.ReadAllText(file))));
            }

            return localItems;
        }

        /// <summary>
        ///     Saves to history.
        /// </summary>
        public void SaveToHistory(InvocationInfo info)
        {
            var filePath = Path.Combine(directoryPath, info.ToString().Replace(":", "____") + ".json");

            Utility.WriteAllText(filePath, SerializeToJson(info));
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Serializes to json.
        /// </summary>
        static string SerializeToJson(object value)
        {
            if (value == null)
            {
                return null;
            }

            var settings = new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting           = Formatting.Indented,
                DateFormatString     = "yyyy.MM.dd hh:mm:ss",
                TypeNameHandling     = TypeNameHandling.Objects
            };

            return JsonConvert.SerializeObject(value, settings);
        }
        #endregion
    }
}