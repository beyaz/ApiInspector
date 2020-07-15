using System.Collections.Generic;
using System.IO;
using ApiInspector.Models;
using BOA.DataFlow;
using Newtonsoft.Json;

namespace ApiInspector.History
{
    static class HistoryManager
    {
        #region Properties
        static string DirectoryPath => Path.GetDirectoryName(typeof(HistoryManager).Assembly.Location) + Path.DirectorySeparatorChar + 
                                       nameof(ApiInspector) + "History" + Path.DirectorySeparatorChar;
        #endregion

        #region Public Methods
        public static IReadOnlyList<InvocationInfo> GetHistory(DataContext context)
        {

            var localItems = new List<InvocationInfo>();

            foreach (var file in Directory.GetFiles(DirectoryPath))
            {
                Utility.TryRun(() => localItems.Add(JsonConvert.DeserializeObject<InvocationInfo>(File.ReadAllText(file))));
            }

            return localItems;

        }

        public static void SaveToHistory(InvocationInfo info)
        {
            var filePath = Path.Combine(DirectoryPath, info.ToString().Replace(":", "____") + ".json");

            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            File.WriteAllText(filePath, SerializeToJson(info));
        }
        #endregion

        #region Methods
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