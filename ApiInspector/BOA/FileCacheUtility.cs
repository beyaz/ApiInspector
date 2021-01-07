using System;
using System.IO;
using Newtonsoft.Json;

namespace ApiInspector.BOA
{
    /// <summary>
    ///     The file cache utility
    /// </summary>
    class FileCacheUtility
    {
        #region Public Methods
        /// <summary>
        ///     Tries the get.
        /// </summary>
        public static T TryGet<T>(Func<T> body, params object[] keyList)
        {
            var key = CalculateKey(keyList);

            var filePath = GetFilePath(key);

            if (File.Exists(filePath))
            {
                var jsonText = File.ReadAllText(filePath);

                return (T) JsonConvert.DeserializeObject(jsonText, typeof(T));
            }

            {
                var returnValue = body();

                var jsonText = SerializeToJson(returnValue);

                WriteAllText(filePath, jsonText);

                return returnValue;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Calculates the key.
        /// </summary>
        static string CalculateKey(object[] keyList)
        {
            return string.Join("-", keyList);
        }

        /// <summary>
        ///     Gets the file path.
        /// </summary>
        static string GetFilePath(string key)
        {
            return $@"d:\boa\server\bin\ApiInspectorConfiguration\Cache\{key}.json";
        }

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
                DateFormatString     = "yyyy.MM.dd hh:mm:ss"
            };

            return JsonConvert.SerializeObject(value, settings);
        }

        /// <summary>
        ///     Writes all text.
        /// </summary>
        static void WriteAllText(string filePath, string content)
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (directoryName == null)
            {
                throw new ArgumentNullException(nameof(directoryName));
            }

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            File.WriteAllText(filePath, content);
        }
        #endregion
    }
}