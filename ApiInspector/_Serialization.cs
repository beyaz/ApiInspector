using System.Collections.Generic;
using Newtonsoft.Json;

namespace ApiInspector
{
    static partial class _
    {
        static readonly List<JsonConverter> jsonConverters = new List<JsonConverter>();

        public static void AddJsonConverter(JsonConverter jsonConverter)
        {
            jsonConverters.Add(jsonConverter);
        }

        public static JsonSerializerSettings CreateJsonSerializerSettings()
        {
            return  new JsonSerializerSettings
            {
                DefaultValueHandling       = DefaultValueHandling.Ignore,
                Formatting                 = Formatting.Indented,
                DateFormatString           = "yyyy.MM.dd hh:mm:ss",
                Converters                 = jsonConverters,
                ReferenceLoopHandling      = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling =PreserveReferencesHandling.None,
                TypeNameHandling = TypeNameHandling.Auto
            };
        }
    }
}