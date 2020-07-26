using System.Collections.Generic;
using System.IO;
using ApiInspector.Models;
using ApiInspector.Serialization;
using Newtonsoft.Json;

namespace ApiInspector.History
{
    /// <summary>
    ///     The data source
    /// </summary>
    class FileDataSource
    {
        #region Fields
        /// <summary>
        ///     The directory path
        /// </summary>
        readonly string directoryPath;

        /// <summary>
        ///     The serializer
        /// </summary>
        readonly Serializer serializer = new Serializer();
        #endregion

        #region Constructors
        /// <summary>
        ///     Initializes a new instance of the <see cref="DataSource" /> class.
        /// </summary>
        public FileDataSource()
        {
            directoryPath = @"d:\boa\server\bin\ApiInspectorHistory\";
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
        ///     Removes the specified information.
        /// </summary>
        public void Remove(InvocationInfo info)
        {
            var filePath = GetFilePath(info);
            File.Delete(filePath);
        }

        /// <summary>
        ///     Saves to history.
        /// </summary>
        public void SaveToHistory(InvocationInfo info)
        {
            var filePath = GetFilePath(info);

            Utility.WriteAllText(filePath, serializer.SerializeToJsonIgnoreDefaultValuesHandleObjectTypeNames(info));
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Gets the file path.
        /// </summary>
        string GetFilePath(InvocationInfo info)
        {
            return Path.Combine(directoryPath, Utility.CleanPath(info + ".json"));
        }
        #endregion
    }
}