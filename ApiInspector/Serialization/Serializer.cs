using System;
using System.Collections.Generic;
using ApiInspector.Plugins;
using Newtonsoft.Json;

namespace ApiInspector.Serialization
{
    /// <summary>
    ///     The serializer
    /// </summary>
    class Serializer
    {
        #region Public Methods
        /// <summary>
        ///     Serializes to json.
        /// </summary>
        public static string SerializeToJson(object value)
        {
            return SerializeToJson(value, true);
        }

        /// <summary>
        ///     Serializes to json do not ignore default values.
        /// </summary>
        public static string SerializeToJsonDoNotIgnoreDefaultValues(object value)
        {
            return SerializeToJson(value, false);
        }

        /// <summary>
        ///     Clones the specified source.
        /// </summary>
        public T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        /// <summary>
        ///     Deserializes the specified json content.
        /// </summary>
        public static T Deserialize<T>(string jsonContent)
        {
            if (jsonContent == null)
            {
                throw new ArgumentNullException(nameof(jsonContent));
            }

            return JsonConvert.DeserializeObject<T>(jsonContent,CreateJsonSerializerSettings());
        }

        /// <summary>
        ///     Deserializes the specified json content.
        /// </summary>
        public static object Deserialize(string jsonContent, Type targetType)
        {
            if (jsonContent == null)
            {
                throw new ArgumentNullException(nameof(jsonContent));
            }

            return JsonConvert.DeserializeObject(jsonContent, targetType,CreateJsonSerializerSettings());
        }

        
        #endregion

        #region Methods
        /// <summary>
        ///     Serializes to json.
        /// </summary>
        static string SerializeToJson(object value, bool ignoreDefaultValues)
        {
            if (value == null)
            {
                return null;
            }

            var settings = CreateJsonSerializerSettings();
            
            settings.DefaultValueHandling = ignoreDefaultValues ? DefaultValueHandling.Ignore : DefaultValueHandling.Include;


            return JsonConvert.SerializeObject(value, settings);
        }

        public static JsonSerializerSettings CreateJsonSerializerSettings()
        {
            return  new JsonSerializerSettings
            {
                DefaultValueHandling       = DefaultValueHandling.Ignore,
                Formatting                 = Formatting.Indented,
                DateFormatString           = "yyyy.MM.dd hh:mm:ss",
                Converters                 = JsonConverters,
                ReferenceLoopHandling      = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling =PreserveReferencesHandling.None
            };
        }

        static IList<JsonConverter> JsonConverters
        {
            get
            {
                var list = new List<JsonConverter>
                {
                    new DecimalConverter(),
                    new Newtonsoft.Json.Converters.StringEnumConverter()
                };

                list.AddRange(Global.JsonConverters);

                return list;
            }
        } 

        #endregion
    }
}